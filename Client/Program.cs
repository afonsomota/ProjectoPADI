using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonInterfaces;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Collections;
using System.Threading;

namespace Client
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



            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientPuppet), "ClientPuppet", WellKnownObjectMode.Singleton);
            IPuppetMaster ligacao = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
               "tcp://localhost:8090/PseudoNodeReg");
            Node node = new Node(host, port, NodeType.Client);
            Client clt = new Client(node,channel);
            ClientPuppet.ctx = clt;

            ligacao.RegisterPseudoNode(node);
            System.Console.WriteLine(host + ":" + port.ToString());
            System.Console.ReadLine();

            
        }
    }

    class Client {
        public string[] Registers;
        public Node Info;
        public TcpChannel Channel;

        public Client(Node info,TcpChannel channel){
            Registers = new string[10];
            Info = info;
            Channel = channel;
        }

        public void StoreValue(int reg, string value){
            Registers[reg] = value;
        }

    }

    class ClientRemoting : MarshalByRefObject, IClient
    {
        public void GetNetworkUpdate(List<Node> network) { 
            //TODO
        }
    }

    class ClientPuppet : MarshalByRefObject, IClientPuppet
    {
        public static Client ctx;

        public void StartClient()
        {
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemoting), "ClientRemoting", WellKnownObjectMode.Singleton);

            Console.WriteLine("Client Online");
        }


        public void KillClient()
        {
            ThreadStart ts = delegate() { KillClientThread(); };
            Thread t = new Thread(ts);
            t.Start();
        }

        public void KillClientThread()
        {
            ChannelServices.UnregisterChannel(ctx.Channel);
            ctx.Channel = new TcpChannel(ctx.Info.Port);
            ChannelServices.RegisterChannel(ctx.Channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemoting), "ClientRemoting", WellKnownObjectMode.Singleton);
            Console.WriteLine("Client Offline");
        }
    }

}
