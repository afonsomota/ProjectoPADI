using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.IO;
using CommonInterfaces;

namespace PuppetMaster
{
    public delegate void WriteHost(NodeType type, string url);
    public delegate void WriteUserScripts(string[] userScriptList);


    public partial class Form1 : Form
    {
        public WriteHost WriteHostDelegate;
        public WriteUserScripts WriteUserScripts;
        public List<Node> Clients;
        public List<Node> Servers;
        public string[] userScriptList;
        public string[] testes;
        public Dictionary<string, List<string>> clientsOperations = new Dictionary<string, List<string>>();
        
        public string ip;
        

        public Form1()
        {
            InitializeComponent();
            PuppetMaster.ctx = this;
            TcpChannel channel = new TcpChannel(8090);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(PuppetMaster), "PseudoNodeReg", WellKnownObjectMode.Singleton);
            Clients = new List<Node>();
            Servers = new List<Node>();
            WriteHostDelegate = new WriteHost(WriteHostMethod);
           // WriteUserScriptsDelegate = new WriteUserScripts(WriteUserScriptsMethod);
            
        }

        public void WriteUserScriptsMethod(string[] userScripts) { 
        
        
        }

        public void WriteHostMethod(NodeType type,string name){
            if(type==NodeType.Client){
                listCliOnline.Items.Add(name);
            }else{
                listServOnline.Items.Add(name);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            

            if (listCliOnline.SelectedItem!= null)
            {
                string item = (string)listCliOnline.SelectedItem;
                listCliOnline.Items.Remove(item);
                listCliOffline.Items.Add(item);

                IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
                typeof(IClientPuppet),
                 "tcp://" + (string)listCliOnline.SelectedItem + "/ClientPuppet");
                ligacao.StartClient();



                clientsOperations.Add((string)listCliOnline.SelectedItem, new List<string>());
            }
            else
            {
                string item = (string)listCliOnline.Items[0];
                listCliOnline.Items.RemoveAt(0);
                listCliOffline.Items.Add(item);

                IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
               typeof(IClientPuppet),
                "tcp://" + item + "/ClientPuppet");
                ligacao.StartClient();



                clientsOperations.Add(item, new List<string>());
            }
        }

        private string SearchClientAdressByName(string name) 
        {
            string address=null;
            foreach (Node p in Clients)
            {
                if (name==p.Name)
                    address = p.IP +":"+ p.Port;
            }
            return address;
        }
        
        private string SearchServerAdressByName(string name)
        {
            string address = null;
            foreach (Node p in Servers)
            {
                if (name == p.Name)
                    address = p.IP + ":" + p.Port;
            }
            return address;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
              typeof(IClientPuppet),
              "tcp://" + SearchClientAdressByName((string)listCliOnline.SelectedItem) + "/ClientPuppet");
           // try
            //{
                cliente.KillClient();
            //}
            //catch (IOException p) 
            //{
              //  clientsOperations.Remove((string)listCliOnline.SelectedItem);
            //}

            clientsOperations.Remove((string)listCliOnline.SelectedItem);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = (string)listCliOnline.SelectedItem;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listServOnline.SelectedItem != null)
            {
                IServerPuppet ligacao = (IServerPuppet)Activator.GetObject(
                  typeof(IServerPuppet),
                  "tcp://" + (string)listServOnline.SelectedItem + "/ServerPuppet");

                ligacao.StartServer();
                string item = (string)listServOnline.SelectedItem;
                listServOnline.Items.Remove(item);
                listServOffline.Items.Add(item);
            }
            else  
            {
                string item = (string)listServOnline.Items[0];
                listServOnline.Items.RemoveAt(0);
                listServOffline.Items.Add(item);

                IServerPuppet ligacao = (IServerPuppet)Activator.GetObject(
                  typeof(IServerPuppet),
                  "tcp://" + item + "/ServerPuppet");

                ligacao.StartServer();
               
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IServerPuppet server = (IServerPuppet)Activator.GetObject(
              typeof(IServerPuppet),
              "tcp://" + SearchServerAdressByName((string)listServOnline.SelectedItem) + "/ServerPuppet");
            server.KillServer();

            string item = (string)listServOnline.SelectedItem;
            listServOnline.Items.Remove(item);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string scriptLine;

            if (listBox1.SelectedItem != null)
            {
                string scriptPath = (string)listBox1.SelectedItem;
                StreamReader userscript = new StreamReader(scriptPath);

                listBox3.Items.Clear();
                while ((scriptLine = userscript.ReadLine()) != null)
                {
                    listBox3.Items.Add(scriptLine);
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "PADI Script|*.txt";
            openFileDialog1.Title = "Select a PADI User Script";

        
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (listBox1.FindString(openFileDialog1.FileName,0)!=0 )
                {
                    listBox1.Items.Add(openFileDialog1.FileName);
                }
                
            }
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void listCliOffline_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text += (string)listCliOffline.SelectedItem;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {   
            string operation = textBox1.Text;
            char[] delim = { ' ', '\t' };
                    string[] arg = operation.Split(delim);
            

            
            if (operation.StartsWith("BEGINTX"))
            {
                clientsOperations[arg[1]].Add("BEGINTX");
            }
            // Esta a meio de uma transacção JA EXISTE UM "BEGINTX" -- Adiciona a pilha de transaccoes
            else if (clientsOperations[arg[1]].Contains("BEGINTX"))
            {
                if (operation.StartsWith("STORE") || operation.StartsWith("PUTVAL") || operation.StartsWith("CONCAT") || operation.StartsWith("PUT") || operation.StartsWith("GET"))
                {
                    clientsOperations[arg[1]].Add(arg[0] + " " + arg[2] + " " + arg[3]);
                }
                else if (operation.StartsWith("TOLOWER") || operation.StartsWith("TOUPPER"))
                {
                    clientsOperations[arg[1]].Add(arg[0] + " " + arg[2]);
                }
                else if (operation.StartsWith("DUMP"))
                {
                    clientsOperations[arg[1]].Add(arg[0]);
                }
                else if (operation.StartsWith("COMMITTX"))
                {
                    clientsOperations[arg[1]].Add("COMMITTX");
                    IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet),
                    "tcp://" + arg[1] + "/ClientPuppet");
                    ligacao.ExeScript(clientsOperations[arg[1]]);
                    clientsOperations[arg[1]]=new List<string>();
                }
            }

            //Nao tem nenhum begin, é para executar logo 
            else {
                if (operation.StartsWith("STORE") || operation.StartsWith("PUTVAL") || operation.StartsWith("CONCAT"))
                {
                    IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet),
                    "tcp://" + arg[1] + "/ClientPuppet");
                    List<string> umaInstrucao = new List<string>();
                    umaInstrucao.Add(arg[0] + " " + arg[2] + " " + arg[3]);
                    ligacao.ExeScript(umaInstrucao);
                }
                else if (operation.StartsWith("TOLOWER") || operation.StartsWith("TOUPPER"))
                {
                    IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet),
                    "tcp://" + arg[1] + "/ClientPuppet");
                    List<string> umaInstrucao = new List<string>();
                    umaInstrucao.Add(arg[0] + " " + arg[2]);
                    ligacao.ExeScript(umaInstrucao);
                    
                }
                else if (operation.StartsWith("DUMP"))
                {
                    IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
                     typeof(IClientPuppet),
                     "tcp://" + arg[1] + "/ClientPuppet");
                    List<string> umaInstrucao = new List<string>();
                    umaInstrucao.Add(arg[0]);
                    ligacao.ExeScript(umaInstrucao);
                }
            
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (listBox3.SelectedItem != null)
            {
                foreach (string operation in listBox3.Items)
                {
                    char[] delim = { ' ', '\t' };
                    string[] arg = operation.Split(delim);

                    if (operation.StartsWith("BEGINTX"))
                    {
                        clientsOperations[arg[1]].Add("BEGINTX");
                    }
                    // Esta a meio de uma transacção JA EXISTE UM "BEGINTX" -- Adiciona a pilha de transaccoes
                    else if (clientsOperations[arg[1]].Contains("BEGINTX"))
                    {
                        if (operation.StartsWith("STORE") || operation.StartsWith("PUTVAL") || operation.StartsWith("CONCAT") || operation.StartsWith("PUT") || operation.StartsWith("GET"))
                        {
                            clientsOperations[arg[1]].Add(arg[0] + " " + arg[2] + " " + arg[3]);
                        }
                        else if (operation.StartsWith("TOLOWER") || operation.StartsWith("TOUPPER"))
                        {
                            clientsOperations[arg[1]].Add(arg[0] + " " + arg[2]);
                        }
                        else if (operation.StartsWith("DUMP"))
                        {
                            clientsOperations[arg[1]].Add(arg[0]);
                        }
                        else if (operation.StartsWith("COMMITTX"))
                        {
                            clientsOperations[arg[1]].Add("COMMITTX");
                            IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
                            typeof(IClientPuppet),
                            "tcp://" + arg[1] + "/ClientPuppet");
                            ligacao.ExeScript(clientsOperations[arg[1]]);
                            clientsOperations[arg[1]] = new List<string>();
                        }
                    }

                    //Nao tem nenhum begin, é para executar logo 
                    else
                    {
                        if (operation.StartsWith("STORE") || operation.StartsWith("PUTVAL") || operation.StartsWith("CONCAT"))
                        {
                            IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
                            typeof(IClientPuppet),
                            "tcp://" + arg[1] + "/ClientPuppet");
                            List<string> umaInstrucao = new List<string>();
                            umaInstrucao.Add(arg[0] + " " + arg[2] + " " + arg[3]);
                            ligacao.ExeScript(umaInstrucao);
                        }
                        else if (operation.StartsWith("TOLOWER") || operation.StartsWith("TOUPPER"))
                        {
                            IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
                            typeof(IClientPuppet),
                            "tcp://" + arg[1] + "/ClientPuppet");
                            List<string> umaInstrucao = new List<string>();
                            umaInstrucao.Add(arg[0] + " " + arg[2]);
                            ligacao.ExeScript(umaInstrucao);
                        }
                        else if (operation.StartsWith("DUMP"))
                        {
                            IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
                            typeof(IClientPuppet),
                            "tcp://" + arg[1] + "/ClientPuppet");
                            List<string> umaInstrucao = new List<string>();
                            umaInstrucao.Add(arg[0]);
                            ligacao.ExeScript(umaInstrucao);
                        }
                    }
                }
            }
            
        }

        private void listServOnline_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listServOffline_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string selectedOperation = (string)listBox3.SelectedItem;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            string arguments = textBox2.Text;
            string currentDirectory = Environment.CurrentDirectory;
            string path = currentDirectory.Replace("PuppetMaster", "Client");
            path += "/Client.exe";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.FileName = path;
            process.Start();
            //centralDirectory = new CentralDirectoryInfo(ip, Convert.ToInt16(port));
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            string currentDirectory = Environment.CurrentDirectory;
            string path = currentDirectory.Replace("PuppetMaster", "Server");
            path += "/Server.exe";
            process.StartInfo.FileName = path;
            process.Start();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Process[] procs = Process.GetProcessesByName("Client");
            foreach (Process p in procs) { p.Kill(); }
            listCliOnline.Items.Clear();

            procs = Process.GetProcessesByName("Server");
            foreach (Process p in procs) { p.Kill(); }
            listServOnline.Items.Clear();
        }
    }

    public class PuppetMaster : MarshalByRefObject, IPuppetMaster
    {
        public static Form1 ctx;

        public void RegisterPseudoNode(Node node)
        {
            if (node.Type == NodeType.Client)
            {
                ctx.Clients.Add(node);
                ctx.Invoke(ctx.WriteHostDelegate, new Object[] { node.Type, node.Name });
            }
            else {
                node.Name = "server-" + (ctx.Servers.Count + 1).ToString();
                ctx.Servers.Add(node);
                ctx.Invoke(ctx.WriteHostDelegate, new Object[] { node.Type, node.Name });
            }
        }
    }
}
