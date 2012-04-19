using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonInterfaces;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace ConsoleClient
{
    class Program
    {

        static Dictionary<Node, List<Operation>> InvertNodesLocation(Dictionary<string, List<Node>> nodesLocation, Dictionary<int,Operation> operations)
        {
            Dictionary<Node, List<Operation>> serversKeys = new Dictionary<Node, List<Operation>>();
            foreach (KeyValuePair<string, List<Node>> servers in nodesLocation)
            {
                foreach (Node serv in servers.Value)
                {
                    bool servExists = false;
                    Node existingServ = null;
                    foreach (Node s in serversKeys.Keys) {
                        if (s == serv) { 
                            servExists = true;
                            existingServ = s;
                            break;
                        }
                    }
                    if (!servExists)
                    {
                        List<Operation> new_list = new List<Operation>();
                        foreach(Operation op in operations.Values){
                            if (servers.Key == op.Key)
                                new_list.Add(op);
                        }
                        serversKeys.Add(serv, new_list);
                        existingServ = serv;
                    }
                    else foreach (Operation op in operations.Values)
                        {
                            if (servers.Key == op.Key)
                            {
                                if (!serversKeys[existingServ].Contains(op))
                                    serversKeys[existingServ].Add(op);
                                else break;
                            } 
                        } 
                }
            }
            return serversKeys;
        }

        static void Main(string[] args)
        {

            TcpChannel channel = new TcpChannel(0);
            ChannelServices.RegisterChannel(channel, true);

            ChannelDataStore channelData = (ChannelDataStore)channel.ChannelData;
            int port = new System.Uri(channelData.ChannelUris[0]).Port;
            string host = new System.Uri(channelData.ChannelUris[0]).Host;
            string name = args[1];

            Node node = new Node(host, port, name, NodeType.Client);


            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemoting), "Client", WellKnownObjectMode.Singleton);
            ICentralDirectory ligacao = (ICentralDirectory)Activator.GetObject(
               typeof(ICentralDirectory),
               "tcp://localhost:9090/CentralDirectory");
            ligacao.RegisterClient(node);
            Console.Write("Enter seed: ");
            string seed = Console.ReadLine();

            List<Operation> ops = new List<Operation>();

            string[] fIn = { "PUT 1" + seed + " 1", "GET 1" + seed, "PUT 2" + seed + " 2", "PUT Afonso" + seed + " A", "PUT Francisco" + seed + " F", "PUT Jerome" + seed + " J", "PUT JAmbrosio" + seed + " JA", "GET JAmbrosio" + seed, "PUT 3" + seed + " 3", "PUT 4" + seed + " 4", "GET 4" + seed };


            foreach (string inp in fIn)
            {
                if (inp.StartsWith("GET"))
                {
                    char[] delim = {' ','\t'};
                    string[] arg = inp.Split(delim);
                    ops.Add(new Operation(arg[1]));
                }
                else if (inp.StartsWith("PUT"))
                {
                    char[] delim = {' ','\t'};
                    string[] arg = inp.Split(delim);
                    ops.Add(new Operation(arg[1], arg[2]));
                }
            }

            Console.WriteLine("Choose you're operations\n\nPUT <key> <value> for a put\nGET <key> for a GET\nEmpty Line to process");
            string input="";
            do
            {
                input = Console.ReadLine();
                if (input.StartsWith("GET"))
                {
                    char[] delim = {' ','\t'}; 
                    string[] arg = input.Split(delim);
                    ops.Add(new Operation(arg[1]));
                }
                else if(input.StartsWith("PUT")) {
                    char[] delim = {' ','\t'}; 
                    string[] arg = input.Split(delim);
                    ops.Add(new Operation(arg[1], arg[2]));
                }

            } while (input != "");


            List<string> keys = new List<string>();

            foreach (Operation op in ops) {
                if(!keys.Contains(op.Key)) keys.Add(op.Key);
                Console.WriteLine(op);
            }


            TransactionContext tctx = ligacao.GetServers(ops);

            Console.WriteLine(tctx);

            Dictionary<Node,List<Operation>> serversKeys = InvertNodesLocation(tctx.NodesLocation,tctx.Operations);

            foreach(KeyValuePair<Node,List<Operation>> pair in serversKeys){
                Console.Write("For node " + pair.Key + ": ");
                foreach (Operation op in pair.Value) {
                    Console.WriteLine(op + "; ");
                }
                Console.WriteLine();
            }

            Dictionary<Node, bool> canLockValues = new Dictionary<Node, bool>();
            foreach (Node serv in serversKeys.Keys) {
                IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + serv.IP + ":" + serv.Port.ToString() + "/Server");
                bool b1 = link.CanLock(tctx.Txid,serversKeys[serv]);
                canLockValues.Add(serv, b1);
            }

            bool allCanLock = true;
            foreach (bool b in canLockValues.Values) {
                if (!b) {
                    allCanLock = false;
                    break;
                }
            }
            if (!allCanLock) {
                foreach (Node n in canLockValues.Keys) {
                    if (!canLockValues[n]) {
                        IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                        link.Abort(tctx.Txid);
                        Console.WriteLine("Transaction Aborted");
                        return;
                    }
                }
            }

            foreach (Node n in canLockValues.Keys) {
                IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                link.Lock(tctx.Txid);
            }



            for (int i = 1; i <= tctx.Operations.Count; i++) { 
                Operation op = tctx.Operations[i];
                Node srvToSend = tctx.NodesLocation[op.Key][0];
                IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + srvToSend.IP + ":" + srvToSend.Port.ToString() + "/Server");
                if (op.Type == OpType.GET){
                    Console.WriteLine("GET(" + op.Key + ") = " + link.Get(tctx.Txid, op.Key) + " on server " + srvToSend.IP + ":" + srvToSend.Port.ToString());
                }
                else {
                    link.Put(tctx.Txid, op.Key, op.Value);
                }
            }

            Dictionary<Node, bool> canCommitValues = new Dictionary<Node, bool>();
            foreach (Node serv in serversKeys.Keys)
            {
                IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + serv.IP + ":" + serv.Port.ToString() + "/Server");
                bool b1 = link.CanCommit(tctx.Txid);
                canCommitValues.Add(serv, b1);
            }

            bool allCanCommit = true;
            foreach (bool b in canCommitValues.Values)
            {
                if (!b)
                {
                    allCanCommit = false;
                    break;
                }
            }
            if (!allCanCommit)
            {
                foreach (Node n in canCommitValues.Keys)
                {
                    if (!canCommitValues[n])
                    {
                        IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                        link.Abort(tctx.Txid);
                        Console.WriteLine("Transaction Aborted");
                        return;
                    }
                }
            }

            foreach (Node n in canCommitValues.Keys)
            {
                IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                link.Commit(tctx.Txid);
            }



            

            Console.ReadLine();
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
