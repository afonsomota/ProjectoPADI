using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonInterfaces;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Collections;

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

            System.Console.WriteLine(port);
            System.Console.ReadLine();

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientPuppet), "ClientPuppet", WellKnownObjectMode.Singleton);
            
        }
    }

    class Client {
        string[] registers;

        public Client(){
            registers = new string[10];
        }

        public void StoreValue(int reg, string value){
            registers[reg] = value;
        }

    }

    class ClientRemoting : IClient {
        public void GetNetworkUpdate(List<Node> network) { 
            //TODO
        }
    }

    class ClientPuppet : IClientPuppet {

        public bool StartClient()
        {
            throw new NotImplementedException();
        }

        public bool KillClient()
        {
            throw new NotImplementedException();
        }
    }

}
