using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using CommonInterfaces;
using System.Threading;

namespace Server
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

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServerPuppet), "ServerPuppet", WellKnownObjectMode.Singleton);
            IPuppetMaster ligacao = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
               "tcp://localhost:8090/PseudoNodeReg");
            Node node = new Node(host, port, NodeType.Server);
            Server srv = new Server(node, channel);
            ServerPuppet.ctx = srv;
            ServerRemoting.ctx = srv;
            ligacao.RegisterPseudoNode(node);
            System.Console.WriteLine(host + ":" + port.ToString());
            System.Console.ReadLine();
        }
    }

    public class Server
    {
        public Node Info;
        public TcpChannel Channel;
        public List<Node> NetworkTopology;

        public Server(Node info, TcpChannel channel)
        {
            Info = info;
            Channel = channel;
        }
    }

    public class ServerRemoting: MarshalByRefObject, IServer{
        public static Server ctx;

        public void GetNetworkUpdate(List<Node> network)
        {
            ctx.NetworkTopology = network;
            Console.WriteLine("\nNetwork Topology Update!");
            foreach (Node n in ctx.NetworkTopology)
            {
                Console.WriteLine(n);
            }
        }

        public Dictionary<int, int> GetSemiTablesCount()
        {
            throw new NotImplementedException();
        }

        public void CleanSemiTable(int semiTableToClean)
        {
            throw new NotImplementedException();
        }

        public void CopyAndCleanTable(int semiTableToClean)
        {
            throw new NotImplementedException();
        }

        public void CopyTable()
        {
            throw new NotImplementedException();
        }

        public bool CanLock(int txid, List<string> keys)
        {
            throw new NotImplementedException();
        }

        public bool Lock(int txid)
        {
            throw new NotImplementedException();
        }

        public string Get(int txid, string key)
        {
            throw new NotImplementedException();
        }

        public string Put(int txid, string key, string new_value)
        {
            throw new NotImplementedException();
        }

        public bool Abort(int txid)
        {
            throw new NotImplementedException();
        }

        public bool CanCommit(int txid)
        {
            throw new NotImplementedException();
        }

        public bool Commit(int txid)
        {
            throw new NotImplementedException();
        }
    }

    class ServerPuppet : MarshalByRefObject, IServerPuppet
    {
        public static Server ctx;

        public void StartServer()
        {
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServerRemoting), "Server", WellKnownObjectMode.Singleton);
            ICentralDirectory ligacao = (ICentralDirectory)Activator.GetObject(
               typeof(ICentralDirectory),
               "tcp://localhost:9090/CentralDirectory");
            ligacao.RegisterClient(ctx.Info);
            Console.WriteLine("Server Online");
        }


        public void KillServer()
        {
            ThreadStart ts = delegate() { KillServerThread(); };
            Thread t = new Thread(ts);
            t.Start();
        }

        public void KillServerThread()
        {
            ChannelServices.UnregisterChannel(ctx.Channel);
            ctx.Channel = new TcpChannel(ctx.Info.Port);
            ChannelServices.RegisterChannel(ctx.Channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServerPuppet), "ServerPuppet", WellKnownObjectMode.Singleton);
            Console.WriteLine("Server Offline");
        }
    }
}
