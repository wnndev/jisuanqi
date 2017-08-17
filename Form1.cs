using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string ip;
        int port;
        bool seropen = false;
        Dictionary<TcpClient, Thread> clientThread = new Dictionary<TcpClient, Thread>();
        Thread ServerThread;
        TcpListener server;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ip = textBoxIP.Text;
            port = int.Parse(textBoxPort.Text);
            if (!seropen)
            {
                button1.Text = "停止";
                seropen = true;
                ServerThread = new Thread(StartServer);
                ServerThread.Start();
            }
            else
            {
                button1.Text = "开启";
                seropen = false;
                ServerThread.Abort();
                server.Stop();
                if (clientThread!=null)
                {
                    ArrayList allKeys = new ArrayList(clientThread.Keys);
                    for(int i=0;i<allKeys.Count;i++)
                    {
                        TcpClient tc = (TcpClient)allKeys[i];
                        clientThread[tc].Abort();
                        clientThread.Remove(tc);
                        textBox1.Text = textBox1.Text + tc.Client.RemoteEndPoint.ToString()+"已停止\r\n";
                        tc.Close();
                    }
                }
                
            }

        }
        void StartServer()
        {
            server = new TcpListener(IPAddress.Parse(ip), port);
            server.Start();
            
                
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Thread thread = new Thread(ClientRev);
                    thread.Start(client);
                    clientThread.Add(client, thread);
                }
           
        }
        public void ClientRev(object c)
        {
            TcpClient client = (TcpClient)c;
            string revdata="" ;
            string value = "";
            int revlength = 0;
            StreamReader rs = new StreamReader(client.GetStream(), Encoding.Default);
            StreamWriter ws = new StreamWriter(client.GetStream(),Encoding.Default);
            NetworkStream ns = client.GetStream();
            DataTable table = new DataTable();
            ShowText(textBox1.Text + client.Client.RemoteEndPoint.ToString() + "连接\r\n");
            while (client.Connected)
            {
                try
                {
                    revdata = rs.ReadLine();
                    try
                    {
                        value = table.Compute(revdata, "").ToString();
                    }
                    catch (Exception ex)
                    {
                        value = ex.Message;
                    }
                    ws.WriteLine(value);
                    
                    ws.Flush();
                    ShowText(textBox1.Text + client.Client.RemoteEndPoint.ToString() + ":" + revdata + "=" + value + "\r\n");
                }
                catch (Exception)
                {

                }
                finally
                {
                    
                }            
            }       
            ShowText(textBox1.Text + client.Client.RemoteEndPoint.ToString()+"断开\r\n");
            clientThread.Remove(client);
            client.Close();
        }
        delegate void SetTextCallback(string text);
        void ShowText(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(ShowText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text = text;
            }
        }

    }
}
