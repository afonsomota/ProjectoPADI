using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonInterfaces;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net.Sockets;
using System.Threading;

namespace ConsoleClient
{
    class Program
    {

        static void Main(string[] args)
        {

            TcpChannel channel = new TcpChannel(0);
            ChannelServices.RegisterChannel(channel, true);

            ChannelDataStore channelData = (ChannelDataStore)channel.ChannelData;
            int port = new System.Uri(channelData.ChannelUris[0]).Port;
            string host = new System.Uri(channelData.ChannelUris[0]).Host;
            //string name = args[1];

            Node node = new Node(host, port, "Console", NodeType.Client);

            ICentralDirectory centralDirectory = (ICentralDirectory)Activator.GetObject(
              typeof(ICentralDirectory),
              "tcp://localhost:9090/CentralDirectory");
            centralDirectory.RegisterClient(node);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemoting), "Client", WellKnownObjectMode.Singleton);
           
            Console.Write("Enter seed: ");
            string seed = Console.ReadLine();

            string[] fIn = { "PUT 1" + seed + " 1", "GET 1" + seed, "PUT 2" + seed + " 2", "PUT Afonso" + seed + " A", "PUT Francisco" + seed + " F", "PUT Jerome" + seed + " J", "PUT JAmbrosio" + seed + " JA", "GET JAmbrosio" + seed, "PUT 3" + seed + " 3", "PUT 4" + seed + " 4", "GET 4" + seed };

            Transaction t = new Transaction();


           if (seed != "clean")
               foreach (string inp in fIn)
               {
                   if (inp.StartsWith("GET"))
                   {
                       char[] delim = { ' ', '\t' };
                       string[] arg = inp.Split(delim);
                       if (t.GetValue(arg[1]) == null)
                       {
                           Console.ReadLine();
                           return;
                       }
                   }
                   else if (inp.StartsWith("PUT"))
                   {
                       char[] delim = { ' ', '\t' };
                       string[] arg = inp.Split(delim);
                       if (!t.PutValue(arg[1], arg[2]))
                       {
                           Console.ReadLine();
                           return;
                       }
                   }
               }
           else Console.WriteLine("No default operations were made.");

            Console.WriteLine("Choose you're operations\n\nPUT <key> <value> for a put\nGET <key> for a GET\nEmpty Line to process");
            string input="";
            do
            {
                input = Console.ReadLine();
                if (input.StartsWith("GET"))
                {
                    char[] delim = {' ','\t'};
                    string val = null;
                    string[] arg = input.Split(delim);
                    val = t.GetValue(arg[1]);
                    if (val ==  null) {
                        Console.ReadLine();
                        return;
                    }
                    Console.WriteLine(val);
                }
                else if(input.StartsWith("PUT")) {
                    char[] delim = {' ','\t'};
                    string[] arg = input.Split(delim);
                    if (!t.PutValue(arg[1], arg[2]))
                    {
                        Console.ReadLine();
                        return;
                    }
                }

            } while (input != "");

            t.Commit();

