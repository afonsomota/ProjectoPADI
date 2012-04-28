using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonInterfaces;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Collections;
using System.Threading;
using System.IO;


namespace Client
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
            string name = args[0];

            Console.WriteLine("Ciente: " + name);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientPuppet), "ClientPuppet", WellKnownObjectMode.Singleton);
           
            IPuppetMaster puppet = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
               "tcp://localhost:8090/PseudoNodeReg");

            ICentralDirectory cd = (ICentralDirectory)Activator.GetObject(
              typeof(ICentralDirectory),
              "tcp://localhost:9090/CentralDirectory");

            Node node = new Node(host, port, name,NodeType.Client);
            cd.RegisterClient(node);
            Client clt = new Client(node,channel,puppet,cd);
            ClientPuppet.ctx = clt;
            ClientRemoting.ctx = clt;
            puppet.RegisterPseudoNode(node);
            System.Console.WriteLine(host + ":" + port.ToString());
            Console.WriteLine("Press Enter to Test...");

            //Testes by Bernardo
            clt.Registers[0] = "pEdro";
            clt.Registers[1] = "paUlo";
            clt.Registers[2] = "pReto";
            clt.Registers[3] = "PATO";
            clt.Registers[4] = "porCo";
            clt.Registers[5] = "para";
            clt.Registers[6] = "pedra";
            clt.Registers[7] = "peneu";
            clt.Registers[8] = "paralizado";
            clt.Registers[9] = "princesa";

            clt.ToLowerInternal(2);

            //Testes
           // System.Console.ReadLine();
           // List<string> testList = new List<string>();
           // testList.Add("Afonso");
            //testList.Add("Rui");
            //testList.Add("Chinchila");
           // testList.Add("Power Ranger");
           // testList.Add("Pokemon");
           // testList.Add("Mais uma string qualquer");
            System.Console.ReadLine();    
        
            
        }
    }

    class Client {
        public string[] Registers;
        public Node Info;
        public TcpChannel Channel;
        public List<Node> NetworkTopology;
        public IPuppetMaster Puppet;
        public ICentralDirectory CD;
        Transaction SuperTransaction;
        bool transactionAborted = false;
 
        public Client(Node info,TcpChannel channel,  IPuppetMaster puppet, ICentralDirectory cd){
            Registers = new string[10];
            Info = info;
            Channel = channel;
            Puppet = puppet;
            CD = cd;
        }


        //Adiciona o valor ao registo
        public void StoreInternal(int register, string value)
        {
            Registers[register-1]=value;
        }

        //Executa um Put com o conteudo do registo "register" na key "key"
        public void PutInternal(int register, string key)
        {
            if (SuperTransaction != null &&
                SuperTransaction.PutValue(key, Registers[register - 1]) == false) 
            {
                Console.BackgroundColor = ConsoleColor.Red;
                SuperTransaction = null;
            }
        }

        //Executa um get na key "key" e guarda o conteudo no register
        public void GetInternal(int register, string key)
        {
            string returnValue =null;
            if(SuperTransaction!=null) returnValue=  SuperTransaction.GetValue(key);
            if (returnValue == null)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                SuperTransaction = null;
            }
            else Registers[register - 1] = returnValue; 
             
        }

        //mete o valor "value" na key "key"
        public void PutVAlInternal(string key, string value)
        {
            if (SuperTransaction != null &&
                SuperTransaction.PutValue(key, value) == false)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                SuperTransaction = null;
            }
        }

        //change the value of the key in the register number to lower case
        public void ToLowerInternal(int register)
        {
            Registers[register]=Registers[register].ToLower();

        }

        public void ToUpperInternal(int register)
        {
            Registers[register]=Registers[register].ToUpper();
        }

        //concata a string do registo 2 no registo 1
        public void ConcatInternal(int register1, int register2)
        {
            Registers[register1-1] = Registers[register1-1] + Registers[register2-1];
        }

        //faz commit da transacção
        public void CommitTxInternal()
        {
           if(SuperTransaction!=null) SuperTransaction.Commit();
        }

        public void BeginTxInternal() 
        {
            SuperTransaction = new Transaction();  
        }

        public void WaitInternal(int ms) 
        {
            Thread.Sleep(ms); 
        }
        }
    class ClientRemoting : MarshalByRefObject, IClient
    {

        public static Client ctx;

        public void GetNetworkUpdate(List<Node> network)
        {
            ctx.NetworkTopology = network;
            Console.WriteLine("\nNetwork Topology Update!");
            foreach (Node n in ctx.NetworkTopology)
            {
                Console.WriteLine(n);
            }
        }
    }

    class ClientPuppet : MarshalByRefObject, IClientPuppet
    {
        public static Client ctx;

        public void StartClient()
        {
            ctx.CD.RegisterClient(ctx.Info);
            Console.WriteLine("Client Online");
        }



        public void KillClient()
        {
            ThreadStart ts = delegate() { KillClientThread(); };
            Thread t = new Thread(ts);
            t.Start();
        }

        public void KillClientThread()
        {
            Thread.Sleep(50);
            Environment.Exit(0);
        }

        //Adiciona o valor ao registo
        public void Store(int register, string value)
        {
            ctx.StoreInternal(register, value);
        }

        //Executa um Put com o conteudo do registo "register" na key "key"
        public void Put(int register, string key)
        {
            ctx.PutInternal(register, key);
        }

        //Executa um get na key "key" e guarda o conteudo no register
        public void Get(int register, string key)
        {
            ctx.GetInternal(register, key);
        }

        //mete o valor "value" na key "key"
        public void PutVAl(string key, string value)
        {
            ctx.PutVAlInternal(key, value);
        }

        //change the value of the key in the register number to lower case
        public void ToLower(int register)
        {
            ctx.ToLowerInternal(register - 1);
        }

        public void ToUpper(int register)
        {
            ctx.ToUpperInternal(register - 1);
        }

        //concata a string do registo 2 no registo 1
        public void Concat(int register1, int register2)
        {
            ctx.ConcatInternal(register1, register2);
        }

        //faz commit da transacção
        public void CommitTx()
        {
            ctx.CommitTxInternal();
        }

        public void BeginTx()
        {
            ctx.BeginTxInternal();
        }

        public string[] Dump()
        {
            return ctx.Registers;
        }

        public void Sleep(int ms) 
        {
            ctx.WaitInternal(ms);
        }

        public void ExeScript(List<string> instructions)
        {
            Dictionary<int, List<string>> clientOperations = new Dictionary<int, List<string>>();
            int numberOfTransactions = -1;
            bool inTransaction = false;

            for (; instructions.Count != 0; instructions.Remove(instructions[0]))
            {


                if (instructions[0].StartsWith("BEGINTX"))
                {
                    inTransaction = true;
                    numberOfTransactions++;
                    continue;
                }
                if (instructions[0].StartsWith("COMMITTX"))
                {
                    inTransaction = false;
                    continue;
                }

                if (inTransaction)
                {
                    clientOperations[numberOfTransactions].Add(instructions[0]);
                    continue;
                }
                else
                {
                    char[] delim = { ' ', '\t' };
                    string[] arg = instructions[0].Split(delim);

                    if (instructions[0].StartsWith("GET"))
                    {

                    }
                    else if (instructions[0].StartsWith("PUT"))
                    {

                    }
                    else if (instructions[0].StartsWith("STORE"))
                        this.Store(Int32.Parse(arg[1]), arg[2]);
                    else if (instructions[0].StartsWith("PUTVAL"))
                        this.PutVAl(arg[1], arg[2]);
                    else if (instructions[0].StartsWith("TOLOWER"))
                        this.ToLower(Int32.Parse(arg[1]));
                    else if (instructions[0].StartsWith("TOUPPER"))
                        this.ToUpper(Int32.Parse(arg[1]));
                    else if (instructions[0].StartsWith("CONCAT"))
                        this.Concat(Int32.Parse(arg[1]), Int32.Parse(arg[2]));
                    else if (instructions[0].StartsWith("DUMP"))
                        this.Dump();
                }
            }
        }

        public void ExecuteTransactions(Dictionary<int, List<string>> clientOperations)
        {
            List<Operation> operationsLocations = new List<Operation>();
            char[] delim = { ' ', '\t' };

            for (int aux2 = 1; clientOperations.Count != aux2; aux2++)
            {
               // operationsLocations = GetOperationsLocation(clientOperations[aux2]);

                //Nao ha gets nem puts... é correr tudo....
                if (operationsLocations.Count == 0)
                {
                    for (int aux = 1; clientOperations[aux].Count != aux; aux++)
                    {
                        string[] arg = clientOperations[aux][aux2].Split(delim);

                        if (clientOperations[aux][aux2].StartsWith("STORE"))
                            this.Store(Int32.Parse(arg[1]), arg[2]);
                        else if (clientOperations[aux][aux2].StartsWith("PUTVAL"))
                            this.PutVAl(arg[1], arg[2]);
                        else if (clientOperations[aux][aux2].StartsWith("TOLOWER"))
                            this.ToLower(Int32.Parse(arg[1]));
                        else if (clientOperations[aux][aux2].StartsWith("TOUPPER"))
                            this.ToUpper(Int32.Parse(arg[1]));
                        else if (clientOperations[aux][aux2].StartsWith("CONCAT"))
                            this.Concat(Int32.Parse(arg[1]), Int32.Parse(arg[2]));
                        else if (clientOperations[aux][aux2].StartsWith("DUMP"))
                            this.Dump();
                    }
                }


                for (int aux = 1; clientOperations[aux].Count != aux; aux++)
                {

                    string[] arg = clientOperations[aux][aux2].Split(delim);

                    //standart stuff
                    if (clientOperations[aux][aux2].StartsWith("STORE"))
                        this.Store(Int32.Parse(arg[1]), arg[2]);
                    else if (clientOperations[aux][aux2].StartsWith("PUTVAL"))
                        this.PutVAl(arg[1], arg[2]);
                    else if (clientOperations[aux][aux2].StartsWith("TOLOWER"))
                        this.ToLower(Int32.Parse(arg[1]));
                    else if (clientOperations[aux][aux2].StartsWith("TOUPPER"))
                        this.ToUpper(Int32.Parse(arg[1]));
                    else if (clientOperations[aux][aux2].StartsWith("CONCAT"))
                        this.Concat(Int32.Parse(arg[1]), Int32.Parse(arg[2]));
                    else if (clientOperations[aux][aux2].StartsWith("DUMP"))
                        this.Dump();


                    //get e sets...
                    //else if (clientOperations[aux][aux2].StartsWith("GET"))
                    //{
                    //    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + transaction.NodesLocation[arg[2]] + "/Server");
                    //    ctx.Registers[Int32.Parse(arg[1])] = server.Get(transaction.Txid, arg[2]);

                    //}
                    //else if (clientOperations[aux][aux2].StartsWith("SET"))
                    //{
                    //    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + transaction.NodesLocation[arg[2]] + "/Server");
                    //    server.Put(transaction.Txid, arg[1], ctx.Registers[Int32.Parse(arg[1])]);
                    //}
                }

            }

        }

    }

    class Transaction
    {

        List<string> AccessedKeys;
        Dictionary<string, List<Node>> NodesLocation;
        List<Node> Nodes;
        TransactionContext Tctx;
        ICentralDirectory Central;

        public Transaction()
        {
            AccessedKeys = new List<string>();
            NodesLocation = new Dictionary<string, List<Node>>();
            Nodes = new List<Node>();
            Central = (ICentralDirectory)Activator.GetObject(
              typeof(ICentralDirectory),
              "tcp://localhost:9090/CentralDirectory");
            while (true)
            {
                Tctx = Central.BeginTx();
                if (Tctx.Txid != -1)
                    break;

                Thread.Sleep(500);

            }
            Console.WriteLine(Tctx);

        }

        List<Node> GetAndLockKey(string key)
        {
            List<Node> nodes = Central.GetServers(key);
            foreach (Node n in nodes)
                if (!Nodes.Contains(n))
                    Nodes.Add(n);
            NodesLocation.Add(key, nodes);
            bool allCanLock = true;
            bool allLocked = true;
            foreach (Node n in nodes)
            {
                IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                if (!server.CanLock(Tctx.Txid, key)) allCanLock = false;
            }
            if (!allCanLock)
            {
                Abort();
                return null;
            }
            else
            {
                foreach (Node n in nodes)
                {
                    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                    if (!server.Lock(Tctx.Txid, key))
                    {
                        allLocked = false;
                        break;
                    }
                }
                if (!allLocked)
                {
                    Abort();
                    return null;
                }
            }
            return nodes;
        }

        public string GetValue(string key)
        {
            List<Node> nodes = null;
            if (AccessedKeys.Contains(key))
            {
                nodes = NodesLocation[key];
            }
            else
            {
                nodes = GetAndLockKey(key);
                AccessedKeys.Add(key);
            }
            if (nodes != null)
            {
                IServer serv = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[0].IP + ":" + nodes[0].Port.ToString() + "/Server");
                string value;
                try
                {
                    value = serv.Get(Tctx.Txid, key);
                }
                catch
                {
                    IServer servBackup = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[1].IP + ":" + nodes[1].Port.ToString() + "/Server");
                    try
                    {
                        value = servBackup.Get(Tctx.Txid, key);
                        Central.ServerDown(nodes[0]);
                    }
                    catch
                    {
                        value = null;
                        throw;
                    }
                    throw;
                }
                if (value == null)
                {
                    Abort();
                    return null;
                }
                else return value;
            }
            else return null;
        }

        public bool PutValue(string key, string value)
        {
            List<Node> nodes = null;
            if (AccessedKeys.Contains(key))
            {
                nodes = NodesLocation[key];
            }
            else
            {
                nodes = GetAndLockKey(key);
                AccessedKeys.Add(key);
            }
            if (nodes != null)
            {
                IServer serv = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[0].IP + ":" + nodes[0].Port.ToString() + "/Server");
                bool success;
                try
                {
                    success = serv.Put(Tctx.Txid, key, value);
                }
                catch
                {
                    IServer servBackup = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[1].IP + ":" + nodes[1].Port.ToString() + "/Server");
                    try
                    {
                        success = servBackup.Put(Tctx.Txid, key, value);
                        Central.ServerDown(nodes[0]);
                    }
                    catch
                    {
                        success = false;
                        throw;
                    }
                    throw;
                }
                if (!success)
                {
                    Abort();
                    return false;
                }
                else return success;
            }
            else return false;
        }

        public bool Commit()
        {
            bool allCanCommit = true;
            Node nodeToRemove = null;
            foreach (Node n in Nodes)
            {
                try
                {
                    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                    if (!server.CanCommit(Tctx.Txid))
                    {
                        allCanCommit = false;
                        break;
                    }
                }
                catch
                {
                    nodeToRemove = n;
                    Central.ServerDown(n);
                }
            }
            if (nodeToRemove != null) Nodes.Remove(nodeToRemove);
            Tctx.State = TransactionContext.states.tentatively;
            Console.WriteLine(Tctx);
            if (!allCanCommit)
            {
                Abort();
                return false;
            }
            else
            {
                foreach (Node n in Nodes)
                {
                    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                    server.Commit(Tctx.Txid);
                }
                Tctx.State = TransactionContext.states.commited;
                Central.UpdateTransactionState(Tctx);
                Console.WriteLine(Tctx);
                return true;
            }
        }

        public void Abort()
        {
            foreach (Node n in Nodes)
            {
                IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                server.Abort(Tctx.Txid);
            }
            Tctx.State = TransactionContext.states.aborted;
            Central.UpdateTransactionState(Tctx);
            Console.WriteLine(Tctx);

        }

    }

    }

  
