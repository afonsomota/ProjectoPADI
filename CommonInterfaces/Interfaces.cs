using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CommonInterfaces
{

    public enum NodeType { Server, Client }

    [Serializable]
    public class Node
    {
        public string IP;
        public int Port;
        public NodeType Type;

        public Node(string ip, int port, NodeType clientOrServer){
            IP = ip;
            Port = port;
            Type = clientOrServer;
        }

        public override string ToString()
        {
            return string.Format("{0}@{1}:{2}", (Type==NodeType.Client?"client":"server"),IP,Port.ToString());
        }
    }

    [Serializable]
    public class Operation 
    {
        public string Type;
        public string Key;

        public Operation(string type, string key){
            Type = type;
            Key = key;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})",Type, Key);
        }
    }

    [Serializable]
    public class TransactionContext
    {
        public int Txid;
        public enum states { initiated, tentatively, commited, aborted };
        public Dictionary<int, Operation> Operations;
        public Dictionary<string, List<Node>> NodesLocation;
    }



    public interface ICentralDirectory {

        //Servidor
        bool RegisterServer(Node server);

        //Cliente
        bool RegisterClient(Node client);
        //public Dictionary<string, List<Node>> GetServers(List<string> keys);
        TransactionContext GetServers(List<string> keys);
        void ServerDown(Node server);
    }


    public interface IServer {
        //Central Directory
        Dictionary<uint, int> GetSemiTablesCount();
        void CleanSemiTable(uint semiTableToClean);
        void CopyAndCleanTable(uint semiTableToClean,Node nodeToCopy);
        void GetInitialIntervals(uint minST1, uint maxST1, Node replica);
        void GetNetworkUpdate(List<Node> network);

        //Cliente
        bool CanLock(int txid, List<string> keys);
        bool Lock(int txid);
        string Get(int txid, string key);
        void Put(int txid, string key, string new_value);
        bool Abort(int txid);
        bool CanCommit(int txid);
        bool Commit(int txid);  
    }

    public interface IServerPuppet {
        void KillServer();
        void StartServer();
    }


    public interface IClientPuppet
    {
        void StartClient();
        void KillClient();
        void BeginTx();
        void Store(int register, string value);
        void Put(int register, string key);
        void Get(int register, string key);
        void PutVAl(string key, int value);
        void ToLower(int register);
        void ToUpper(int register);
        void Concat(int register1, int register2);
        void CommitTx();
        string[] Dump();
        void ExeScript(string[] operations);
    }

    public interface IClient {
        void GetNetworkUpdate(List<Node> network);
    }

    public interface IPuppetMaster {
        void RegisterPseudoNode(Node node);
    }
}