            Console.ReadLine();
        }


        
    }


    class Transaction
    {

        List<string> AccessedKeys;
        Dictionary<string, List<Node>> NodesLocation;
        List<Node> Nodes;
        TransactionContext Tctx;
        ICentralDirectory Central;
        bool readOnlyTransaction = false;

        public Transaction()
        {
            AccessedKeys = new List<string>();
            NodesLocation = new Dictionary<string, List<Node>>();
            Nodes = new List<Node>();
            Central = (ICentralDirectory)Activator.GetObject(
              typeof(ICentralDirectory),
              "tcp://localhost:9090/CentralDirectory");
            while (true)
            {
                Tctx = Central.BeginTx();
                if (Tctx.Txid != -1)
                    break;

                Thread.Sleep(500);

            }
            Console.WriteLine(Tctx);

        }

        List<Node> GetAndLockKey(string key)
        {
            List<Node> nodes = Central.GetServers(key);
            foreach (Node n in nodes)
                if (!Nodes.Contains(n))
                    Nodes.Add(n);
            NodesLocation.Add(key, nodes);
            bool allCanLock = true;
            bool allLocked = true;
            int serversDown = 0;
            foreach (Node n in nodes)
            {
                try
                {
                    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                    char canLock = server.CanLock(Tctx.Txid, key);
                    Console.WriteLine("CanLock returned: " + canLock);
                    if (canLock == 'n') allCanLock = false;
                    else if (canLock == 'r') readOnlyTransaction = true;
                }
                catch
                {
                    serversDown++;
                    Central.ServerDown(n);
                }
            }
            if (!allCanLock || serversDown == 2)
            {
                Abort();
                return null;
            }
            else if (!readOnlyTransaction)
            {
                serversDown = 0;
                foreach (Node n in nodes)
                {
                    try
                    {
                        IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                        if (!server.Lock(Tctx.Txid, key))
                        {
                            allLocked = false;
                            break;
                        }
                    }
                    catch
                    {
                        serversDown++;
                        Central.ServerDown(n);
                    }
                }
                if (!allLocked || serversDown == 2)
                {
                    Abort();
                    return null;
                }
            }
            return nodes;
        }

        public string GetValue(string key)
        {
            List<Node> nodes = null;
            if (AccessedKeys.Contains(key))
            {
                nodes = NodesLocation[key];
            }
            else
            {
                nodes = GetAndLockKey(key);
                AccessedKeys.Add(key);
            }
            if (nodes != null)
            {
                IServer serv = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[0].IP + ":" + nodes[0].Port.ToString() + "/Server");
                string value;
                try
                {
                    if (readOnlyTransaction) value = serv.GetStable(key);
                    else value = serv.Get(Tctx.Txid, key);
                }
                catch
                {
                    IServer servBackup = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[1].IP + ":" + nodes[1].Port.ToString() + "/Server");
                    try
                    {
                        if (readOnlyTransaction) value = serv.GetStable(key);
                        else value = serv.Get(Tctx.Txid, key);
                        Central.ServerDown(nodes[0]);
                    }
                    catch
                    {
                        value = null;
                    }
                }
                if (value == null)
                {
                    Abort();
                    return null;
                }
                else return value;
            }
            else return null;
        }

        public bool PutValue(string key, string value)
        {
            if (readOnlyTransaction)
            {
                Abort();
                return false;
            }
            List<Node> nodes = null;
            if (AccessedKeys.Contains(key))
            {
                nodes = NodesLocation[key];
            }
            else
            {
                nodes = GetAndLockKey(key);
                AccessedKeys.Add(key);
            }
            if (nodes != null)
            {
                IServer serv = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[0].IP + ":" + nodes[0].Port.ToString() + "/Server");
                bool success = true;
                try
                {
                    success = serv.Put(Tctx.Txid, key, value);
                }
                catch
                {
                    IServer servBackup = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[1].IP + ":" + nodes[1].Port.ToString() + "/Server");
                    try
                    {
                        success = servBackup.Put(Tctx.Txid, key, value) && success;
                        Central.ServerDown(nodes[0]);
                    }
                    catch
                    {
                        success = false;
                    }
                }
                if (!success)
                {
                    Abort();
                    return false;
                }
                else return success;
            }
            else return false;
        }

        public bool Commit()
        {
            bool allCanCommit = true;
            Node nodeToRemove = null;
            int serversDown = 0;
            foreach (Node n in Nodes)
            {
                try
                {
                    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                    if (!server.CanCommit(Tctx.Txid))
                    {
                        allCanCommit = false;
                        break;
                    }
                }
                catch
                {
                    serversDown++;
                    nodeToRemove = n;
                    Central.ServerDown(n);
                }
            }
            if (nodeToRemove != null) Nodes.Remove(nodeToRemove);
            Tctx.State = TransactionContext.states.tentatively;
            Console.WriteLine(Tctx);
            if (!allCanCommit || serversDown == 2)
            {
                Abort();
                return false;
            }
            else
            {
                foreach (Node n in Nodes)
                {
                    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                    server.Commit(Tctx.Txid);
                }
                Tctx.State = TransactionContext.states.commited;
                Central.UpdateTransactionState(Tctx);
                Console.WriteLine(Tctx);
                return true;
            }
        }

        public void Abort()
        {
            foreach (Node n in Nodes)
            {
                try
                {
                    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                    server.Abort(Tctx.Txid);
                }
                catch
                {
                    Central.ServerDown(n);
                }
            }
            Tctx.State = TransactionContext.states.aborted;
            Central.UpdateTransactionState(Tctx);
            Console.WriteLine(Tctx);

        }

    }


    class ClientRemoting : MarshalByRefObject, IClient
    {
        public void GetNetworkUpdate(List<Node> network)
        {
            /*Console.WriteLine("\nNetwork Topology Update!");
            foreach (Node n in network)
            {
                Console.WriteLine(n);
            }*/
        }
    }
}
