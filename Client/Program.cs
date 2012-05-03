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
            int port = 0;
            if (args.Length > 1) port = Int32.Parse(args[1]);
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);

            ChannelDataStore channelData = (ChannelDataStore)channel.ChannelData;
            port = new System.Uri(channelData.ChannelUris[0]).Port;
            string host = new System.Uri(channelData.ChannelUris[0]).Host;
            string name = null;
            if (args.Length > 0) name = args[0];
            else name = "Debug" + port.ToString();

            Console.WriteLine("Ciente: " + name);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientPuppet), "ClientPuppet", WellKnownObjectMode.Singleton);
           
            IPuppetMaster puppet = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
               "tcp://localhost:8090/PseudoNodeReg");

            ICentralDirectory cd = (ICentralDirectory)Activator.GetObject(
              typeof(ICentralDirectory),
              "tcp://localhost:9090/CentralDirectory");

            Node node = new Node(host, port, name,NodeType.Client);
            //cd.RegisterClient(node);
            Client clt = new Client(node,channel,puppet,cd);
            ClientPuppet.ctx = clt;
            ClientRemoting.ctx = clt;
            try { puppet.RegisterPseudoNode(node); }
            catch { System.Console.WriteLine("We Dont Need No Puppet Master"); } 
            System.Console.WriteLine(host + ":" + port.ToString());
            Console.WriteLine("Press Enter to Test...");
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

        public void RunScriptInternal(List<string> operations) 
        {
            foreach(string operation in operations){
            char[] delim = { ' ' };
            string[] arg = operation.Split(delim);

            if (arg[0] == "BEGINTX") BeginTxInternal();
            else if (arg[0] == "STORE") StoreInternal(Int32.Parse(arg[2]), arg[3]);
            else if (arg[0] == "PUT") PutInternal(Int32.Parse(arg[2]), arg[3]);
            else if (arg[0] == "GET") GetInternal(Int32.Parse(arg[2]), arg[3]);
            else if (arg[0] == "PUTVAL") PutVAlInternal(arg[2], arg[3]);
            else if (arg[0] == "TOLOWER") ToLowerInternal(Int32.Parse(arg[2]));
            else if (arg[0] == "TOUPPER") ToUpperInternal(Int32.Parse(arg[2]));
            else if (arg[0] == "CONCAT") ConcatInternal(Int32.Parse(arg[2]), Int32.Parse(arg[3]));
            else if (arg[0] == "COMMITTX") CommitTxInternal();
            else if (arg[0] == "WAIT") WaitInternal(Int32.Parse(arg[2]));    
        
            }
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

        public void Runscript(List<string> operations) 
        {
                ThreadStart ts = delegate() { ctx.RunScriptInternal(operations); };
                Thread t = new Thread(ts);
                t.Start();       
        }

        public void KillClient()
        {
            ThreadStart ts = delegate() { KillClientThread(); };
            Thread t = new Thread(ts);
            t.Start();
        }

        public void KillClientThread()
        {
            ChannelServices.UnregisterChannel(ctx.Channel);
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

        //public void ExeScript(List<string> instructions)
        //{


        //    for (; instructions.Count != 0; instructions.Remove(instructions[0]))
        //    {


        //        if (instructions[0].StartsWith("BEGINTX"))

        //        if (instructions[0].StartsWith("COMMITTX"))


        //       if (instructions[0].StartsWith("GET"))


        //       if (instructions[0].StartsWith("PUT"))

        //        if (instructions[0].StartsWith("STORE"))
        //            this.Store(Int32.Parse(arg[1]), arg[2]);
        //        else if (instructions[0].StartsWith("PUTVAL"))
        //            this.PutVAl(arg[1], arg[2]);
        //        else if (instructions[0].StartsWith("TOLOWER"))
        //            this.ToLower(Int32.Parse(arg[1]));
        //        else if (instructions[0].StartsWith("TOUPPER"))
        //            this.ToUpper(Int32.Parse(arg[1]));
        //        else if (instructions[0].StartsWith("CONCAT"))
        //            this.Concat(Int32.Parse(arg[1]), Int32.Parse(arg[2]));
        //        else if (instructions[0].StartsWith("DUMP"))
        //                this.Dump();
        //        }
        //    }
        }

        //public void ExecuteTransactions(Dictionary<int, List<string>> clientOperations)
        //{
        //    char[] delim = { ' ', '\t' };

        //    for (int aux = 1; clientOperations[aux].Count != aux; aux++)
        //    {
        //        string[] arg = clientOperations[aux][aux2].Split(delim);

        //        if (clientOperations[aux][aux2].StartsWith("STORE"))
        //            this.Store(Int32.Parse(arg[1]), arg[2]);
        //        else if (clientOperations[aux][aux2].StartsWith("PUTVAL"))
        //            this.PutVAl(arg[1], arg[2]);
        //        else if (clientOperations[aux][aux2].StartsWith("TOLOWER"))
        //            this.ToLower(Int32.Parse(arg[1]));
        //        else if (clientOperations[aux][aux2].StartsWith("TOUPPER"))
        //            this.ToUpper(Int32.Parse(arg[1]));
        //        else if (clientOperations[aux][aux2].StartsWith("CONCAT"))
        //            this.Concat(Int32.Parse(arg[1]), Int32.Parse(arg[2]));
        //        else if (clientOperations[aux][aux2].StartsWith("DUMP"))
        //            this.Dump();
        //    }
        //}

    class Transaction
    {

        List<string> AccessedKeys;
        Dictionary<string, List<Node>> NodesLocation;
        List<Node> Nodes;
        TransactionContext Tctx;
        ICentralDirectory Central;
        bool readOnlyTransaction = false;
        bool performedPutOperation = false;

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
            Console.WriteLine(nodes);
            bool allCanLock = true;
            bool allLocked = true;
            int serversDown = 0;
            if(!readOnlyTransaction)
                foreach (Node n in nodes)
                {
                    try
                    {
                        IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                        char canLock = server.CanLock(Tctx.Txid, key);
                        Console.WriteLine("CanLock returned: " + canLock);
                        if (canLock == 'n') allCanLock = false;
                        else if (canLock == 'r') {
                            if (performedPutOperation) allCanLock = false;
                            else readOnlyTransaction = true; 
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        serversDown++;
                        Central.ServerDown(n);
                    }
                }
            if (!allCanLock || serversDown == 2)
            {
                Abort();
                return null;
            }
            else if(!readOnlyTransaction)
            {
                serversDown = 0;
                foreach (Node n in nodes)
                {
                    try
                    {
                        IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                        if (!server.Lock(Tctx.Txid, key))
                        {
                            allLocked = false;
                            break;
                        }
                    }
                    catch
                    {
                        serversDown++;
                        Central.ServerDown(n);
                    }
                }
                if (!allLocked || serversDown == 2)
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
            Console.WriteLine("Get(" + key + ");");
            if (nodes != null)
            {
                string value;
                try
                {
                    IServer serv = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[0].IP + ":" + nodes[0].Port.ToString() + "/Server");
                    if (readOnlyTransaction) value = serv.GetStable(key);
                    else value = serv.Get(Tctx.Txid, key);
                    Console.WriteLine("Value= " + value);
                }
                catch
                {
                    try
                    {
                    IServer servBackup = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[1].IP + ":" + nodes[1].Port.ToString() + "/Server");
                    
                        if (readOnlyTransaction) value = servBackup.GetStable(key);
                        else value = servBackup.Get(Tctx.Txid, key);
                        Central.ServerDown(nodes[0]);
                    }
                    catch
                    {
                        value = null;
                    }
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
            performedPutOperation = true;
            if (readOnlyTransaction) {
                Abort();
                return false;
            }
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
                bool success = true;
                try
                {
                    IServer serv = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[0].IP + ":" + nodes[0].Port.ToString() + "/Server");
                    success = serv.Put(Tctx.Txid, key, value);
                }
                catch
                {
                    try
                    {
                        IServer servBackup = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + nodes[1].IP + ":" + nodes[1].Port.ToString() + "/Server");
                        success = servBackup.Put(Tctx.Txid, key, value) && success;
                        Central.ServerDown(nodes[0]);
                    }
                    catch
                    {
                        success = false;
                    }
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
            int serversDown = 0;
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
                    serversDown++;
                    nodeToRemove = n;
                    Central.ServerDown(n);
                }
            }
            if (nodeToRemove != null) Nodes.Remove(nodeToRemove);
            Tctx.State = TransactionContext.states.tentatively;
            Console.WriteLine(Tctx);
            if (!allCanCommit || serversDown == 2)
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
                try
                {
                    IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + n.IP + ":" + n.Port.ToString() + "/Server");
                    server.Abort(Tctx.Txid);
                }
                catch
                {
                    Central.ServerDown(n);
                }
            }
            Tctx.State = TransactionContext.states.aborted;
            Central.UpdateTransactionState(Tctx);
            Console.WriteLine(Tctx);

        }

    }

    }

  
