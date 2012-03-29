﻿using System;

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

            foreach (CentralDirectory.interval n in central.Location)
            {
                Console.WriteLine(n.min+" - "+n.max + ":" + n.IP[0] + ";" + n.IP[1]);
            }

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }

    class CentralDirectory{
       public struct interval{
          public uint min;
          public uint max;
          public List<string> IP;
        };

       List<CommonInterfaces.Node> listClient = new List<CommonInterfaces.Node>();
       List<CommonInterfaces.Node> listServer = new List<CommonInterfaces.Node>();
       List<interval> tableOfLocation = new List<interval>();
       uint max = UInt32.MaxValue;


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

       public void division()
       {
           uint numberofServer = (uint)listServer.Count();
           uint result = 0;
           uint aux1 = 0;
           
           //int aux3 = 0;
           result = max / numberofServer;
           uint aux2 = result;
           
           List<string> aux4 = new List<string>();

           for (int i = 0; i < numberofServer; i++)
           {

               interval st = new interval();
               st.IP = new List<string>();
               st.min = aux1;
               st.max = aux2;
               if (i == numberofServer - 1)
               {
                   st.IP.Add(listServer[i].IP + ":" + listServer[i].Port.ToString());
                   st.IP.Add(listServer[0].IP + ":" + listServer[0].Port.ToString());
               }
               else
               {
                   st.IP.Add(listServer[i].IP + ":" + listServer[i].Port.ToString());
                   st.IP.Add(listServer[i + 1].IP + ":" + listServer[i + 1].Port.ToString());
               }

               tableOfLocation.Add(st);

               aux1 = aux1 + result;
               aux2 = aux2 + result;

           }
       }

       public uint SHA1Hash(string input)
       {
           SHA1 sha = new SHA1CryptoServiceProvider();
           byte[] data = System.Text.Encoding.ASCII.GetBytes(input);
           byte[] hash = sha.ComputeHash(data);
           uint interval = (uint)((hash[0] ^ hash[4] ^ hash[8] ^ hash[12] ^ hash[16]) << 24) +
                                  (uint)((hash[1] ^ hash[5] ^ hash[9] ^ hash[13] ^ hash[17]) << 16) +
                                 (uint)((hash[2] ^ hash[6] ^ hash[10] ^ hash[14] ^ hash[18]) << 8) +
                                 (uint)(hash[3] ^ hash[7] ^ hash[11] ^ hash[15] ^ hash[19]);


           /*StringBuilder sb = new StringBuilder();
           for (int i = 0; i < hash.Length; i++)
           {
               sb.Append(hash[i].ToString("X2"));
           }
           return sb.ToString();*/
           return interval;
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
               link.GetNetworkUpdate(listUpDate);
           }
           foreach (CommonInterfaces.Node node in servers)
           {
               IServer link = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + node.IP + ":" + node.Port.ToString() + "/Server");
               link.GetNetworkUpdate(listUpDate);
           }
       }

    }
    class CentralDirectoryRemoting : MarshalByRefObject, CommonInterfaces.ICentralDirectory 
    {

        public static CentralDirectory ctx;
        
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
            if(ctx.ListServer.Contains(node)){
                return false;
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
                uint aux = ctx.SHA1Hash(keys[i]);
                Console.WriteLine(aux);
                
                
                for (int j = 0; j < ctx.Location.Count(); j++)
                {
                    if (ctx.Location[i].max > aux && ctx.Location[i].min < aux)
                    {
                        listaux.Add(ctx.getNode(ctx.Location[i].IP[0]));
                        listaux.Add(ctx.getNode(ctx.Location[i].IP[1]));
                        server.Add(aux2, listaux);

                    }
                }
            }
            transactionsContext.NodesLocation = server;
            return transactionsContext;
        }

        public void ServerDown(CommonInterfaces.Node server)
        {
           CommonInterfaces.Node node = ctx.getNode(server.IP);
           ctx.ListServer.Remove(node);
        }

        

    }

}