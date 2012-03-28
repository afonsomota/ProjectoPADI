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
using CommonInterfaces;

namespace PuppetMaster
{
    public partial class Form1 : Form
    {
        public delegate void WriteHost(NodeType type,string url);
        public WriteHost WriteHostDelegate;
        public List<Node> Clients;
        public List<Node> Servers;

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
        }

        public void WriteHostMethod(NodeType type,string url){
            if(type==NodeType.Client){
                listBox1.Items.Add(url);
            }else{
            
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
              typeof(IClientPuppet),
              "tcp://" + (string)listBox1.SelectedItem + "/ClientPuppet");
            
            ligacao.StartClient();
            string item = (string)listBox1.SelectedItem;
            listBox1.Items.Remove(item);
            listBox2.Items.Add(item);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IClientPuppet ligacao = (IClientPuppet)Activator.GetObject(
              typeof(IClientPuppet),
              "tcp://" + (string)listBox2.SelectedItem + "/ClientPuppet");
            ligacao.KillClient();
            string item = (string)listBox2.SelectedItem;
            listBox2.Items.Remove(item);
            listBox1.Items.Add(item);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = (string)listBox1.SelectedItem;
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
            }
        }
    }
}
