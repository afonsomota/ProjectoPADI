using System;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Cryptography;
using System.Threading;
using CommonInterfaces;





namespace CentralDirectory
{
    class Program
    {


        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(9090);
            ChannelServices.RegisterChannel(channel, true);

            CentralDirectory central = new CentralDirectory();
            CentralDirectoryRemoting.ctx = central;

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(CentralDirectoryRemoting), "CentralDirectory", WellKnownObjectMode.Singleton);
            System.Console.ReadLine();


            CentralDirectoryRemoting.ctx.division();

            foreach (CentralDirectory.Interval n in central.Location)
            {
                Console.WriteLine(n.min+" - "+n.max + ":  " + n.IP[0] + ";  " + n.IP[1]);
            }

            

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }

    class CentralDirectory{
       public class Interval{
          public uint min;
          public uint max;
          public List<Node> IP;
          public bool order;
        };

        public  bool firstPut = false;
       List<CommonInterfaces.Node> listClient = new List<CommonInterfaces.Node>();
       List<CommonInterfaces.Node> listServer = new List<CommonInterfaces.Node>();
       List<Interval> tableOfLocation = new List<Interval>();
       uint max = UInt32.MaxValue;


       public CentralDirectory()
       {
           listServer = new List<CommonInterfaces.Node>();
           listClient = new List<CommonInterfaces.Node>();
           tableOfLocation = new List<Interval>();

       }

       public List<Interval> Location
       {
           get
           {
               return tableOfLocation;
           }
       }


       public List<CommonInterfaces.Node> ListClient
       {
           get
           {
               return listClient;
           }
       }

       public CommonInterfaces.Node Client
       {
           set
           {
               listClient.Add(value);
           }
       }

       public CommonInterfaces.Node Server
       {
           set
           {
               listServer.Add(value);
           }
       }

       public List<CommonInterfaces.Node> ListServer
       {
           get
           {
               return listServer;
           }
       }


       //provavelmente não deve ser necessária
       public CommonInterfaces.Node getNode(string IP)
       {

           for (int i = 0; i < listServer.Count(); i++)
           {
               if (listServer[i].IP + ":" + listServer[i].Port.ToString() == IP)
               {
                   
                   return listServer[i];
               }
           }
           return null;
       }

       public void division()
       {
           firstPut = true;
           uint numberofServer = (uint)listServer.Count();
           uint result = 0;
           uint aux1 = 0;
           
           //int aux3 = 0;
           result = max / numberofServer;
           uint aux2 = result;
           
           List<string> aux4 = new List<string>();

           if (numberofServer == 1) {
               Node srv = new Node(listServer[0].IP, listServer[0].Port, NodeType.Server);
               Interval st1 = new Interval();
               st1.IP = new List<Node>();
               st1.min = UInt32.MinValue;
               st1.max = UInt32.MaxValue/2;
               st1.IP.Add(srv);
               st1.IP.Add(srv);
               Interval st2 = new Interval();
               st2.IP = new List<Node>();
               st2.min = UInt32.MaxValue / 2 + 1;
               st2.max = UInt32.MaxValue;
               st2.IP.Add(srv);
               st2.IP.Add(srv);

               IServer link1 = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + srv.IP + ":" + srv.Port.ToString() + "/Server");
               link1.GetInitialIntervals(st1.min, st1.max, srv);
               IServer link2 = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + srv.IP + ":" + srv.Port.ToString() + "/Server");
               link2.GetInitialIntervals(st2.min, st2.max, srv);

               tableOfLocation.Add(st1);
               tableOfLocation.Add(st2);

               return;
           }

           for (int i = 0; i < numberofServer; i++)
           {

               Interval st = new Interval();
               st.IP = new List<Node>();
               st.min = aux1;
               st.max = aux2;
              
               
               if (i == numberofServer - 1)
               {
                   Node node1 = new Node(listServer[i].IP, listServer[i].Port, NodeType.Server);
                   Node a = new Node (listServer[0].IP,listServer[0].Port, NodeType.Server);
                   //node1.IP = listServer[i].IP;
                   //node1.Port = listServer[i].Port;
                   //node2.IP = listServer[0].IP;
                   //node2.Port = listServer[0].Port;
                   st.IP.Add(node1);
                   st.IP.Add(a);
                   //st.IP.Add(listServer[i].IP + ":" + listServer[i].Port.ToString());
                   //st.IP.Add(listServer[0].IP + ":" + listServer[0].Port.ToString());

                   IServer link1 = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + listServer[i].IP + ":" + listServer[i].Port.ToString() + "/Server");
                   link1.GetInitialIntervals(aux1, aux2,listServer[0]);

                   IServer link2 = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + listServer[0].IP + ":" + listServer[0].Port.ToString() + "/Server");
                   link2.GetInitialIntervals(aux1, aux2,listServer[i]);


               }
               else
               {
                   Node node1 = new Node(listServer[i].IP, listServer[i].Port, NodeType.Server);
                   Node a = new Node(listServer[i+1].IP, listServer[i+1].Port, NodeType.Server);
                   //node1.IP = listServer[i].IP;
                   //node1.Port = listServer[i].Port;
                   //node2.IP = listServer[i + 1].IP;
                   //node2.Port = listServer[i + 1].Port;
                   st.IP.Add(node1);
                   st.IP.Add(a);
                   //st.IP.Add(listServer[i].IP + ":" + listServer[i].Port.ToString());
                   //st.IP.Add(listServer[i + 1].IP + ":" + listServer[i + 1].Port.ToString());

                   IServer link1 = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + listServer[i].IP + ":" + listServer[i].Port.ToString() + "/Server");
                   link1.GetInitialIntervals(aux1, aux2,listServer[i+1]);

                   IServer link2 = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + listServer[i + 1].IP + ":" + listServer[i + 1].Port.ToString() + "/Server");
                   link2.GetInitialIntervals(aux1, aux2,listServer[i]);
               }

               tableOfLocation.Add(st);


               if (aux1 == 0)
                   aux1 = aux1 + result + 1;
               else aux1 = aux1 + result;
               
               aux2 = aux2 + result;

           }

           
       }

       public uint MaxSemiTable(List<Dictionary<uint, int>> table)
       {
           
           int max = 0;
           for (int i = 0; i < table.Count; i++)
           {
               foreach (KeyValuePair<uint, int> pair in table[i])
                   Console.WriteLine("Valor Maximo: " + pair.Value + "Key: " + pair.Key);
           }
           uint semitableint = 0;

           for (int i = 0; i < table.Count; i++)
           {
               foreach (KeyValuePair<uint, int> pair in table[i])
               {
                   if (max < pair.Value)
                   {
                       max = pair.Value;
                       semitableint = pair.Key;
                       break;
                   }
               }
           }
           Console.WriteLine("resultado:" + semitableint); 
            return semitableint;
       }

       public static uint MD5Hash(string input)
       {
           System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
           byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
           byte[] hash = x.ComputeHash(bs);
           uint interval = (uint)((hash[0] ^ hash[4] ^ hash[8] ^ hash[12]) << 24) +
                                  (uint)((hash[1] ^ hash[5] ^ hash[9] ^ hash[13]) << 16) +
                                 (uint)((hash[2] ^ hash[6] ^ hash[10] ^ hash[14]) << 8) +
                                 (uint)(hash[3] ^ hash[7] ^ hash[11] ^ hash[15]);
           return interval;
       }

        public void Restructure(uint semiTable,Node d){
            uint max_aux = 0;
            uint numberofServer = (uint)listServer.Count();


            if (numberofServer == 1)
            {
                Console.WriteLine("Replicating... ");
                IServer oldNode = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + Location[0].IP[1].IP + ":" + Location[0].IP[1].Port.ToString() + "/Server");
                oldNode.CopySemiTables(d);
                Location[0].IP[1] = d;
                Location[1].IP[0] = d;
                return;
            }

            for (int i = 0; i < Location.Count(); i++)
            {

                if (Location[i].min < semiTable && Location[i].max > semiTable)
                {
                    Console.WriteLine("Restructuring... " + semiTable);

                    Interval st = new Interval();
                    st.IP = new List<Node>();
                    max_aux = Location[i].max;
                    Location[i].max = semiTable - 1;
                    st.min = semiTable;
                    st.max = max_aux;
                    st.IP.Add(d);
                    st.IP.Add(Location[i].IP[1]);


                    tableOfLocation.Add(st);
                    IServer link1 = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + Location[i].IP[0].IP + ":" + Location[i].IP[0].Port.ToString() + "/Server");
                    link1.CleanSemiTable(semiTable,d);
                    IServer link2 = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + Location[i].IP[1].IP + ":" + Location[i].IP[1].Port.ToString() + "/Server");
                    link2.CopyAndCleanTable(semiTable, d);
                    Location[i].IP[1] = d;
                    break;
                }
            }

            for (int i = 0; i < Location.Count; i++)
            {
                Console.WriteLine("min: " + Location[i].min + " max: " + Location[i].max + "   IP1: " + Location[i].IP[0].Port + "  IP2: " + Location[i].IP[1].Port);
            }
               //link.GetNetworkUpdate(listUpDate);
             //CleanSemiTable(uint semiTableToClean);
            //CopyAndCleanTable(uint semiTableToClean);
        }

        public void Send(List<CommonInterfaces.Node> clients, List<CommonInterfaces.Node> servers)
       {
           ThreadStart ts = delegate() { UpDate(clients, servers); };
           Thread t = new Thread(ts);
           t.Start();
       }

       public void UpDate(List<CommonInterfaces.Node> clients, List<CommonInterfaces.Node> servers)
       {
           List<Node> listUpDate = new List<Node>();
           listUpDate.AddRange(clients);
           listUpDate.AddRange(servers);
           foreach (CommonInterfaces.Node node in clients)
           {
               IClient link = (IClient)Activator.GetObject(typeof(IClient), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Client");
               try
               {
                   if (link != null) link.GetNetworkUpdate(listUpDate);
               }
               catch { }
           }
           foreach (CommonInterfaces.Node node in servers)
           {
               IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
               if (link != null) link.GetNetworkUpdate(listUpDate);
           }
       }

       public Dictionary<uint,int> SendDimension(List<CommonInterfaces.Node> servers)
       {
           ThreadStart ts = delegate() { DimensionOfServers(servers); };
           Thread t = new Thread(ts);
           t.Start();
           return null;
       }


       public List<Dictionary<uint,int>>  DimensionOfServers(List<CommonInterfaces.Node> servers)
       {
           List<Dictionary<uint, int>> listAux = new List<Dictionary<uint, int>>();
            foreach (CommonInterfaces.Node node in servers)
           {
               IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
               listAux.Add(link.GetSemiTablesCount()); 
           }
            return listAux;
       }
    }
    class CentralDirectoryRemoting : MarshalByRefObject, CommonInterfaces.ICentralDirectory 
    {

        public static CentralDirectory ctx;
        public int txid = 0;
        public uint semiTablemin1 = 0;
        public uint semiTablemax1 = 0;
        public uint semiTablemin2 = 0;
        public uint semiTablemax2 = 0;
        public bool miss = false;
        
        public bool RegisterClient(CommonInterfaces.Node node)
        {
            Console.WriteLine("Registred " + node.IP + " on port " + node.Port);
            if (ctx.ListClient.Contains(node))
            {
                return false;
            }

            
            ctx.Client = node;
            ctx.Send(ctx.ListClient, ctx.ListServer);
            return true;
        }

        public bool RegisterServer(CommonInterfaces.Node node)
        {
            Console.WriteLine("Registred" + node.IP + "on port" + node.Port);
            List<Dictionary<uint, int>> listAux = new List<Dictionary<uint, int>>();

            if (miss == true)
            {
                for (int i = 0; i < ctx.Location.Count; i++)
                {
                    if (ctx.Location[i].min == semiTablemin1 && ctx.Location[i].max == semiTablemax1)
                    {
                        Console.WriteLine("Sending restructure request to: " + ctx.Location[i].IP[0]);
                        IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + ctx.Location[i].IP[0].IP + ":" + ctx.Location[i].IP[0].Port.ToString() + "/Server");
                        ctx.Location[i].IP.Add(node);
                        link.CopySemiTable(ctx.Location[i].min,node);
                    }

                    else if (ctx.Location[i].min == semiTablemin2 && ctx.Location[i].max == semiTablemax2)
                    {
                        Console.WriteLine("Sending restructure request to: " + ctx.Location[i].IP[0]);
                        IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + ctx.Location[i].IP[0].IP + ":" + ctx.Location[i].IP[0].Port.ToString() + "/Server");
                        ctx.Location[i].IP.Add(node);
                        link.CopySemiTable(ctx.Location[i].min, node);
                    }
                }
                miss = false;
            }

            
            else if (ctx.firstPut == true)
            {
                listAux = ctx.DimensionOfServers(ctx.ListServer);
                ctx.Restructure(ctx.MaxSemiTable(listAux), node);
            }

                       
            ctx.Server = node;
            ctx.Send(ctx.ListClient, ctx.ListServer);
            return true;
        }

        
        

        //falta completar
        /*public Dictionary<string, List<CommonInterfaces.Node>> GetServers(List<string> keys)
        {
            Dictionary<string, List<CommonInterfaces.Node>> server = new Dictionary<string, List<CommonInterfaces.Node>>();
            List<CommonInterfaces.Node> listaux = new List<CommonInterfaces.Node>();
            Random rd = new Random();
            
            CommonInterfaces.TransactionContext transactionsContext = new CommonInterfaces.TransactionContext();
            transactionsContext.Txid = rd.Next();

            for (int i = 0; i < keys.Count(); i++)
            {   
                string aux2 = keys[i];
                string aux = MD5Hash(keys[i]);
                int aux5 = Convert.ToInt32(aux); //verificar
                for(int j = 0; j < tableOfLocation.Count(); j++){
                    if(tableOfLocation[i].max > aux5 && tableOfLocation[i].min < aux5){
                        listaux.Add(getNode(tableOfLocation[i].IP[0]));
                        listaux.Add(getNode(tableOfLocation[i].IP[1]));
                        server.Add(aux2,listaux);

                    }
            }
    }
            transactionsContext.NodesLocation = server;
            return server;
        }
        */

        public CommonInterfaces.TransactionContext GetServers(List<Operation> ops)
        {
            Dictionary<string, List<CommonInterfaces.Node>> server = new Dictionary<string, List<CommonInterfaces.Node>>();
            //Random rd = new Random();

            CommonInterfaces.TransactionContext transactionsContext = new CommonInterfaces.TransactionContext();
            transactionsContext.Txid=txid;
            txid++;
            Console.WriteLine("transacao identificador" + transactionsContext.Txid);
            Console.WriteLine(txid);
            List<string> keys = new List<string>();


            Dictionary<int, Operation> opsd = new Dictionary<int, Operation>();
            int o = 0;
            foreach (Operation op in ops) {
                opsd.Add(++o, op);
                if(!keys.Contains(op.Key)) keys.Add(op.Key);
                if (op.Type == OpType.PUT)
                {
                    if (ctx.firstPut == false)
                    {
                        ctx.division();
                        ctx.firstPut = true;
                    }
                }
                    
            }

            for (int i = 0; i < keys.Count(); i++)
            {
                string key = keys[i];
                uint hash = CentralDirectory.MD5Hash(keys[i]);
                
                
                for (int j = 0; j < ctx.Location.Count(); j++)
                {
                    if (ctx.Location[j].max > hash && ctx.Location[j].min <= hash)
                    {
                        List<CommonInterfaces.Node> listaux = new List<CommonInterfaces.Node>();
                        Console.WriteLine("For key " + key + " hash is " + hash);
                        if (ctx.Location[j].order)
                        {
                            listaux.Add(ctx.Location[j].IP[0]);
                            listaux.Add(ctx.Location[j].IP[1]);
                            Console.WriteLine("0-" + ctx.Location[j].IP[0] + " 1-" + ctx.Location[j].IP[1]);
                            ctx.Location[j].order = false;
                        }
                        else {
                            listaux.Add(ctx.Location[j].IP[1]);
                            listaux.Add(ctx.Location[j].IP[0]);
                            Console.WriteLine("1-" + ctx.Location[j].IP[1] + " 0-" + ctx.Location[j].IP[0]);
                            ctx.Location[j].order = true;
                        }
                        server.Add(key, listaux);
                        
                    }
                }
            }
            transactionsContext.NodesLocation = server;
            transactionsContext.Operations = opsd;
            return transactionsContext;
        }

        

        public void ServerDown(CommonInterfaces.Node server)
        {
           int j = 1;
           ctx.ListServer.Remove(server);
           for (int i = 0; i < ctx.Location.Count; i++)
           {
               if (ctx.Location[i].IP[0].Port == server.Port || ctx.Location[i].IP[1].Port == server.Port)
               {

                   Node nodeToRemove = null;
                  if (j == 1)
                  {
                       semiTablemin1 = ctx.Location[i].min;
                       semiTablemax1 = ctx.Location[i].max;
                       j = 2;
                   }
                   else if (j == 2)
                   {
                       semiTablemin2 = ctx.Location[i].min;
                       semiTablemax2 = ctx.Location[i].max;
                       j = 1;
                   }
                  foreach (Node n in ctx.Location[i].IP)
                      if (n.IP == server.IP && n.Port == server.Port)
                          nodeToRemove = n;
                  ctx.Location[i].IP.Remove(nodeToRemove);
               }

               

           }
            
           miss = true;
           Console.WriteLine("min1 - " + semiTablemin1 + " max1 - " + semiTablemax1);
           Console.WriteLine("min2 - " + semiTablemin2 + " max2 - " + semiTablemax2);
           ctx.Send(ctx.ListClient, ctx.ListServer);
        }
        

    }

}