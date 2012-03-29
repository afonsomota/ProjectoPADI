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

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(CentralDirectory), "CentralDirectory", WellKnownObjectMode.Singleton);


            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }

    class CentralDirectory : MarshalByRefObject, CommonInterfaces.ICentralDirectory 
    {
       public struct interval{
          public int min;
          public int max;
          public List<string> IP;
        };

        List<CommonInterfaces.Node> listClient = new List<CommonInterfaces.Node>();
        List<CommonInterfaces.Node> listServer = new List<CommonInterfaces.Node>();
        List<interval> tableOfLocation = new List<interval>();
        int max = Int32.MaxValue;
        
        
        public CentralDirectory()
        {
            listServer = new List<CommonInterfaces.Node>();
            listClient = new List<CommonInterfaces.Node>();
            tableOfLocation = new List<interval>();
            
        }

        public List<interval> Location
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
                if (listServer[i].IP == IP)
                {
                    return listServer[i];
                }
            }
            return null;
        }

        public bool RegisterClient(CommonInterfaces.Node node)
        {
            Console.WriteLine("Registred " + node.IP + " on port " + node.Port);
            if (listClient.Contains(node))
            {
                return false;
            }
            
            Client = node;
            Send(listClient, listServer);
            return true;
        }

        public bool RegisterServer(CommonInterfaces.Node node)
        {
            Console.WriteLine("Registred" + node.IP + "on port" + node.Port);
            if(listServer.Contains(node)){
                return false;
            }

            Server = node;
            Send(listClient, listServer);
            return true;
        }

        
        public void division()
        {
            int numberofServer = listServer.Count();
            int result = 0;
            int aux1 = 0;
            int aux2 = result;
            int aux3 = 0;
            result = max / numberofServer;
            interval st = new interval();
            st.IP = new List<string> ();
            List<string> aux4 = new List<string>();
            
            for (int i = 0; i < numberofServer; i++)
            {
                st.min = aux1;
                st.max = aux2;
                if (i == numberofServer)
                {
                    st.IP.Add(listClient[aux3].IP);
                    st.IP.Add(listClient[0].IP);
                }

                st.IP.Add(listClient[aux3].IP);
                st.IP.Add(listClient[aux3 + 1].IP);

                tableOfLocation.Add(st);

                aux1 = aux1 + result;
                aux2 = aux2 + result;
                aux3 = aux3 + 1;
                st.IP.Clear();

            }
         }

        public string SHA1Hash(string input)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] data = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = sha.ComputeHash(data);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
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
                string aux = SHA1Hash(keys[i]);
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

        public CommonInterfaces.TransactionContext GetServers(List<string> keys)
        {
            Dictionary<string, List<CommonInterfaces.Node>> server = new Dictionary<string, List<CommonInterfaces.Node>>();
            List<CommonInterfaces.Node> listaux = new List<CommonInterfaces.Node>();
            Random rd = new Random();

            CommonInterfaces.TransactionContext transactionsContext = new CommonInterfaces.TransactionContext();
            transactionsContext.Txid=rd.Next();

            for (int i = 0; i < keys.Count(); i++)
            {
                string aux2 = keys[i];
                string aux = SHA1Hash(keys[i]);
                Console.WriteLine(aux);
                int aux5 = Convert.ToInt32(aux); //verificar
                Console.WriteLine(aux5);
                for (int j = 0; j < tableOfLocation.Count(); j++)
                {
                    if (tableOfLocation[i].max > aux5 && tableOfLocation[i].min < aux5)
                    {
                        listaux.Add(getNode(tableOfLocation[i].IP[0]));
                        listaux.Add(getNode(tableOfLocation[i].IP[1]));
                        server.Add(aux2, listaux);

                    }
                }
            }
            transactionsContext.NodesLocation = server;
            return transactionsContext;
        }

        public void ServerDown(CommonInterfaces.Node server)
        {
           CommonInterfaces.Node node = getNode(server.IP);
           listServer.Remove(node);
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
                IClient link = (IClient)Activator.GetObject(typeof(IClient), "tcp://"+node.IP+":" + node.Port.ToString() + "/Client");
                link.GetNetworkUpdate(listUpDate);
            }
            foreach (CommonInterfaces.Node node in servers)
            {
                IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
                link.GetNetworkUpdate(listUpDate);
            }
        }

    }

}