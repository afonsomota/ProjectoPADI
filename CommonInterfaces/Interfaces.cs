using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace CommonInterfaces
{
    
    public enum NodeType { Server, Client }

    [Serializable]
    public class Node
    {
        public string IP;
        public string Name;
        public int Port;
        public NodeType Type;

        public Node(string ip, int port,string name, NodeType clientOrServer){
            IP = ip;
            Name = name;
            Port = port;
            Type = clientOrServer;
        }
        public Node(string ip, int port, NodeType clientOrServer)
        {
            IP = ip;
            Name = null;
            Port = port;
            Type = clientOrServer;
        }

        public Node(SerializationInfo info, StreamingContext context) {
            IP = info.GetString("i");
            Port = info.GetInt32("p");
            char c = info.GetChar("c");
            if (c == 'c')
            {
                Type = NodeType.Client;
            }
            else {
                Type = NodeType.Server;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}@{1}:{2}", (Type==NodeType.Client?"client":"server"),IP,Port.ToString());
        }

        public override bool Equals(System.Object obj)
        {
            Node n = obj as Node;
            if ((object)n == null) return false;
            return Equals(n);
        }

        public bool Equals(Node node)
        {
            if (IP == node.IP && Port == node.Port) return true;
            else return false;
        }

        public static bool operator ==(Node a, Node b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.IP == b.IP && a.Port == b.Port;
        }

        public static bool operator !=(Node a, Node b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^  Port.GetHashCode() ^ IP.GetHashCode() ^ Type.GetHashCode();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("i", IP);
            info.AddValue("p", Port);
            char c;
            if (Type == NodeType.Client)
            {
                c = 'c';
                info.AddValue("t", c);
            }
            else {
                c = 's';
                info.AddValue("t", c);
            }
        }
    }

    public enum OpType { PUT, GET }

    [Serializable]
    public class Operation 
    {
        public OpType Type;
        public string Key;
        public string Value;
        public int Register;

        public Operation(string key){
            Type = OpType.GET;
            Key = key;
            Value = null;
        }

        public Operation(string key, string value) {
            Type = OpType.PUT;
            Key = key;
            Value = value;
        }


        public override string ToString()
        {
            if (Type == OpType.GET)
            {
                return string.Format("GET({0})", Key);
            }
            else {
                return string.Format("PUT({0},{1})", Key,Value);
            }
        }
    }

    [Serializable]
    public class TransactionContext
    {
        public enum states { initiated, tentatively, commited, aborted };
        public int Txid;
        public states State;
        //public Dictionary<int, Operation> Operations;
        //public Dictionary<string, List<Node>> NodesLocation;


        public override string ToString()
        {
            string ret = "";
            ret += "ID: "+Txid.ToString() + "\n";
            switch (State) { 
                case states.initiated:
                    ret += "State: Initiated\n";
                    break;
                case states.tentatively:
                    ret += "State: Tentatively\n";
                    break;
                case states.commited:
                    ret += "State: Commited\n";
                    break;
                case states.aborted:
                    ret += "State: Aborted\n"; 
                    break;
                default:
                    ret += "No State";
                    break;
            }/*
            ret += "Operations:\n";
            foreach (KeyValuePair<int,Operation> op in Operations)
            {
                ret += op.Key.ToString() +  ". " + op.Value.ToString() + "\n";
            }
            ret += "Nodes:\n";
            foreach (KeyValuePair<string,List<Node>> n in NodesLocation)
            {
                ret += n.Key + " - " + n.Value[0].ToString() + " and " + n.Value[1].ToString() + "\n";
            }*/
            return ret;
        }

    }

    public interface ICentralDirectory {

        //Servidor
        void RegisterServer(Node server);

        //Cliente
        bool RegisterClient(Node client);
        //public Dictionary<string, List<Node>> GetServers(List<string> keys);
        void UpdateTransactionState(TransactionContext tctx);
        TransactionContext BeginTx();
        List<Node> GetServers(string key);
        void ServerDown(Node server);
    }


    public interface IServer {
        //Central Directory
        Dictionary<uint, int> GetSemiTablesCount();
        void CleanSemiTable(uint semiTableToClean, Node newNode);
        void CopyAndCleanTable(uint semiTableToClean,Node nodeToCopy);
        void GetInitialIntervals(uint minST1, uint maxST1, Node replica);
        void GetNetworkUpdate(List<Node> network);
        void CopySemiTables(Node newNode);//Caso especial em que se comeca com um servidor e adiciona-se outro
        void CopySemiTable(uint semiTableToCopy, Node nodeToCopy);// Caso em que um servidor vai abaixo e outro vem acima

        //Cliente
        bool CanLock(int txid, string key);
        bool Lock(int txid, string key);
        string Get(int txid, string key);
        bool Put(int txid, string key, string new_value);
        bool Abort(int txid);
        bool CanCommit(int txid);
        bool Commit(int txid); 
    }

    public interface IServerPuppet {
        void KillServer();
        void StartServer();
        string GetAll(string key);
    }


    public interface IClientPuppet
    {
        void StartClient();
        void KillClient();
        void Store(int register, string value);
        void Put(int register, string key);
        void Get(int register, string key);
        void PutVAl(string key, string value);
        void ToLower(int register);
        void ToUpper(int register);
        void Concat(int register1, int register2);
        void CommitTx();
        void Sleep(int ms);
        string[] Dump();
        void ExeScript(List<string> operations);
    }

    public interface IClient {
        void GetNetworkUpdate(List<Node> network);
    }

    public interface IPuppetMaster {
        void RegisterPseudoNode(Node node);
    }
}
