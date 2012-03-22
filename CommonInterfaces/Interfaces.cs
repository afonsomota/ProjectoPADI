using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonInterfaces
{
    enum NodeType { Server , Client }

    public class Node{
        public string IP;
        public int Port;
        public NodeType Type;

        public Node(string ip, int port, NodeType clientOrServer){
            IP = ip;
            Port = port;
            Type = clientOrServer;
        }
    }


    public class Operation{
        public string Type;
        public string Key;

        public Operation(string type, string key){
            Type = type;
            Key = key;
        }
    }

    public class TransactionContext {
        public int Txid;
        public Dictionary<int, Operation> Operations;
        public Dictionary<string, List<Node>> NodesLocation;
    }



    public interface ICentralDirectory { 
        //Servidor
        public bool RegisterServer(Node server);

        //Cliente
        public bool RegisterClient(string ip, int port);
        public Dictionary<string, List<Node>> GetServers(List<string> keys);
        public void ServerDown(Node server);
    }


    public interface IServer {
        //Central Directory
        public Dictionary<int, int> GetSemiTablesCount();
        public void CleanSemiTable(int semiTableToClean);
        public void CopyAndCleanTable(int semiTableToClean);
        public void GetNetworkUpdate(List<Node> network);

        //Cliente
        public bool CanLock(int txid, List<string> keys);
        public bool Lock(int txid);
        public string Get(int txid, string key);
        public string Put(int txid, string key, string new_value);
        public bool Abort(int txid);
        public bool CanCommit(int txid);
        public bool Commit(int txid);  
    }

    public interface IServerPuppet {
        public bool KillServer();
        public bool StartServer();
    }


    public interface IClientPuppet
    {
        public bool StartClient();
        public bool KillClient();
    }

    public interface IClient {
        public void GetNetworkUpdate(List<Node> network);
    }
}
