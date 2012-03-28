using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            return string.Format("{1}@{2}:{3}", (Type==NodeType.Client?"client":"server"),IP,Port.ToString());
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
            return string.Format("{1}({2})",Type, Key);
        }
    }

    public class TransactionContext
    {
        int Txid;
        enum states { initiated, tentatively, commited, aborted };
        Dictionary<int, Operation> Operations;
        Dictionary<string, List<Node>> NodesLocation;
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
        Dictionary<int, int> GetSemiTablesCount();
        void CleanSemiTable(int semiTableToClean);
        void CopyAndCleanTable(int semiTableToClean);
        void GetNetworkUpdate(List<Node> network);

        //Cliente
        bool CanLock(int txid, List<string> keys);
        bool Lock(int txid);
        string Get(int txid, string key);
        string Put(int txid, string key, string new_value);
        bool Abort(int txid);
        bool CanCommit(int txid);
        bool Commit(int txid);  
    }

    public interface IServerPuppet {
        bool KillServer();
        bool StartServer();
    }


    public interface IClientPuppet
    {
        void StartClient();
        void KillClient();
    }

    public interface IClient {
        void GetNetworkUpdate(List<Node> network);
    }

    public interface IPuppetMaster {
        void RegisterPseudoNode(Node node);
    }
}
