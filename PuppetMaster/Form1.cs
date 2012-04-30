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
using System.Threading;


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
        int numberOfUnamedClients = 0;
        string[] defaultUsernames = new string[] { "caravela", "adamastor", "pedro", "carro", "kika", "antonio", "shelf", "chapeu", "frigorifico", "porta", "papel", "computador", "livro", "caneta", "lapiseira", "rato" };
        public string[] userScriptList;
        public string[] testes;
        public Dictionary<string, Process> runningProcesses = new Dictionary<string, Process>();
        public Dictionary<int, Process> runningServers = new Dictionary<int, Process>();

        public Dictionary<string, List<string>> clientsOperations = new Dictionary<string, List<string>>();
        public Dictionary<string, ListBox> clientRegistersValueListbox = new Dictionary<string, ListBox>();
        public Dictionary<string, Label> clientRegistersValueListboxLabel = new Dictionary<string, Label>();
        public Dictionary<string, Button> clientListBoxDumpButton = new Dictionary<string, Button>();
        public Dictionary<string, Button> clientListBoxToUpperButton = new Dictionary<string, Button>();
        public Dictionary<string, Button> clientListBoxLowerButton = new Dictionary<string, Button>();
        public Dictionary<string, TextBox> clientListBoxLowerTextBox = new Dictionary<string, TextBox>();
        public Dictionary<string, TextBox> clientListBoxUpperTextBox = new Dictionary<string, TextBox>();
        public Dictionary<string, TextBox> clientRegistersValueListboxConcatRegister1 = new Dictionary<string,TextBox>();
        public Dictionary<string, TextBox> clientRegistersValueListboxConcatRegister2 = new Dictionary<string, TextBox>();
        public Dictionary<string, Button> clientListBoxConcatButton = new Dictionary<string, Button>();
        public Dictionary<string,TextBox > clientListBoxPutRegisterTextBox = new Dictionary<string, TextBox>();
        public Dictionary<string, TextBox> clientListBoxPutRegisterTextBox2 = new Dictionary<string, TextBox>();
        public Dictionary<string, Button> clientListBoxPutRegisterButton = new Dictionary<string, Button>();
        public Dictionary<string, TextBox> clientListBoxGetKeyTextBox = new Dictionary<string, TextBox>();
        public Dictionary<string, TextBox> clientListBoxGetKeyTextBox2 = new Dictionary<string, TextBox>();
        public Dictionary<string, Button> clientListBoxGetKeyButton = new Dictionary<string, Button>();
        private System.Object locker = new System.Object(); 

        int numberOfClients = 0;
        int numberOfServers = 1;

        public Process CentralDirectory = new Process();

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
              //  listCliOffline.Items.Add(item);

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
                //listCliOffline.Items.Add(item);

                IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
               typeof(IClientPuppet),
                "tcp://" + item + "/ClientPuppet");
                ligacao.StartClient();
                clientsOperations.Add(item, new List<string>());
            }
        }

        private string SearchClientAdressByName(string name) 
        {
            lock (locker)
            {
                string address = null;
                foreach (Node p in Clients)
                {
                    if (name == p.Name)
                        address = p.IP + ":" + p.Port;
                }
                return address;
            }   
        }
        
        private string SearchServerAdressByName(string name)
        {
            lock (locker)
            {
                string address = null;
                foreach (Node p in Servers)
                {
                    if (name == p.Name)
                        address = p.IP + ":" + p.Port;
                }
                return address;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string clientName = (string)listCliOnline.SelectedItem;
            stopClient(clientName);
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
                //listServOffline.Items.Add(item);
            }
            else  
            {
                string item = (string)listServOnline.Items[0];
                listServOnline.Items.RemoveAt(0);
             //   listServOffline.Items.Add(item);

                IServerPuppet ligacao = (IServerPuppet)Activator.GetObject(
                  typeof(IServerPuppet),
                  "tcp://" + item + "/ServerPuppet");

                ligacao.StartServer();    
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string serter = (string)listServOnline.SelectedItem;
            char[] delim = { '-'};
            string[] arg = serter.Split(delim);

            stopServer(Int32.Parse(arg[1]));
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
          //  textBox1.Text += (string)listCliOffline.SelectedItem;
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
            RunInstruction(operation);
        }

        private void RunInstruction(string operation) 
        {
            char[] delim = { ' ', '\t' };
            string[] arg = operation.Split(delim);

            if (operation.StartsWith("BEGINTX"))
            {
                while (SearchClientAdressByName(arg[1])==null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet),
                "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.BeginTx();
            }
            else if (operation.StartsWith("COMMITTX"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet), "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.CommitTx();
            }
            else if (operation.StartsWith("PUT"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet), "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.Put(Int32.Parse(arg[2]), arg[3]);
            }
            else if (operation.StartsWith("GET"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet), "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.Get(Int32.Parse(arg[2]), arg[3]);
            }
            else if (operation.StartsWith("WAIT"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet), "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.Sleep(Int32.Parse(arg[1]));
            }
            else if (operation.StartsWith("STORE"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet), "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.Store(Int32.Parse(arg[2]), arg[3]);

            }
            else if (operation.StartsWith("PUTVAL"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                  typeof(IClientPuppet), "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.PutVAl(arg[2], arg[3]);
            }
            else if (operation.StartsWith("CONCAT"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet), "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.Concat(Int32.Parse(arg[2]), Int32.Parse(arg[3]));
            }
            else if (operation.StartsWith("TOLOWER"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet), "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.ToLower(Int32.Parse(arg[2]));
            }
            else if (operation.StartsWith("TOUPPER"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
                    typeof(IClientPuppet), "tcp://" + SearchClientAdressByName(arg[1]) + "/ClientPuppet");
                cliente.ToUpper(Int32.Parse(arg[2]));
            }
            else if (operation.StartsWith("DUMP"))
            {
                while (SearchClientAdressByName(arg[1]) == null) { }
                DumpClient(arg[1]);
            }
            else if (operation.StartsWith("CONNECT"))
            {
                if (arg[1].StartsWith("server-") || arg[1].StartsWith("SERVER-"))
                {
                    char[] lol = { '-' };
                    string[] argt = arg[1].Split(lol);
                    startServer(Int32.Parse(argt[1]));
                    //while (SearchServerAdressByName(arg[1]) == null) { }
                }
                else if (arg[1] == "central" || arg[1]=="CENTRAL")
                {
                    startCentral();
                }
                else startClient(arg[1]);
            }
            else if (operation.StartsWith("DISCONNECT"))
            {
                if (arg[1].StartsWith("server-") || arg[1].StartsWith("SERVER-"))
                {
                    char[] lol = { '-' };
                    string[] argt = arg[1].Split(lol);
                    while (SearchServerAdressByName(arg[1]) == null) { }
                    stopServer(Int32.Parse(argt[1]));
                }
                else if (arg[1] == "central" || arg[1] == "CENTRAL")
                {
                    stopCentral();
                }
                else
                {
                    while (SearchClientAdressByName(arg[1]) == null) { }
                    stopClient(arg[1]);
                }
            }
        
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (listBox3.SelectedItem != null)
            {
                foreach (string operation in listBox3.Items)
                {
                    RunInstruction(operation);
                }
            }
        }
            
        private void stopCentral()
        {
            CentralDirectory.Kill();
        }
        
        private void startCentral()
        {
            string currentDirectory = Environment.CurrentDirectory;
            string path = currentDirectory.Replace("PuppetMaster", "CentralDirectory");
            path += "/CentralDirectory.exe";
            CentralDirectory.StartInfo.FileName = path;
            CentralDirectory.Start();
        }

        private void startServer(int n)
        {
            //TODO Interface
            runningServers.Add(n,new Process());
            string currentDirectory = Environment.CurrentDirectory;
            string path = currentDirectory.Replace("PuppetMaster", "Server");
            path += "/Server.exe";
            runningServers[n].StartInfo.FileName = path;
            runningServers[n].Start();
            numberOfServers++;
        }

        private void stopServer(int n)
        {
            
            IServerPuppet server = (IServerPuppet)Activator.GetObject(
                typeof(IServerPuppet),
                "tcp://" + SearchServerAdressByName("server-" + n.ToString()) + "/ServerPuppet");
            server.KillServer();

            string item = "server-" + n.ToString();
            listServOnline.Items.Remove(item);

            //runningServers[n].Kill();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            
            string operation = (string)listBox3.SelectedItem;
            RunInstruction(operation);
            if (listBox3.SelectedIndex!= listBox3.Items.Count)listBox3.SelectedIndex++;
        }

        private void startClient(string clientName) 
        {   
            string currentDirectory = Environment.CurrentDirectory;
            string path = currentDirectory.Replace("PuppetMaster", "Client");
            path += "/Client.exe";
            runningProcesses.Add(clientName,new Process());

            clientsOperations.Add(clientName, new List<string>());
            clientRegistersValueListbox.Add(clientName, new ListBox());
            clientRegistersValueListboxLabel.Add(clientName, new Label());
            clientListBoxDumpButton.Add(clientName, new Button());
            clientListBoxToUpperButton.Add(clientName, new Button());
            clientListBoxLowerButton.Add(clientName, new Button());
            clientListBoxLowerTextBox.Add(clientName,new TextBox());
            clientListBoxUpperTextBox.Add(clientName, new TextBox());
            clientRegistersValueListboxConcatRegister2.Add(clientName, new TextBox());
            clientRegistersValueListboxConcatRegister1.Add(clientName, new TextBox());
            clientListBoxConcatButton.Add(clientName, new Button());
            clientListBoxPutRegisterTextBox.Add(clientName,new TextBox());
            clientListBoxPutRegisterTextBox2.Add(clientName, new TextBox());
            clientListBoxPutRegisterButton.Add(clientName,new Button());
            clientListBoxGetKeyTextBox.Add(clientName, new TextBox());
            clientListBoxGetKeyTextBox2.Add(clientName, new TextBox());
            clientListBoxGetKeyButton.Add(clientName, new Button());

            GenerateClientDumpListBox(clientName);
            numberOfClients++;

            runningProcesses[clientName].StartInfo.Arguments = clientName;
            runningProcesses[clientName].StartInfo.FileName = path;
            runningProcesses[clientName].Start(); 
        }

        private void stopClient(string clientName) 
        {

            clientRegistersValueListbox[clientName].Dispose();
            clientRegistersValueListboxLabel[clientName].Dispose();
            clientListBoxDumpButton[clientName].Dispose();
            clientListBoxToUpperButton[clientName].Dispose();
            clientListBoxLowerButton[clientName].Dispose();
            clientListBoxLowerTextBox[clientName].Dispose();
            clientListBoxUpperTextBox[clientName].Dispose();
            clientRegistersValueListboxConcatRegister2[clientName].Dispose(); 
            clientRegistersValueListboxConcatRegister1[clientName].Dispose();
            clientListBoxConcatButton[clientName].Dispose();
            clientListBoxPutRegisterTextBox[clientName].Dispose();
            clientListBoxPutRegisterTextBox2[clientName].Dispose();
            clientListBoxPutRegisterButton[clientName].Dispose();
            clientListBoxGetKeyTextBox[clientName].Dispose();
            clientListBoxGetKeyTextBox2[clientName].Dispose();
            clientListBoxGetKeyButton[clientName].Dispose();

            runningProcesses[clientName].Kill();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string clientName = null;

            if (textBox2.Text.Length==0 )
            {
                clientName = defaultUsernames[numberOfUnamedClients];
                numberOfUnamedClients++;
            }
            else
            {
                clientName = textBox2.Text;
            }
            startClient(clientName);
        }

        private void GenerateClientDumpListBox(string clientName) 
        {
            int tableWidth = 170, tableHeight=150;
            int startingPointX = 40 + numberOfClients * tableWidth;

            clientListBoxDumpButton[clientName].Text = "Show " + clientName + " Registers (DUMP)";
            clientListBoxDumpButton[clientName].Width = tableWidth;
            clientListBoxDumpButton[clientName].Height = 40;
            clientListBoxDumpButton[clientName].Click += (sender, e) => { MyHandler(sender, e,clientName); };
            clientListBoxDumpButton[clientName].Location = new System.Drawing.Point(startingPointX, 40 + tableHeight);

            clientListBoxLowerTextBox[clientName].Location = new System.Drawing.Point(startingPointX + 95, 80 + tableHeight);
            clientListBoxLowerTextBox[clientName].Width = tableWidth / 2 - 10;
            clientListBoxLowerTextBox[clientName].Height = 40;

            clientListBoxLowerButton[clientName].Text = "Lower Case";
            clientListBoxLowerButton[clientName].Width = tableWidth/2;
            clientListBoxLowerButton[clientName].Height = 20;
            clientListBoxLowerButton[clientName].Click += (sender, e) => { ToLowerHandler(sender, e, clientName, Int32.Parse(clientListBoxLowerTextBox[clientName].Text)); };
            clientListBoxLowerButton[clientName].Location = new System.Drawing.Point(startingPointX , 80 + tableHeight);

            clientListBoxUpperTextBox[clientName].Location = new System.Drawing.Point(startingPointX + 95, 100 + tableHeight);
            clientListBoxUpperTextBox[clientName].Width = tableWidth / 2 - 10;
            clientListBoxUpperTextBox[clientName].Height = 40;

            clientListBoxToUpperButton[clientName].Text = "Up Case";
            clientListBoxToUpperButton[clientName].Width = tableWidth/2;
            clientListBoxToUpperButton[clientName].Height = 20;
            clientListBoxToUpperButton[clientName].Click += (sender, e) => { ToUpperHandler(sender, e, clientName, Int32.Parse(clientListBoxUpperTextBox[clientName].Text)); };
            clientListBoxToUpperButton[clientName].Location = new System.Drawing.Point(startingPointX , 100 + tableHeight);

            clientRegistersValueListboxLabel[clientName].Text = clientName;
            clientRegistersValueListboxLabel[clientName].Location = new System.Drawing.Point(startingPointX, 20);

            clientRegistersValueListboxConcatRegister1[clientName].Location = new System.Drawing.Point(startingPointX + 95, 120 + tableHeight);
            clientRegistersValueListboxConcatRegister1[clientName].Width = 15;
            clientRegistersValueListboxConcatRegister1[clientName].Height = 15;

            clientRegistersValueListboxConcatRegister2[clientName].Location = new System.Drawing.Point(startingPointX + 120, 120 + tableHeight);
            clientRegistersValueListboxConcatRegister2[clientName].Width = 15;
            clientRegistersValueListboxConcatRegister2[clientName].Height = 15;

            clientListBoxConcatButton[clientName].Text = "Concat:";
            clientListBoxConcatButton[clientName].Width = tableWidth / 2;
            clientListBoxConcatButton[clientName].Height = 20;
            clientListBoxConcatButton[clientName].Click += (sender, e) => { ConcatHandler(sender, e, clientName, Int32.Parse(clientRegistersValueListboxConcatRegister1[clientName].Text), Int32.Parse(clientRegistersValueListboxConcatRegister2[clientName].Text)); };
            clientListBoxConcatButton[clientName].Location = new System.Drawing.Point(startingPointX, 120 + tableHeight);

            //Put no Register
            clientListBoxPutRegisterTextBox[clientName].Location = new System.Drawing.Point(startingPointX + 95, 140 + tableHeight);
            clientListBoxPutRegisterTextBox[clientName].Width = 15;
            clientListBoxPutRegisterTextBox[clientName].Height = 40;

            clientListBoxPutRegisterTextBox2[clientName].Location = new System.Drawing.Point(startingPointX + 110, 140 + tableHeight);
            clientListBoxPutRegisterTextBox2[clientName].Width = tableWidth / 2 - 20;
            clientListBoxPutRegisterTextBox2[clientName].Height = 40;

            clientListBoxPutRegisterButton[clientName].Text = "Store";
            clientListBoxPutRegisterButton[clientName].Width = tableWidth / 2;
            clientListBoxPutRegisterButton[clientName].Height = 20;
            clientListBoxPutRegisterButton[clientName].Click += (sender, e) => { PutRegisterHandler(sender, e, clientName, Int32.Parse(clientListBoxPutRegisterTextBox[clientName].Text), clientListBoxPutRegisterTextBox2[clientName].Text); };
            clientListBoxPutRegisterButton[clientName].Location = new System.Drawing.Point(startingPointX, 140 + tableHeight);

            //Get no Servidor
            clientListBoxGetKeyTextBox[clientName].Location = new System.Drawing.Point(startingPointX + 95, 160 + tableHeight);
            clientListBoxGetKeyTextBox[clientName].Width = 15;
            clientListBoxGetKeyTextBox[clientName].Height = 40;

            clientListBoxGetKeyTextBox2[clientName].Location = new System.Drawing.Point(startingPointX + 110, 160 + tableHeight);
            clientListBoxGetKeyTextBox2[clientName].Width = tableWidth / 2 - 20;
            clientListBoxGetKeyTextBox2[clientName].Height = 40;

            clientListBoxGetKeyButton[clientName].Text = "Get Key Value";
            clientListBoxGetKeyButton[clientName].Width = tableWidth / 2;
            clientListBoxGetKeyButton[clientName].Height = 20;
            clientListBoxGetKeyButton[clientName].Click += (sender, e) => { GetKeyHandler(sender, e, clientName, Int32.Parse(clientListBoxGetKeyTextBox[clientName].Text), clientListBoxGetKeyTextBox2[clientName].Text); };
            clientListBoxGetKeyButton[clientName].Location = new System.Drawing.Point(startingPointX, 160 + tableHeight);

            clientRegistersValueListbox[clientName].Location = new System.Drawing.Point(startingPointX, 40);
            clientRegistersValueListbox[clientName].Name = "ListBox"+"clientName";
            clientRegistersValueListbox[clientName].Size = new System.Drawing.Size(tableWidth, tableHeight);

            this.tabPage2.Controls.Add(clientListBoxDumpButton[clientName]);
            this.tabPage2.Controls.Add(clientListBoxUpperTextBox[clientName]);
            this.tabPage2.Controls.Add(clientListBoxToUpperButton[clientName]);
            this.tabPage2.Controls.Add(clientListBoxLowerButton[clientName]);
            this.tabPage2.Controls.Add(clientListBoxLowerTextBox[clientName]);
            this.tabPage2.Controls.Add(clientRegistersValueListbox[clientName]);
            this.tabPage2.Controls.Add(clientRegistersValueListboxLabel[clientName]);
            this.tabPage2.Controls.Add(clientRegistersValueListboxConcatRegister2[clientName]);
            this.tabPage2.Controls.Add(clientRegistersValueListboxConcatRegister1[clientName]);
            this.tabPage2.Controls.Add(clientListBoxConcatButton[clientName]);
            this.tabPage2.Controls.Add(clientListBoxPutRegisterTextBox[clientName]);
            this.tabPage2.Controls.Add(clientListBoxPutRegisterButton[clientName]);
            this.tabPage2.Controls.Add(clientListBoxPutRegisterTextBox2[clientName]);
            this.tabPage2.Controls.Add(clientListBoxGetKeyTextBox2[clientName]);
            this.tabPage2.Controls.Add(clientListBoxGetKeyTextBox[clientName]);
            this.tabPage2.Controls.Add(clientListBoxGetKeyButton[clientName]);
        
        }

        void GetKeyHandler(object sender, EventArgs e, string clientName, int registerNumber, string key)
        {

            IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
            typeof(IClientPuppet),
            "tcp://" + SearchClientAdressByName(clientName) + "/ClientPuppet");

            cliente.Get(registerNumber, key);
            DumpClient(clientName);
        }

        void PutRegisterHandler(object sender, EventArgs e, string clientName, int registerNumber, string newValue)
        {

            IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
            typeof(IClientPuppet),
            "tcp://" + SearchClientAdressByName(clientName) + "/ClientPuppet");

            cliente.Store(registerNumber,newValue);
            DumpClient(clientName);
        }

        void ConcatHandler(object sender, EventArgs e, string clientName, int registerNumber, int registerNumber2)
        {

            IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
            typeof(IClientPuppet),
            "tcp://" + SearchClientAdressByName(clientName) + "/ClientPuppet");

            cliente.Concat(registerNumber,registerNumber2);
            DumpClient(clientName);
        }

        void ToLowerHandler(object sender, EventArgs e, string clientName,int registerNumber) 
        {
            Button toLowerButton = (Button)sender;

            IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
            typeof(IClientPuppet),
            "tcp://" + SearchClientAdressByName(clientName) + "/ClientPuppet");

            cliente.ToLower(registerNumber);
            DumpClient(clientName);
        }

        void ToUpperHandler(object sender, EventArgs e, string clientName, int registerNumber)
        {
            Button toLowerButton = (Button)sender;

            IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
            typeof(IClientPuppet),
            "tcp://" + SearchClientAdressByName(clientName) + "/ClientPuppet");

            cliente.ToUpper(registerNumber);
            DumpClient(clientName);
        }

        void DumpClient(string clientName)
        {
            int aux = 1;
            //dumpButton.Text = clientName;
            IClientPuppet cliente = (IClientPuppet)Activator.GetObject(
             typeof(IClientPuppet),
             "tcp://" + SearchClientAdressByName(clientName) + "/ClientPuppet");
            string[] clientRegisters = new string[] { "", "", "", "", "", "", "", "", "", "", "" };
            clientRegisters = cliente.Dump();

            ListBox currentListBox = clientRegistersValueListbox[clientName];
            currentListBox.Items.Clear();


            foreach (string s in clientRegisters)
            {
                currentListBox.Items.Add("Resgisto " + aux.ToString() + "  Valor: " + s);
                aux++;
            }
        }

        void MyHandler(object sender, EventArgs e, string clientName) {
            DumpClient(clientName);
        }


        private void button8_Click(object sender, EventArgs e)
        {
            startServer(numberOfServers);

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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void listServOnline_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            startCentral();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            stopCentral();
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
