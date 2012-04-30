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
using System.Net.Sockets;

namespace Server
{

    //Read Lock = Read+Write Lock
    public enum KeyState {FREE= 0 , READ_LOCKING = 1, WRITE_LOCKING= 2 , READ_LOCK = 3  , WRITE_LOCK =4 , NEW =5, TO_REMOVE= 6, COMMITING=7, NEW_COMMITING = 8, TO_REMOVE_COMMITING = 9 }

    [Serializable]
    public class TransactionState {
        public int Txid;
        public KeyState State;

        public TransactionState() {
            Txid = 0;
            State = KeyState.FREE; 
        }
        public TransactionState(int txid,KeyState state)
        {
            Txid = txid;
            State = state;
        }

        public override string ToString() {
            string ret;

            switch (State) { 
                case KeyState.FREE:
                    ret  = "FREE";
                    break;
                case KeyState.READ_LOCKING:
                    ret = "READ_LOCKING";
                    break;
                case KeyState.WRITE_LOCKING:
                    ret = "WRITE_LOCKING";
                    break;
                case KeyState.READ_LOCK:
                    ret = "READ_LOCK";
                    break;
                case KeyState.WRITE_LOCK:
                    ret = "WRITE_LOCK";
                    break;
                case KeyState.NEW:
                    ret = "NEW";
                    break;
                case KeyState.TO_REMOVE:
                    ret = "TO_REMOVE";
                    break;
                case KeyState.COMMITING:
                    ret = "COMMITING";
                    break;
                case KeyState.NEW_COMMITING:
                    ret = "NEW_COMMITING";
                    break;
                case KeyState.TO_REMOVE_COMMITING:
                    ret = "TO_REMOVE_COMMITING";
                    break;
                default:
                    ret = "";
                    break;
            }
            if (ret != "FREE") {
                ret += " to txid " + Txid;
            }
            return ret;
        }
    }

    [Serializable]
    public class TableValue
    {
        public string Value;
        public int Timestamp;
        public TransactionState State;

        public TableValue(string val, int timestamp) {
            Value = val;
            Timestamp = timestamp;
            State = new TransactionState();
        }
        public TableValue(string val, int timestamp, TransactionState state)
        {
            Value = val;
            Timestamp = timestamp;
            State = state;
        }

        public bool isLocked(int txid) {
            if (State.State > KeyState.WRITE_LOCKING) return true;
            else return false;
        }

        public bool isLockable(int txid) { 
            KeyState state = State.State;
            if ((state < KeyState.COMMITING && txid < State.Txid && state != KeyState.FREE) || state == KeyState.FREE)
            {
                return true;
            }
            else return false;
        }

