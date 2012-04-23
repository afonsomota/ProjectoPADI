﻿using System;
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

    //Read Lock = Read+Write Lock
    public enum KeyState {FREE= 0 , READ_LOCKING = 1, WRITE_LOCKING= 2 , READ_LOCK = 3  , WRITE_LOCK =4 , NEW =5, TO_REMOVE= 6, COMMITING=7 }

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
            if (state == KeyState.FREE || ((state == KeyState.READ_LOCKING || state == KeyState.WRITE_LOCKING) && txid <= State.Txid))
            {
                State.Txid = txid;
                State.State = KeyState.READ_LOCKING;
                return true;
            }
            else
            {

                Console.WriteLine("Inconsistent State on canLock on Transaction " + txid);
                return false;
            }
        }

        public void lockVariable(int txid) {
            if ((State.State != KeyState.READ_LOCKING && State.State != KeyState.WRITE_LOCKING)||State.Txid!=txid) {
                Console.WriteLine("Inconsistent State on Lock on Transaction " + txid);
            }
            State.State = KeyState.READ_LOCK;
        }

        public void commitingVariable(int txid) {
            if ((State.State != KeyState.READ_LOCK || State.State != KeyState.WRITE_LOCK) || State.Txid != txid)
            {
                Console.WriteLine("Inconsistent State on canCommit on Transaction " + txid);
            }
            State.State = KeyState.COMMITING;
        }

        public void commitVariable(int txid) {
            if (State.State != KeyState.COMMITING || State.Txid != txid)
            {
                Console.WriteLine("Inconsistent State on Commit on Transaction " + txid);
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

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServerPuppet), "ServerPuppet", WellKnownObjectMode.Singleton);
            IPuppetMaster ligacao = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
               "tcp://localhost:8090/PseudoNodeReg");

            Console.WriteLine("Registering Server on Central Directory...");
            Node node = new Node(host, port, NodeType.Server);
            Server srv = new Server(node, channel, 5);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServerRemoting), "Server", WellKnownObjectMode.Singleton);
            ICentralDirectory central = (ICentralDirectory)Activator.GetObject(
               typeof(ICentralDirectory),
               "tcp://localhost:9090/CentralDirectory");
            central.RegisterServer(srv.Info);
            Console.WriteLine("Server Online.");
            ServerPuppet.ctx = srv;
            ServerRemoting.ctx = srv;
            ligacao.RegisterPseudoNode(node);
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
        public Dictionary<int, List<Operation>> OperationList;

        public Server(Node info, TcpChannel channel, int k)
        {
            Info = info;
            Channel = channel;
            K = k;
            Semitables = new Semitable[2];
            TransactionObjects = new Dictionary<int, Dictionary<string,List<TableValue>>>();
            OperationList = new Dictionary<int, List<Operation>>();
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
            bool isTxLocked = true;
            foreach (List<TableValue> list in TransactionObjects[txid].Values)
                foreach (TableValue tv in list)
                {
                    if (tv.isLocked(txid))
                    {
                        isTxLocked = true;
                        break;
                    }
                }
            if (isTxLocked) {
                foreach (string key in TransactionObjects[txid].Keys)
                {
                    foreach (Semitable st in Semitables)
                    {
                        if (st.ContainsKey(key))
                        {
                            List<TableValue> tvToRemove = new List<TableValue>();
                            foreach (TableValue tv in st[key])
                                if (tv.State.State == KeyState.NEW && tv.State.Txid == txid)
                                    tvToRemove.Add(tv);
                            foreach (TableValue tv in tvToRemove)
                            {
                                st[key].Remove(tv);
                            }
                        }
                    }
                }
            }
            foreach (List<TableValue> list in TransactionObjects[txid].Values)
                foreach (TableValue tv in list)
                {
                    tv.State.State = KeyState.FREE;
                    tv.State.Txid = 0;
                }
        }

        public bool Finish(int txid) {
            foreach (string key in TransactionObjects[txid].Keys) {
                foreach (Semitable st in Semitables) {
                    if (st.ContainsKey(key)) { 
                        List<TableValue> tvToRemove = new List<TableValue>();
                        foreach (TableValue tv in st[key])
                            if (tv.State.State == KeyState.TO_REMOVE && tv.State.Txid == txid)
                                tvToRemove.Add(tv);
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
            OperationList.Remove(txid);
            return true;
        }

        public bool IsFinished(int txid) {
            if (OperationList[txid].Count == 0)
            {
                foreach (string key in TransactionObjects[txid].Keys)
                    foreach (TableValue tv in TransactionObjects[txid][key])
                    {
                        tv.commitingVariable(txid);
                    }
                return false;
            }
            else {
                return false; 
            }
        }

        public bool LockTransactionVariables(int txid) {
            foreach (List<TableValue> list in TransactionObjects[txid].Values)
                    foreach(TableValue tv in list)
                        tv.lockVariable(txid);   
            
            return true;
        }
        
        
        public bool AreKeysFree(int txid, List<Operation> ops) {
            Dictionary<string,List< TableValue>> objectsToLock = new Dictionary<string, List<TableValue>>();
            List<Operation> localOperations = new List<Operation>();
            foreach (Operation op in ops)
            {
                string key = op.Key;
                uint hash = MD5Hash(key);
                if(op.Type == OpType.PUT) 
                    foreach (Semitable st in Semitables)
                    {
                        if (hash >= st.MinInterval && hash <= st.MaxInterval) {
                            localOperations.Add(op);
                            if (!st.ContainsKey(key))
                            {
                                List<TableValue> tvList = new List<TableValue>();
                                tvList.Add(new TableValue(null, 0, new TransactionState(txid, KeyState.READ_LOCKING)));
                                st.Add(key, tvList);
                            }
                            int max_timestamp = -1;
                            TableValue valueToAdd = null;
                            foreach (TableValue tv in st[key]) {
                                if (!tv.isFree(txid)) return false;
                                else if (tv.Timestamp >= max_timestamp) valueToAdd = tv;
                            }
                            if (!objectsToLock.ContainsKey(key))
                            {
                                List<TableValue> list = new List<TableValue>();
                                list.Add(valueToAdd);
                                objectsToLock.Add(key, list);
                            }
                            else objectsToLock[key].Add(valueToAdd);
                            
                        }
                    }
                }
            if (objectsToLock.Count > 0)
            {
                foreach(List<TableValue> list in objectsToLock.Values)
                    foreach (TableValue tv in list)
                        tv.lockingVariable(txid);
                TransactionObjects.Add(txid, objectsToLock);
                OperationList.Add(txid, localOperations);
                return true;
            }
            return false;
        }


        public void Put(int txid, string key, string value, bool isReplica)
        {
            Console.WriteLine("PUT " + key + " " + value);
            TransactionState tvState = new TransactionState(txid, KeyState.NEW);
            foreach (Semitable st in Semitables)
            {
                if (st.ContainsKey(key))
                {

                    if (!isReplica && !(st.Replica.IP == Info.IP && st.Replica.Port == Info.Port))
                    {
                        ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + st.Replica.IP + ":" + st.Replica.Port.ToString() + "/Server");
                        link.PutInner(txid, key, value);
                    }
                    int max_timestamp = 0;
                    int min_timestamp = Int32.MaxValue;
                    if (st[key][0].Value == null)
                    {
                        st[key][0].Value = value;
                        st[key][0].State.State = KeyState.NEW;
                        return;
                    }
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
                        min_tv.State.State = KeyState.TO_REMOVE;
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
                        Console.WriteLine("Inserted Key: " + key);
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

        public void PrintSemiTablesValues() {
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
            foreach(string k in TransactionObjects[txid].Keys)
                if (k == key) 
                    foreach (TableValue tv in TransactionObjects[txid][k]) 
                        if (tv.State.State > KeyState.WRITE_LOCKING && tv.State.State != KeyState.COMMITING && tv.State.Txid == txid)
                            return true;
            return false;    
        }

        public bool EligibleForRead(int txid, string key)
        {
            foreach (string k in TransactionObjects[txid].Keys)
                if (k == key)
                    foreach (TableValue tv in TransactionObjects[txid][k])
                        if ((tv.State.State > KeyState.WRITE_LOCKING && tv.State.State != KeyState.COMMITING && tv.State.Txid == txid) ||
                                tv.State.State==KeyState.WRITE_LOCK)
                            return true;
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
                if (hashs.Count != 0)
                {
                    if (hashs.Count == 1) semiCount.Add(hashs[0], hashs.Count);
                    else semiCount.Add(hashs[hashs.Count / 2 + hashs.Count % 2], hashs.Count);
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

        public void RemoveOperation(int txid,OpType type, string key) {
            List<Operation> ops = OperationList[txid];
            Operation opToRemove = null;
            foreach (Operation op in ops) {
                if (op.Key == key && op.Type == type){
                    opToRemove = op;
                    break;
                }
            }
            if (opToRemove != null) ops.Remove(opToRemove);
            else Console.WriteLine("Inconsistent state for Remove Operation");
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

        public bool CanLock(int txid, List<Operation> keys) {
            lock (locker) {
                return ctx.AreKeysFree(txid,keys);
            }
        }

        public bool Lock(int txid)
        {
            lock (locker) {
                return ctx.LockTransactionVariables(txid);
            }
        }

        public string Get(int txid, string key)
        {
            string ret = null;
            if (ctx.EligibleForRead(txid, key) && ctx.ContainsKey(key))
            {
                ret =  ctx.Get(key);
                ctx.RemoveOperation(txid, OpType.GET, key);
            }

            return ret;
        }

        public void Put(int txid, string key, string new_value)
        {
            if (ctx.EligibleForWrite(txid, key))
            {
                ctx.Put(txid, key, new_value, false);
                ctx.RemoveOperation(txid, OpType.PUT, key);
            }
        }

        public void PutInner(int txid, string key, string new_value) {
            if (ctx.EligibleForWrite(txid, key))
            {
                ctx.Put(txid, key, new_value, true);
                ctx.RemoveOperation(txid, OpType.PUT, key);
            }
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
            Semitable[] tables = ctx.Semitables;
            ServerRemoting link = (ServerRemoting)Activator.GetObject(typeof(ServerRemoting), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
            Semitable st1 = tables[0];
            Semitable st2 = tables[1];
            link.CopyTable(st1, st2);
            ctx.UpdateReplica(node);
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
            ICentralDirectory ligacao = (ICentralDirectory)Activator.GetObject(
              typeof(ICentralDirectory),
              "tcp://localhost:9090/CentralDirectory");
            ligacao.ServerDown(ctx.Info);
            Environment.Exit(0);
        }
    }
}
