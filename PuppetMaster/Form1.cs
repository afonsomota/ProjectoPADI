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
            label1.Text="Debug";
            if(type==NodeType.Client){
                listBox1.Items.Add(url);
            }else{
            
            }
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
                ctx.Invoke(ctx.WriteHostDelegate, new Object[] { node.Type, node.IP + node.Port.ToString() });
            }
            else {
                ctx.Servers.Add(node);
            }
        }
    }
}
