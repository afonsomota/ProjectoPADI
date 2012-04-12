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
        static void Main(string[] args)
        {

            TcpChannel channel = new TcpChannel(0);
            ChannelServices.RegisterChannel(channel, true);

            ChannelDataStore channelData = (ChannelDataStore)channel.ChannelData;
            int port = new System.Uri(channelData.ChannelUris[0]).Port;
            string host = new System.Uri(channelData.ChannelUris[0]).Host;


            Node node = new Node(host, port, NodeType.Client);


            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemoting), "Client", WellKnownObjectMode.Singleton);
            ICentralDirectory ligacao = (ICentralDirectory)Activator.GetObject(
               typeof(ICentralDirectory),
               "tcp://localhost:9090/CentralDirectory");
            ligacao.RegisterClient(node);
            Console.WriteLine("Client Online");

            List<Operation> ops = new List<Operation>();

            string[] fIn = { "PUT 1 1", "GET 1", "PUT 2 2", "PUT Afonso A", "PUT Francisco F", "PUT Jerome J", "PUT JAmbrosio JA","GET JAmbrosio","PUT 3 3","PUT 4 4","GET 4"};


            foreach (string inp in fIn)
            {
                if (inp.StartsWith("GET"))
                {
                    char[] delim = { ' ', '\t' };
                    string[] arg = inp.Split(delim);
                    ops.Add(new Operation(arg[1]));
                }
                else if (inp.StartsWith("PUT"))
                {
                    char[] delim = { ' ', '\t' };
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