        public bool isFree(int txid){
            KeyState state = State.State;
            if (state == KeyState.FREE || ((state == KeyState.READ_LOCKING || state == KeyState.WRITE_LOCKING) && txid <= State.Txid))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool lockingVariable(int txid) {
            KeyState state = State.State;
            if ((state < KeyState.COMMITING && txid < State.Txid && state != KeyState.FREE) || state == KeyState.FREE)
            {
                State.Txid = txid;
                State.State = KeyState.READ_LOCKING;
                Value = null;
                return true;
            }
            else
            {
                Console.WriteLine("Inconsistent State on canLock on Transaction " + txid + ": " + State);
                return false;
            }
        }

        public bool lockVariable(int txid) {
            if (State.Txid != txid)
            {
                return false;
            }
            else
            {
                if (State.State != KeyState.READ_LOCKING && State.State != KeyState.WRITE_LOCKING) 
                    Console.WriteLine("Inconsistent State on Lock on Transaction " + txid + ": " + State + " v:" + Value + " t:" + Timestamp);
                State.State = KeyState.READ_LOCK;
                return true;
            }
        }

        public bool commitingVariable(int txid) {
            if (State.Txid != txid)
            {
                return false;
            }
            else if(State.State == KeyState.READ_LOCK || State.State == KeyState.WRITE_LOCK
                || State.State == KeyState.NEW || State.State == KeyState.TO_REMOVE){
                switch (State.State){
                    case KeyState.NEW:
                        State.State = KeyState.NEW_COMMITING;
                        break;
                    case KeyState.TO_REMOVE:
                        State.State = KeyState.TO_REMOVE_COMMITING;
                        break;
                    default:
                        State.State = KeyState.COMMITING;
                        break;
                }
                return true;
            }
            Console.WriteLine("Inconsistent State on canCommit on Transaction " + txid + ": " + State + " v:" + Value + " t:" + Timestamp);
            return false;
        }

        public void commitVariable(int txid) {
            if (State.State < KeyState.COMMITING || State.Txid != txid)
            {
                Console.WriteLine("Inconsistent State on Commit on Transaction " + txid + ": " + State + " v:" + Value + " t:" + Timestamp);
            }
            State.State = KeyState.FREE;
            State.Txid = 0;
        }
    } 


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

            
            Console.WriteLine("Registering Server on Central Directory...");
            Node node = new Node(host, port, NodeType.Server);
            Server srv = new Server(node, channel, 5);
            ServerPuppet.ctx = srv;
            ServerRemoting.ctx = srv;
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServerPuppet), "ServerPuppet", WellKnownObjectMode.Singleton);
            IPuppetMaster ligacao = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
               "tcp://localhost:8090/PseudoNodeReg");
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServerRemoting), "Server", WellKnownObjectMode.Singleton);
            ICentralDirectory central = (ICentralDirectory)Activator.GetObject(
               typeof(ICentralDirectory),
               "tcp://localhost:9090/CentralDirectory");
            central.RegisterServer(srv.Info);
            Console.WriteLine("Server Online.");
            ServerPuppet.ctx = srv;
            ServerRemoting.ctx = srv;
            try
            {
                ligacao.RegisterPseudoNode(node);
            }
            catch
            {
                Console.WriteLine("We Don't Need No PuppetMaster");
            }
            System.Console.WriteLine(host + ":" + port.ToString());
            while(true){
                Console.ReadLine();
                srv.PrintSemiTablesValues();
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
        public Dictionary<int, Dictionary<string,List<TableValue>>> TransactionObjects;
        public Dictionary<int, bool> ReadOnlyTransation;

        public Server(Node info, TcpChannel channel, int k)
        {
            Info = info;
            Channel = channel;
            K = k;
            Semitables = new Semitable[2];
            TransactionObjects = new Dictionary<int, Dictionary<string,List<TableValue>>>();
            ReadOnlyTransation = new Dictionary<int, bool>();
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

        public void UpdateReplica(uint hash,Node newNode)
        {
            foreach (Semitable st in Semitables)
                if (hash >= st.MinInterval && hash <= st.MaxInterval)
                {
                    Console.WriteLine("Replica from semiTable containing " + hash + " updated to " + newNode);
                    st.Replica = newNode;
                }
        }

        public void Abort(int txid) {
            if (!TransactionObjects.ContainsKey(txid)) return;
                foreach (string key in TransactionObjects[txid].Keys)
                {
                    foreach (Semitable st in Semitables)
                    {
                        if (st.ContainsKey(key))
                        {
                            List<TableValue> tvToRemove = new List<TableValue>();
                            foreach (TableValue tv in st[key])
                                if (tv.State.State == KeyState.NEW && tv.State.Txid == txid || tv.Value == null)
                                    tvToRemove.Add(tv);
                            foreach (TableValue tv in tvToRemove)
                            {
                                st[key].Remove(tv);
                            }
                            if (st[key].Count == 0) {
                                st.Remove(key);
                            }
                        }
                    }
                }
            
            foreach (List<TableValue> list in TransactionObjects[txid].Values)
                foreach (TableValue tv in list)
                {
                    if (tv.State.Txid == txid)
                    {
                        tv.State.State = KeyState.FREE;
                        tv.State.Txid = 0;
                    }
                }
        }

        public bool Finish(int txid) {
            if (ReadOnlyTransation.ContainsKey(txid) && ReadOnlyTransation[txid])
                return true;
            foreach (string key in TransactionObjects[txid].Keys) {
                foreach (Semitable st in Semitables) {
                    if (st.ContainsKey(key)) { 
                        List<TableValue> tvToRemove = new List<TableValue>();
                        foreach (TableValue tv in st[key])
                        {
                            if (tv.State.State == KeyState.TO_REMOVE_COMMITING && tv.State.Txid == txid || tv.Value == null)
                                tvToRemove.Add(tv);
                        }
                        foreach (TableValue tv in tvToRemove) {
                            st[key].Remove(tv);
                        }
                    }
                }
            }
            foreach (List<TableValue> list in TransactionObjects[txid].Values)
                foreach (TableValue tv in list)
                    tv.commitVariable(txid);
            TransactionObjects.Remove(txid);
            return true;
        }

        public bool IsFinished(int txid) {
            bool allAbleToCommit = true;
            List<TableValue> valuesToRemove = new List<TableValue>();
            if (ReadOnlyTransation.ContainsKey(txid) && ReadOnlyTransation[txid])
                return true;
            foreach (string key in TransactionObjects[txid].Keys){
                foreach (TableValue tv in TransactionObjects[txid][key]){
                    if (!tv.commitingVariable(txid))
                    {
                        allAbleToCommit = false;
                        valuesToRemove.Add(tv);
                    }
                }
                foreach (TableValue tv in valuesToRemove)
                    TransactionObjects[txid][key].Remove(tv);
            }
            return allAbleToCommit;
        }

        public bool LockTransactionVariables(int txid,string key) {
            bool allLocked = true;
            List<TableValue> valuesToRemove = new List<TableValue>();
            if (ReadOnlyTransation.ContainsKey(txid))
            {
                if (ReadOnlyTransation[txid]) return true;
            }
            
            foreach (TableValue tv in TransactionObjects[txid][key])
                if (!tv.lockVariable(txid)){
                    allLocked = false;
                    valuesToRemove.Add(tv);   
                }
            PrintSemiTablesValues();
            foreach (TableValue tv in valuesToRemove)
                TransactionObjects[txid][key].Remove(tv);
            return allLocked;
        }
        
        
        public bool AreKeysFree(int txid, string key) {
            Dictionary<string,List< TableValue>> objectsToLock = new Dictionary<string, List<TableValue>>();
            uint hash = MD5Hash(key);
            foreach (Semitable st in Semitables)
            {
                if (hash >= st.MinInterval && hash <= st.MaxInterval) {
                    TableValue valueToAdd = null;
                    if (!st.ContainsKey(key))
                    {
                        List<TableValue> tvList = new List<TableValue>();
                        st.Add(key, tvList);
                        valueToAdd = new TableValue(null, 0, new TransactionState(0, KeyState.FREE));
                    }
                    int max_timestamp = -1;
                    TableValue max_tv = null;
                    foreach (TableValue tv in st[key]) {
                        if (!tv.isLockable(txid))
                        {
                            Console.WriteLine("ReadOnly so far...");
                            if(ReadOnlyTransation.ContainsKey(txid)){
                                if (ReadOnlyTransation[txid]) return true;
                                else return false;
                            }else{
                                ReadOnlyTransation.Add(txid,true);
                                return true;
                            }
                        }
                        else if (tv.Timestamp >= max_timestamp)
                        {
                            max_timestamp = tv.Timestamp;
                            max_tv = tv;
                        }
                        if (tv.Value == null)
                            valueToAdd = tv;
                    }
                    if (valueToAdd == null) {
                        if (max_tv != null && max_tv.State.State!=KeyState.FREE)
                        {
                            max_tv.State.State = KeyState.FREE;
                            max_tv.State.Txid = txid;
                            max_tv.Value = null;
                            valueToAdd = max_tv;
                        }
                        else {
                            valueToAdd = new TableValue(null, max_timestamp + 1, new TransactionState(0, KeyState.FREE));
                        }
                    }
                    st[key].Add(valueToAdd);
                    bool objectsContainsKeys = false;
                    foreach (string dicKey in objectsToLock.Keys)
                        if (key == dicKey) objectsContainsKeys = true;
                    if (!objectsContainsKeys)
                    {
                        List<TableValue> list = new List<TableValue>();
                        list.Add(valueToAdd);
                        objectsToLock.Add(key, list);
                    }
                }
            }
            if (objectsToLock.Count > 0)
            {
                foreach(List<TableValue> list in objectsToLock.Values)
                    foreach (TableValue tv in list)
                    {
                        tv.lockingVariable(txid);
                        PrintSemiTablesValues();
                    }
                if (!TransactionObjects.ContainsKey(txid))
                    TransactionObjects.Add(txid, objectsToLock);
                else foreach (string dicKey in objectsToLock.Keys) {
                    TransactionObjects[txid].Add(dicKey, objectsToLock[dicKey]);
                }
                return true;
            }
            if (ReadOnlyTransation.ContainsKey(txid))
            {
                if (ReadOnlyTransation[txid]) return true;
                else return false;
            }
            else
            {
                ReadOnlyTransation.Add(txid, true);
                return true;
            }
        }


        public void Put(int txid, string key, string value, bool isReplica)
        {
            TransactionState tvState = new TransactionState(txid, KeyState.NEW);
            foreach (Semitable st in Semitables)
            {
                if (st.ContainsKey(key))
                {

                    if (!isReplica && !(st.Replica.IP == Info.IP && st.Replica.Port == Info.Port))
                    {
                        ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + st.Replica.IP + ":" + st.Replica.Port.ToString() + "/Server");
                        try
                        {
                            link.PutInner(txid, key, value);
                        }
                        catch(SocketException ex)
                        {
                            ICentralDirectory central = (ICentralDirectory)Activator.GetObject(
                                  typeof(ICentralDirectory),
                                  "tcp://localhost:9090/CentralDirectory");
                            central.ServerDown(st.Replica);
                        }
                    }
                    
                    TableValue min_tv = st[key][0];
                    int max_timestamp = 0;
                    int min_timestamp = Int32.MaxValue;
                    TableValue max_tv = st[key][0];
                    foreach (TableValue tv in st[key])
                    {
                        if (tv.Timestamp > max_timestamp)
                        {
                            max_timestamp = tv.Timestamp;
                            max_tv = tv;
                        }
                        if (tv.State.State != KeyState.TO_REMOVE && tv.Timestamp < min_timestamp)
                        {
                            min_timestamp = tv.Timestamp;
                            min_tv = tv;
                        }
                    }
                    if (max_tv.Value == null)
                    {
                        max_tv.Value = value;
                        max_tv.State.State = KeyState.NEW;
                        return;
                    }
                    if (st[key].Count >= K)
                    {
                        min_tv.State.State = KeyState.TO_REMOVE;
                    }
                    TableValue newtv = new TableValue(value, max_timestamp + 1, tvState);
                    TransactionObjects[txid][key].Add(newtv);
                    st[key].Add(newtv);
                }
                else
                {
                    uint hash = MD5Hash(key);
                    if (hash >= st.MinInterval && hash <= st.MaxInterval)
                    {

                        if (!isReplica && !(st.Replica.IP == Info.IP && st.Replica.Port == Info.Port))
                        {
                            ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + st.Replica.IP + ":" + st.Replica.Port.ToString() + "/Server");
                            link.PutInner(txid, key, value);
                        }
                        TableValue tv = new TableValue(value, 0, tvState);
                        List<TableValue> values = new List<TableValue>();
                        values.Add(tv);
                        st.Add(key, values);
                    }
                }
            }
        }


        public bool ContainsKey(string key){
            foreach (Semitable st in Semitables) 
                if (st.ContainsKey(key)) 
                    return true;
            return false;
        }

        public string GetAll(string key) {
            string ret = "";
            foreach (Semitable st in Semitables) { 
                if(st.ContainsKey(key)){
                    foreach (TableValue tv in Semitables[0][key])
                    {
                        ret += (tv.Value + ";" + tv.Timestamp + ";" + tv.State + "\n");
                    }
                }
            }
            return ret;
        }

        public void PrintSemiTablesValues() {
            if (Semitables[0] == null)
            {
                Console.WriteLine("Server Not Initialized.");
                return;
            }
            Console.Write("SemiTable 0-> ");
            Console.Write("[" + Semitables[0].MinInterval + "," + Semitables[0].MaxInterval+"]");
            Console.WriteLine("; Replica: " + Semitables[0].Replica);
            foreach (string key in Semitables[0].Keys)
            {
                Console.WriteLine(key + ":");
                foreach (TableValue tv in Semitables[0][key]) {
                    Console.WriteLine("\t\tValue: " + tv.Value + "; Timestamp: " + tv.Timestamp + "; State: " + tv.State);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("SemiTable 1-> ");
            Console.Write("[" + Semitables[1].MinInterval + "," + Semitables[1].MaxInterval + "]");
            Console.WriteLine("; Replica: " + Semitables[1].Replica);
            foreach (string key in Semitables[1].Keys)
            {
                Console.WriteLine(key + ":");
                foreach (TableValue tv in Semitables[1][key])
                {
                    Console.WriteLine("\t\tValue: " + tv.Value + "; Timestamp: " + tv.Timestamp + "; State: " + tv.State);
                }
                Console.WriteLine();
            }
            
        
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

        public bool EligibleForWrite(int txid,string key) {
            foreach (Semitable st in Semitables)
            {
                uint hash = MD5Hash(key);
                if (hash >= st.MinInterval && hash <= st.MaxInterval)
                {
                    bool hasKey = false;
                    foreach (string dicKey in st.Keys)
                    {
                        if (dicKey == key)
                        {
                            hasKey = true;
                            break;
                        }
                    }
                    if (hasKey)
                    {
                        foreach (TableValue tv in st[key])
                        {
                            if (tv.State.State > KeyState.WRITE_LOCKING && tv.State.State < KeyState.COMMITING && tv.State.Txid == txid)
                                return true;
                        }
                    }
                    else return false;
                }
            }
                /*
            foreach(string k in TransactionObjects[txid].Keys)
                if (k == key) 
                    foreach (TableValue tv in TransactionObjects[txid][k]) 
                        if (tv.State.State > KeyState.WRITE_LOCKING && tv.State.State < KeyState.COMMITING && tv.State.Txid == txid)
                            return true;*/
            return false;    
        }

        public bool EligibleForRead(int txid, string key)
        {
            foreach (Semitable st in Semitables)
            {
                uint hash = MD5Hash(key);
                if (hash >= st.MinInterval && hash <= st.MaxInterval)
                {
                    bool hasKey = false;
                    foreach (string dicKey in st.Keys)
                    {
                        if (dicKey == key)
                        {
                            hasKey = true;
                            break;
                        }
                    }
                    if (hasKey)
                    {
                        foreach (TableValue tv in st[key])
                        {
                            if ((tv.State.State > KeyState.WRITE_LOCKING && tv.State.State < KeyState.COMMITING && tv.State.Txid == txid) ||
                                tv.State.State == KeyState.WRITE_LOCK) 
                                return true;
                        }
                    }
                    else return false;
                }
            }
     /*
            foreach (string k in TransactionObjects[txid].Keys)
                if (k == key)
                    foreach (TableValue tv in TransactionObjects[txid][k])
                        if ((tv.State.State > KeyState.WRITE_LOCKING && tv.State.State < KeyState.COMMITING && tv.State.Txid == txid) ||
                                tv.State.State==KeyState.WRITE_LOCK)
                            return true;*/
            return false;
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
                if (hashs.Count > 1)
                {
                    if (hashs.Count == 1) semiCount.Add(hashs[0], hashs.Count);
                    else semiCount.Add(hashs[hashs.Count / 2 + hashs.Count % 2], hashs.Count);
                }
                else {
                    semiCount.Add(st.MinInterval + (st.MaxInterval - st.MinInterval) / 2, hashs.Count);
                }
            }
            return semiCount;
        }

        public Semitable GetSemiTable(uint hash) {
            foreach (Semitable st in Semitables)
                if (hash >= st.MinInterval && hash <= st.MaxInterval)
                    return st;
            return null;
        }

        public string Get(string key, int timestamp) {
            foreach (Semitable st in Semitables)
                if(st.ContainsKey(key))
                    foreach (TableValue tv in st[key]) 
                        if (tv.Timestamp == timestamp)
                            return tv.Value;
            return null;
        }

        public string Get(int txid, string key)
        {
            foreach (Semitable st in Semitables)
                if (st.ContainsKey(key)){
                    int max_timestamp = 0;
                    TableValue max_tv = st[key][0];
                    foreach (TableValue tv in st[key]){
                        if (tv.Timestamp > max_timestamp){
                            if (ReadOnlyTransation.ContainsKey(txid))
                            {
                                if (ReadOnlyTransation[txid] && tv.State.State == KeyState.FREE) {
                                    max_timestamp = tv.Timestamp;
                                    max_tv = tv;
                                }
                            }
                            else
                            {
                                if (tv.Value != null)
                                {
                                    max_timestamp = tv.Timestamp;
                                    max_tv = tv;
                                }
                            }
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

       

       
    }

    public class ServerRemoting: MarshalByRefObject, IServer{
        public static Server ctx;
        private System.Object locker = new System.Object();
        private System.Object lockerInner = new System.Object();

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
            lock (locker)
            {
                ctx.CleanTable(semiTableToClean, 0, node);
            }
        }

        public void CopyAndCleanTable(uint semiTableToClean, Node node)
        {
            lock (locker)
            {
                Semitable[] tables = ctx.DivideSemiTable(semiTableToClean);
                ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
                Semitable st1 = tables[0];
                Semitable st2 = tables[1];
                link.CopyTable(st1, st2);
                ctx.CleanTable(semiTableToClean, 1, node);
            }
        }

        public void CopyTable(Semitable st1, Semitable st2)
        {
            lock (locker)
            {
                ctx.CopySemitables(st1, st2);
            }
        }

        public bool CanLock(int txid, string key) {
            lock (locker) {
                return ctx.AreKeysFree(txid,key);
            }
        }

        public bool Lock(int txid, string key)
        {
            lock (locker) {
                return ctx.LockTransactionVariables(txid,key);
            }
        }

        public string Get(int txid, string key)
        {
            string ret = null;
            if ((ctx.EligibleForRead(txid, key) || (ctx.ReadOnlyTransation.ContainsKey(txid) && ctx.ReadOnlyTransation[txid])) && ctx.ContainsKey(key))
            {
                ret =  ctx.Get(txid, key);
            }

            return ret;
        }

        public bool Put(int txid, string key, string new_value)
        {
            if (ctx.EligibleForWrite(txid, key))
            {
                if (ctx.ReadOnlyTransation.ContainsKey(txid))
                {
                    if (ctx.ReadOnlyTransation[txid])
                    {
                        ctx.ReadOnlyTransation[txid] = false;
                        return false;
                    }
                }
                else
                {
                    ctx.ReadOnlyTransation.Add(txid, false);
                }
                ctx.Put(txid, key, new_value, false);
                return true;
            }
            else return false;
        }

        public bool PutInner(int txid, string key, string new_value) {
            if (ctx.EligibleForWrite(txid, key))
            {

                if (ctx.ReadOnlyTransation.ContainsKey(txid))
                {
                    if (ctx.ReadOnlyTransation[txid])
                    {
                        ctx.ReadOnlyTransation[txid] = false;
                        return false;
                    }
                }
                else
                {
                    ctx.ReadOnlyTransation.Add(txid, false);
                }
                
                ctx.Put(txid, key, new_value, true);
                return true;
            }
            else return false;
        }

        public bool Abort(int txid)
        {
            lock (locker) {
                ctx.Abort(txid);
                return true;
            }
        }

        public bool CanCommit(int txid)
        {
            ctx.PrintSemiTablesValues();
            lock (locker) {
                return ctx.IsFinished(txid);
            }
        }

        public bool Commit(int txid)
        {
            lock(locker){
                return ctx.Finish(txid);
            }
        }

        public void CopySemiTables(Node node) {
            lock (locker)
            {
                Semitable[] tables = ctx.Semitables;
                ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
                Semitable st1 = tables[0];
                Semitable st2 = tables[1];
                link.CopyTable(st1, st2);
                ctx.UpdateReplica(node);
            }
        }

        public void SendSemiTable(Semitable st1)
        {
            if (ctx.Semitables[0]==null) {
                ctx.Semitables[0] = st1;
            }
            else if (ctx.Semitables[1] == null) {
                ctx.Semitables[1] = st1;
            }
        }

        public void CopySemiTable(uint semiTableToCopy, Node node) {
            Semitable st1 = ctx.GetSemiTable(semiTableToCopy);
            st1.Replica = ctx.Info;
            if (st1 != null) {
                ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
                if (link != null) link.SendSemiTable(st1);
            }
            ctx.UpdateReplica(semiTableToCopy, node);
        }

        public string GetAll(string key)
        {
            return ctx.GetAll(key);
        }

    }

    class ServerPuppet : MarshalByRefObject, IServerPuppet
    {
        public static Server ctx;

        public string GetAll(string key) {
            return ctx.GetAll(key);
        }

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
            ICentralDirectory ligacao = (ICentralDirectory)Activator.GetObject(
              typeof(ICentralDirectory),
              "tcp://localhost:9090/CentralDirectory");
            //ligacao.ServerDown(ctx.Info);
            Environment.Exit(0);
        }
    }
}
