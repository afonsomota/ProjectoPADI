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



            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientPuppet), "ClientPuppet", WellKnownObjectMode.Singleton);
            IPuppetMaster ligacao = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
               "tcp://localhost:8090/PseudoNodeReg");
            Node node = new Node(host, port, NodeType.Client);
            Client clt = new Client(node,channel);
            ClientPuppet.ctx = clt;
            ClientRemoting.ctx = clt;
            ligacao.RegisterPseudoNode(node);
            System.Console.WriteLine(host + ":" + port.ToString());
            Console.WriteLine("Press Enter to Test...");
           
            //Testes
            System.Console.ReadLine();
            List<string> testList = new List<string>();
            testList.Add("Afonso");
            testList.Add("Rui");
            testList.Add("Chinchila");
            testList.Add("Power Ranger");
            testList.Add("Pokemon");
            testList.Add("Mais uma string qualquer");
            System.Console.ReadLine();            
        }
    }

    class Client {
        public string[] Registers;
        public Node Info;
        public TcpChannel Channel;
        public List<Node> NetworkTopology;

        public Client(Node info,TcpChannel channel){
            Registers = new string[10];
            Info = info;
            Channel = channel;
        }

        public void StoreValue(int reg, string value){
            Registers[reg] = value;
        }


        //Inicia a transacção
        public void BeginTxInternal()
        {
           

        }

        //Adiciona o valor ao registo
        public void StoreInternal(int register, string value)
        {
            Registers[register]=value;
        }

        //Executa um Put com o conteudo do registo "register" na key "key"
        public void PutInternal(int register, string key)
        {
            
        }

        //Executa um get na key "key" e guarda o conteudo no register
        public void GetInternal(int register, string key)
        {

        }

        //mete o valor "value" na key "key"
        public void PutVAlInternal(string key, int value)
        {

        }

        //change the value of the key in the register number to lower case
        public void ToLowerInternal(int register)
        {
            Registers[register].ToLower();

        }

        public void ToUpperInternal(int register)
        {
            Registers[register].ToUpper();
        }

        //concata a string do registo 2 no registo 1
        public void ConcatInternal(int register1, int register2)
        {
            Registers[register1] = Registers[register1] + Registers[register2];
        }

        //faz commit da transacção
        public void CommitTxInternal()
        {

        }

        public void ExeScriptInternal(string[] instructions)
        {

        }

    }

    class ClientRemoting : MarshalByRefObject, IClient
    {

        public static Client ctx;

        public void GetNetworkUpdate(List<Node> network) {
            ctx.NetworkTopology = network;
            Console.WriteLine("\nNetwork Topology Update!");
            foreach (Node n in ctx.NetworkTopology) {
                Console.WriteLine(n);
            }
        }



    }

    class ClientPuppet : MarshalByRefObject, IClientPuppet
    {
        public static Client ctx;

        public void StartClient()
        {
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientRemoting), "Client", WellKnownObjectMode.Singleton);
            ICentralDirectory ligacao = (ICentralDirectory)Activator.GetObject(
               typeof(ICentralDirectory),
               "tcp://localhost:9090/CentralDirectory");
            ligacao.RegisterClient(ctx.Info);
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
            ChannelServices.UnregisterChannel(ctx.Channel);
            ctx.Channel = new TcpChannel(ctx.Info.Port);
            ChannelServices.RegisterChannel(ctx.Channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientPuppet), "ClientPuppet", WellKnownObjectMode.Singleton);
            Console.WriteLine("Client Offline");
        }

        //Inicia a transacção
        public void BeginTx() {
            ctx.BeginTxInternal();
        }

        //Adiciona o valor ao registo
        public void Store(int register, string value) {
            ctx.StoreInternal(register, value);
        }

        //Executa um Put com o conteudo do registo "register" na key "key"
        public void Put(int register, string key) {
            ctx.PutInternal(register, key);
        }

        //Executa um get na key "key" e guarda o conteudo no register
        public void Get(int register, string key) {
            ctx.GetInternal(register, key);
        }

        //mete o valor "value" na key "key"
        public void PutVAl(string key, int value) {
            ctx.PutVAlInternal(key, value);
        }

        //change the value of the key in the register number to lower case
        public void ToLower(int register) {
            ctx.ToLowerInternal(register);
        }

        public void ToUpper(int register) {
            ctx.ToUpperInternal(register);
        }

        //concata a string do registo 2 no registo 1
        public void Concat(int register1, int register2) {
            ctx.ConcatInternal(register1, register2);
        }

        //faz commit da transacção
        public void CommitTx() {
            ctx.CommitTxInternal();
        }

        public string[] Dump() {
            return ctx.Registers;
        }

        public void ExeScript(string[] operations){
            ctx.ExeScriptInternal(operations);
        }

    }

}
