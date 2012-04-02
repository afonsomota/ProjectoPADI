using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using CommonInterfaces;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;

namespace Server
{
    public struct TableValue
    {
        public string Value;
        public int Timestamp;
    } ;

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
            Server srv = new Server(node, channel,5);
            ServerPuppet.ctx = srv;
            ServerRemoting.ctx = srv;
            ligacao.RegisterPseudoNode(node);
            System.Console.WriteLine(host + ":" + port.ToString());
            System.Console.ReadLine();
            srv.InitializeSemitables(UInt32.MinValue, UInt32.MaxValue / 2, UInt32.MaxValue / 2 + 1, UInt32.MaxValue);
            srv.Put("Afonso", "Teste1");
            Console.WriteLine(srv.Get("Afonso"));
            srv.Put("Francisco", "OutroTeste");
            Console.WriteLine(srv.Get("Francisco"));
            srv.Put("Ines","nãotenho");
            Console.WriteLine(srv.Get("Ines"));
            srv.Put("Afonso", "Teste2");
            Console.WriteLine(srv.Get("Afonso"));
            srv.Put("Ines", "continuo a nao ter!!!");
            Console.WriteLine(srv.Get("Ines"));
            srv.Put("Afonso", "Eu tenho um Charizard!!!");
            srv.Put("Hitler", "I'm a PokeMaster!!!");
            Console.WriteLine(srv.Get("Hitler"));
            srv.Put("Afonso", "Eu tenho um Charizard!!! Por isso ganho-te!");
            Console.WriteLine(srv.Get("Afonso",2));
            Console.WriteLine(srv.Get("Afonso"));
            srv.Put("Afonso", "Também tenho coisas fixes");
            srv.Put("Afonso", "Que brilham no escuro e tal");
            srv.Put("Afonso", "E trolteiam");
            srv.Put("Afonso", "E catam coisas parvas");
            Dictionary<string, List<TableValue>> all = srv.GetAll();
            foreach (string key in all.Keys) {
                Console.Write(key + ": ");
                foreach (TableValue tv in all[key]) {
                    Console.Write(tv.Value + " (" + tv.Timestamp + "); ");
                }
                Console.WriteLine();
            }
            Console.ReadLine();

        }
    }

    public class Semitable : Dictionary<string, List<TableValue>>
    {
        public uint MinInterval;
        public uint MaxInterval;

        public Semitable(uint min, uint max): base() {
            MinInterval = min;
            MaxInterval = max;
        }
    }

    public class Server
    {
        public Node Info;
        public TcpChannel Channel;
        public List<Node> NetworkTopology;
        private Semitable[] Semitables;
        public int K;

        public Server(Node info, TcpChannel channel, int k)
        {
            Info = info;
            Channel = channel;
            K = k;
        }

        public void InitializeSemitables(uint minST1, uint maxST1, uint minST2, uint maxST2)
        {
            Semitables = new Semitable[2];
            Semitables[0] = new Semitable(minST1, maxST1);
            Semitables[1] = new Semitable(minST2, maxST2);
        }

        public void CopySemitables(Semitable st1, Semitable st2) {  
            Semitables = new Semitable[2];
            Semitables[0] = st1;
            Semitables[1] = st2;
        }

        public void CleanTable(uint div,int side) {
            foreach (Semitable st in Semitables) {
                if (div >= st.MinInterval && div <= st.MaxInterval) {
                    foreach (string key in st.Keys) {
                        uint hash = SHA1Hash(key);
                        if ((side == 0 && hash >= div) || (side == 1 && hash < div))
                        {
                            st.Remove(key);
                            Console.WriteLine("Removed Key: " + key);
                        }
                        if (side == 0) st.MaxInterval = div - 1;
                        if (side == 1) st.MinInterval = div;
                    }
                }
            }
        }

        public Semitable[] DivideSemiTable(uint div) {
            Semitable[] new_semitables = new Semitable[2];
            foreach (Semitable st in Semitables){
                if (div >= st.MinInterval && div <= st.MaxInterval) {
                    new_semitables[0] = new Semitable(st.MinInterval,div-1);
                    new_semitables[1] = new Semitable(div, st.MaxInterval); 
                    foreach(string key in st.Keys){
                        uint hash = SHA1Hash(key);
                        if (hash < div){
                            new_semitables[0].Add(key, st[key]);
                        }
                        else {
                            new_semitables[1].Add(key, st[key]);
                        }
                    }
                }
            }
            return new_semitables;
        }

        public static uint SHA1Hash(string input)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] data = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = sha.ComputeHash(data);
            uint interval = (uint)((hash[0] ^ hash[4] ^ hash[8] ^ hash[12] ^ hash[16]) << 24) +
                                   (uint)((hash[1] ^ hash[5] ^ hash[9] ^ hash[13] ^ hash[17]) << 16) +
                                  (uint)((hash[2] ^ hash[6] ^ hash[10] ^ hash[14] ^ hash[18]) << 8) +
                                  (uint)(hash[3] ^ hash[7] ^ hash[11] ^ hash[15] ^ hash[19]);
            return interval;
        }

        public string Get(string key, int timestamp) {
            foreach (Semitable st in Semitables)
                if(st.ContainsKey(key))
                    foreach (TableValue tv in st[key]) 
                        if (tv.Timestamp == timestamp)
                            return tv.Value;
            return null;
        }

        public string Get(string key)
        {
            foreach (Semitable st in Semitables)
                if (st.ContainsKey(key)){
                    int max_timestamp = 0;
                    TableValue max_tv = st[key][0];
                    foreach (TableValue tv in st[key]){
                        if (tv.Timestamp > max_timestamp){
                            max_timestamp = tv.Timestamp;
                            max_tv = tv;
                        }
                    }
                    return max_tv.Value;
                }
            return null;
        }

        public Dictionary<string, List<TableValue>> GetAll() { 

            Dictionary<string, List<TableValue>> all = new Dictionary<string, List<TableValue>>();
            foreach (Semitable st in Semitables)
            { 
                foreach(string key in st.Keys){
                    all.Add(key, st[key]);
                }
            }
            return all;
        }

        public string Put(string key, string value)
        {
            foreach (Semitable st in Semitables)
                if (st.ContainsKey(key))
                {
                    int max_timestamp = 0;
                    int min_timestamp = Int32.MaxValue;
                    TableValue min_tv = st[key][0];
                    foreach (TableValue tv in st[key]){
                        if (tv.Timestamp > max_timestamp)
                            max_timestamp = tv.Timestamp;
                        else if (tv.Timestamp < min_timestamp){
                            min_timestamp = tv.Timestamp;
                            min_tv = tv;
                        }
                    }
                    if (st[key].Count == K)
                            st[key].Remove(min_tv);
                    TableValue newtv = new TableValue();
                    newtv.Timestamp = max_timestamp + 1;
                    newtv.Value = value;
                    st[key].Add(newtv);
                }
                else {
                    uint hash = SHA1Hash(key);
                    if (hash >= st.MinInterval && hash <= st.MaxInterval){
                        TableValue tv = new TableValue();
                        tv.Timestamp = 0;
                        tv.Value = value;
                        List<TableValue> values = new List<TableValue>();
                        values.Add(tv);
                        st.Add(key, values);
                    }
                }
            return null;
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


        public void GetInitialIntervals(uint minST1, uint maxST1, uint minST2, uint maxST2) {
            ctx.InitializeSemitables(minST1, maxST1, minST2, maxST2);
        }

        public Dictionary<uint, int> GetSemiTablesCount()
        {
            throw new NotImplementedException();
        }

        public void CleanSemiTable(uint semiTableToClean)
        {
            ctx.CleanTable(semiTableToClean,0);
        }

        public void CopyAndCleanTable(uint semiTableToClean, Node node)
        {
            Semitable[] tables = ctx.DivideSemiTable(semiTableToClean);
            ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
            link.CopyTable(tables[0],tables[1]);
            ctx.CleanTable(semiTableToClean, 1);
        }

        public void CopyTable(Semitable st1, Semitable st2)
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
            ligacao.RegisterServer(ctx.Info);
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
