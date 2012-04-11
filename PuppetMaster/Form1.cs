using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public void WriteHostMethod(NodeType type,string url){
            if(type==NodeType.Client){
                listCliOnline.Items.Add(url);
            }else{
                listServOnline.Items.Add(url);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
              typeof(IClientPuppet),
              "tcp://" + (string)listCliOnline.SelectedItem + "/ClientPuppet");
            
            ligacao.StartClient();
            string item = (string)listCliOnline.SelectedItem;
            listCliOnline.Items.Remove(item);
            listCliOffline.Items.Add(item);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
              typeof(IClientPuppet),
              "tcp://" + (string)listCliOffline.SelectedItem + "/ClientPuppet");
            ligacao.KillClient();
            string item = (string)listCliOffline.SelectedItem;
            listCliOffline.Items.Remove(item);
            listCliOnline.Items.Add(item);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = (string)listCliOnline.SelectedItem;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            IServerPuppet ligacao = (IServerPuppet)Activator.GetObject(
              typeof(IServerPuppet),
              "tcp://" + (string)listServOnline.SelectedItem + "/ServerPuppet");

            ligacao.StartServer();
            string item = (string)listServOnline.SelectedItem;
            listServOnline.Items.Remove(item);
            listServOffline.Items.Add(item);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IServerPuppet ligacao = (IServerPuppet)Activator.GetObject(
              typeof(IServerPuppet),
              "tcp://" + (string)listServOffline.SelectedItem + "/ServerPuppet");
            ligacao.KillServer();
            string item = (string)listServOffline.SelectedItem;
            listServOffline.Items.Remove(item);
            listServOnline.Items.Add(item);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "PADI Script|*.txt";
            openFileDialog1.Title = "Select a PADI User Script";
            string line;
            
            

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (userScriptList!=null && !userScriptList.Contains(openFileDialog1.FileName))
                {
                    userScriptList[userScriptList.Length] = openFileDialog1.FileName;
                }
                StreamReader userscript = new StreamReader(openFileDialog1.OpenFile());
             
                while ((line = userscript.ReadLine()) != null)
               {
                   Console.WriteLine(line);
                   
                    string [] operation = line.Split(new Char[] {' ','\t'});

                   switch (operation[0])
                   { 

                    //Client 
                       case "BEGINTX":
                            // beginTx
                       case "STORE":
                           // store
                       case "PUT":
                            //put
                       case "GET":
                           //get
                       case "PUTVAL":

                       case "TOLOWER":

                       case "TOUPPER":

                       case "CONCAT":

                       case "COMMITTX":        

                       case "DUMP":

                       case "WAIT":

                       default:
                           //invalid argument
                           break;


                   }
               }
            }
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void listCliOffline_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

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
                ctx.Invoke(ctx.WriteHostDelegate, new Object[] { node.Type, node.IP +":"+ node.Port.ToString() });
            }
            else {
                ctx.Servers.Add(node);
                ctx.Invoke(ctx.WriteHostDelegate, new Object[] { node.Type, node.IP + ":" + node.Port.ToString() });
            }
        }
    }
}
