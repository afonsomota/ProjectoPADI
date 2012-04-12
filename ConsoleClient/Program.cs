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
                keys.Add(op.Key);
                Console.WriteLine(op);
            }


            TransactionContext tctx = ligacao.GetServers(keys);



            Console.WriteLine(tctx);


            Console.ReadLine();
        }
    }
    class ClientRemoting : MarshalByRefObject, IClient
    {
        public void GetNetworkUpdate(List<Node> network)
        {
            Console.WriteLine("\nNetwork Topology Update!");
            foreach (Node n in network)
            {
                Console.WriteLine(n);
            }
        }
    }
}
