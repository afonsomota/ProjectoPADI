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
        public Node Replica;

        public Semitable(uint min, uint max, Node replica)
            : base()
        {
            MinInterval = min;
            MaxInterval = max;
            Replica = replica;
        }

        protected Semitable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MinInterval = info.GetUInt32("a");
            MaxInterval = info.GetUInt32("b");
            Replica = (Node) info.GetValue("n", typeof(Node));
        }

        
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("a", MinInterval);
            info.AddValue("b", MaxInterval);
            info.AddValue("n",Replica);
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
            /*//srv.InitializeSemitables(UInt32.MinValue, UInt32.MaxValue / 2);
            //srv.InitializeSemitables(UInt32.MaxValue / 2 + 1, UInt32.MaxValue);
            srv.Put(0,"Afonso", "Teste1");
            //Console.WriteLine(srv.Get("Afonso"));
            srv.Put(0, "Francisco", "OutroTeste");
            //Console.WriteLine(srv.Get("Francisco"));
            srv.Put(0, "Ines", "nãotenho");
            //Console.WriteLine(srv.Get("Ines"));
            srv.Put(0, "Afonso", "Teste2");
            //Console.WriteLine(srv.Get("Afonso"));
            srv.Put(0, "Ines", "continuo a nao ter!!!");
            //Console.WriteLine(srv.Get("Ines"));
            srv.Put(0, "Afonso", "Eu tenho um Charizard!!!");
            srv.Put(0, "Hitler", "I'm a PokeMaster!!!");
            //Console.WriteLine(srv.Get("Hitler"));
            srv.Put(0, "Bernardo", "Eu tenho um Charizard!!! Por isso ganho-te!");
            //Console.WriteLine(srv.Get("Afonso",2));
            //Console.WriteLine(srv.Get("Afonso"));
            srv.Put(0, "ola", "Também tenho coisas fixes");
            srv.Put(0, "ole", "Que brilham no escuro e tal");
            srv.Put(0, "oli", "E trolteiam");
            srv.Put(0, "olu", "E catam coisas parvas");
            //srv.CleanTable(UInt32.MaxValue / 2, 0);*/

            while(true){
                Console.ReadLine();
                srv.PrintSemiTables();
            }
        }
    }

    public class Server
    {
        public Node Info;
        public TcpChannel Channel;
        public List<Node> NetworkTopology;
        public Semitable[] Semitables;
        public int K;
        private int tableToInit = 0;

        public Server(Node info, TcpChannel channel, int k)
        {
            Info = info;
            Channel = channel;
            K = k;
            Semitables = new Semitable[2];
        }

        public void InitializeSemitables(uint minST1, uint maxST1,Node replica)
        {
            if (tableToInit < 2)
            {
                Semitables[tableToInit] = new Semitable(minST1, maxST1,replica);
                Console.WriteLine("Initialized SemiTable: " + tableToInit.ToString());
                tableToInit ++;
            }
            else {
                Console.WriteLine("InitializeSemitables() called too many times");
            }
        }

        public void UpdateReplica(Node newNode) {
            foreach (Semitable st in Semitables)
                st.Replica = newNode;
        }

        public void PrintSemiTables() {
            Console.Write("SemiTable 0: ");
            foreach(string key in Semitables[0].Keys){
                Console.Write(key+"; ");
            }
            Console.Write("Min: " + Semitables[0].MinInterval + "; Max: " + Semitables[0].MaxInterval);
            Console.WriteLine("; Replica: "+Semitables[0].Replica);
            Console.Write("SemiTable 1: ");
            foreach (string key in Semitables[1].Keys)
            {
                Console.Write(key + "; ");
            }
            Console.Write("Min: " + Semitables[1].MinInterval + "; Max: " + Semitables[1].MaxInterval);
            Console.WriteLine("; Replica: " + Semitables[1].Replica.ToString());
        }


        public void CopySemitables(Semitable st1, Semitable st2) {  
            Semitables = new Semitable[2];
            Semitables[0] = st1;
            Semitables[1] = st2;
        }

        public void CleanTable(uint div,int side, Node replica) {
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
                    st.Replica = replica;
                }
            }
        }

        public Semitable[] DivideSemiTable(uint div) {
            Semitable[] new_semitables = new Semitable[2];
            foreach (Semitable st in Semitables){
                if (div >= st.MinInterval && div <= st.MaxInterval) {
                    new_semitables[0] = new Semitable(st.MinInterval, div - 1, st.Replica);
                    new_semitables[1] = new Semitable(div, st.MaxInterval,Info); 
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
                    if (hashs.Count == 1) semiCount.Add(hashs[0], hashs.Count);
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

        public void Put(int txid,string key, string value)
        {
            Console.WriteLine("PUT " + key + " " + value);
            foreach (Semitable st in Semitables)
            {
                if (st.ContainsKey(key))
                {

                    if (txid != -1 && !(st.Replica.IP == Info.IP && st.Replica.Port == Info.Port))
                    {
                        ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + st.Replica.IP + ":" + st.Replica.Port.ToString() + "/Server");
                        link.Put(-1, key, value);
                    }
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

                        if (txid != -1 && !(st.Replica.IP == Info.IP && st.Replica.Port == Info.Port))
                        {
                            ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + st.Replica.IP + ":" + st.Replica.Port.ToString() + "/Server");
                            link.Put(-1, key, value);
                        }
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
            /*Console.WriteLine("\nNetwork Topology Update!");
           foreach (Node n in network)
           {
               Console.WriteLine(n);
           }*/
        }


        public void GetInitialIntervals(uint minST1, uint maxST1,Node replica) {
            ctx.InitializeSemitables(minST1, maxST1,replica);
        }

        public Dictionary<uint, int> GetSemiTablesCount()
        {
            return ctx.SemiTablesCount();
        }

        public void CleanSemiTable(uint semiTableToClean, Node node)
        {
            ctx.CleanTable(semiTableToClean,0,node);
        }

        public void CopyAndCleanTable(uint semiTableToClean, Node node)
        {
            Semitable[] tables = ctx.DivideSemiTable(semiTableToClean);
            ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
            Semitable st1 = tables[0];
            Semitable st2 = tables[1];
            link.CopyTable(st1,st2);
            ctx.CleanTable(semiTableToClean, 1,node);
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
            return ctx.Get(key);
        }

        public void Put(int txid, string key, string new_value)
        {
            ctx.Put(txid,key, new_value);

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

        public void CopySemiTables(Node node) {
            Semitable[] tables = ctx.Semitables;
            ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
            Semitable st1 = tables[0];
            Semitable st2 = tables[1];
            link.CopyTable(st1, st2);
            ctx.UpdateReplica(node);
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
            ICentralDirectory ligacao = (ICentralDirectory)Activator.GetObject(
              typeof(ICentralDirectory),
              "tcp://localhost:9090/CentralDirectory");
            ligacao.ServerDown(ctx.Info);
            Console.WriteLine("Server Offline");
        }
    }
}
