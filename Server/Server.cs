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
using System.Runtime.Serialization;

namespace Server
{
    [Serializable]
    public struct TableValue
    {
        public string Value;
        public int Timestamp;
    } ;

    [Serializable]
    public class Semitable : Dictionary<string, List<TableValue>>, ISerializable
    {
        public uint MinInterval;
        public uint MaxInterval;

        public Semitable(uint min, uint max)
            : base()
        {
            MinInterval = min;
            MaxInterval = max;
        }

        protected Semitable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MinInterval = info.GetUInt32("a");
            MaxInterval = info.GetUInt32("b");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("a", MinInterval);
            info.AddValue("b", MaxInterval);
        }
    }

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
            //srv.InitializeSemitables(UInt32.MinValue, UInt32.MaxValue / 2);
            //srv.InitializeSemitables(UInt32.MaxValue / 2 + 1, UInt32.MaxValue);
            srv.Put("Afonso", "Teste1");
            //Console.WriteLine(srv.Get("Afonso"));
            srv.Put("Francisco", "OutroTeste");
            //Console.WriteLine(srv.Get("Francisco"));
            srv.Put("Ines","nãotenho");
            //Console.WriteLine(srv.Get("Ines"));
            srv.Put("Afonso", "Teste2");
            //Console.WriteLine(srv.Get("Afonso"));
            srv.Put("Ines", "continuo a nao ter!!!");
            //Console.WriteLine(srv.Get("Ines"));
            srv.Put("Afonso", "Eu tenho um Charizard!!!");
            srv.Put("Hitler", "I'm a PokeMaster!!!");
            //Console.WriteLine(srv.Get("Hitler"));
            srv.Put("Bernardo", "Eu tenho um Charizard!!! Por isso ganho-te!");
            //Console.WriteLine(srv.Get("Afonso",2));
            //Console.WriteLine(srv.Get("Afonso"));
            srv.Put("ola", "Também tenho coisas fixes");
            srv.Put("ole", "Que brilham no escuro e tal");
            srv.Put("oli", "E trolteiam");
            srv.Put("olu", "E catam coisas parvas");
            //srv.CleanTable(UInt32.MaxValue / 2, 0);
            srv.PrintSemiTables();



            Console.ReadLine();


        }
    }

    public class Server
    {
        public Node Info;
        public TcpChannel Channel;
        public List<Node> NetworkTopology;
        private Semitable[] Semitables;
        public int K;
        private int tableToInit = 0; 

        public Server(Node info, TcpChannel channel, int k)
        {
            Info = info;
            Channel = channel;
            K = k;
            Semitables = new Semitable[2];
        }

        public void InitializeSemitables(uint minST1, uint maxST1)
        {
            if (tableToInit < 2)
            {
                Semitables[tableToInit] = new Semitable(minST1, maxST1);
                Console.WriteLine("Initialized SemiTable: " + tableToInit.ToString());
                tableToInit ++;
            }
            else {
                Console.WriteLine("InitializeSemitables() called too many times");
            }
        }

        public void PrintSemiTables() {
            Console.Write("SemiTable 0: ");
            foreach(string key in Semitables[0].Keys){
                Console.Write(key+"; ");
            }
            Console.WriteLine("Min: " + Semitables[0].MinInterval + "; Max: " + Semitables[0].MaxInterval);
            Console.Write("SemiTable 1: ");
            foreach (string key in Semitables[1].Keys)
            {
                Console.Write(key + "; ");
            }
            Console.WriteLine("Min: " + Semitables[1].MinInterval + "; Max: " + Semitables[1].MaxInterval);
        }

        public void CopySemitables(Semitable st1, Semitable st2) {  
            Semitables = new Semitable[2];
            Semitables[0] = st1;
            Semitables[1] = st2;
            PrintSemiTables();
        }

        public void CleanTable(uint div,int side) {
            foreach (Semitable st in Semitables)
            {
                if (div >= st.MinInterval && div <= st.MaxInterval)
                {
                    Console.WriteLine("Cleaning Table: " + side.ToString());
                    List<string> keysToDelete = new List<string>();
                    foreach (string key in st.Keys) {
                        uint hash = MD5Hash(key);
                        if ((side == 0 && hash >= div) || (side == 1 && hash < div))
                        {
                            keysToDelete.Add(key);
                        }
                        if (side == 0) st.MaxInterval = div - 1;
                        if (side == 1) st.MinInterval = div;
                    }
                    foreach (string k in keysToDelete){
                        st.Remove(k);
                        Console.WriteLine("Removed Key: " + k);
                    }
                }
            }
            PrintSemiTables();
        }

        public Semitable[] DivideSemiTable(uint div) {
            Semitable[] new_semitables = new Semitable[2];
            foreach (Semitable st in Semitables){
                if (div >= st.MinInterval && div <= st.MaxInterval) {
                    new_semitables[0] = new Semitable(st.MinInterval,div-1);
                    new_semitables[1] = new Semitable(div, st.MaxInterval); 
                    foreach(string key in st.Keys){
                        uint hash =  MD5Hash(key);
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

        public static uint MD5Hash(string input) {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = x.ComputeHash(bs);
            uint interval = (uint)((hash[0] ^ hash[4] ^ hash[8] ^ hash[12]) << 24) +
                                   (uint)((hash[1] ^ hash[5] ^ hash[9] ^ hash[13]) << 16) +
                                  (uint)((hash[2] ^ hash[6] ^ hash[10] ^ hash[14]) << 8) +
                                  (uint)(hash[3] ^ hash[7] ^ hash[11] ^ hash[15]);
            return interval;
        }

        public Dictionary<uint, int> SemiTablesCount() {
            Dictionary<uint, int> semiCount = new Dictionary<uint, int>();
            foreach (Semitable st in Semitables) {
                List<uint> hashs = new List<uint>();
                foreach (string key in st.Keys) {
                    uint hash = MD5Hash(key);
                    hashs.Add(hash);
                }
                hashs.Sort();
                if (hashs.Count != 0)
                {
                    if (hashs.Count == 1) semiCount.Add(0, hashs.Count);
                    else semiCount.Add(hashs[hashs.Count / 2 + hashs.Count % 2], hashs.Count);
                }
            }
            return semiCount;
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

        public void Put(string key, string value)
        {
            foreach (Semitable st in Semitables)
            {
                if (st.ContainsKey(key))
                {
                    int max_timestamp = 0;
                    int min_timestamp = Int32.MaxValue;
                    TableValue min_tv = st[key][0];
                    foreach (TableValue tv in st[key])
                    {
                        if (tv.Timestamp > max_timestamp)
                            max_timestamp = tv.Timestamp;
                        else if (tv.Timestamp < min_timestamp)
                        {
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
                else
                {
                    uint hash = MD5Hash(key);
                    if (hash >= st.MinInterval && hash <= st.MaxInterval)
                    {
                        TableValue tv = new TableValue();
                        tv.Timestamp = 0;
                        tv.Value = value;
                        List<TableValue> values = new List<TableValue>();
                        values.Add(tv);
                        st.Add(key, values);
                        Console.WriteLine("Inserted Key: " + key);
                    }
                }
            }
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


        public void GetInitialIntervals(uint minST1, uint maxST1) {
            ctx.InitializeSemitables(minST1, maxST1);
        }

        public Dictionary<uint, int> GetSemiTablesCount()
        {
            return ctx.SemiTablesCount();
        }

        public void CleanSemiTable(uint semiTableToClean)
        {
            ctx.CleanTable(semiTableToClean,0);
        }

        public void CopyAndCleanTable(uint semiTableToClean, Node node)
        {
            Semitable[] tables = ctx.DivideSemiTable(semiTableToClean);
            ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
            Semitable st1 = tables[0];
            Semitable st2 = tables[1];
            link.CopyTable(st1,st2);
            ctx.CleanTable(semiTableToClean, 1);
        }

        public void CopyTable(Semitable st1, Semitable st2)
        {
            ctx.CopySemitables(st1, st2);
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
