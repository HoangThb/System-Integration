using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.IO;

namespace SocketHW1
{
    public partial class Form1 : Form
    {
        System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        NetworkStream serverStream;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            msg("Client Started");
            button2.Visible = false;
            LoadServer();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                NetworkStream serverStream = clientSocket.GetStream();
                byte[] outStream = Encoding.UTF8.GetBytes(textBox2.Text + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                string returndata = "";
                byte[] inStream = new byte[10025];

                // Read all batch data 
                // Đoạn code ngu học sửa cho đỡ dính lặp vô hạn, 
                // sửa xong chẳng hiểu mình viết gì
                int breakCount = 0;
                int batchSize = 15;
                MemoryStream memoryStream = new MemoryStream();
                do
                {
                    Int32 bytes = clientSocket.Available;
                    do
                    {
                        bytes = serverStream.Read(inStream, 0, inStream.Length);
                        memoryStream.Write(inStream, 0, bytes);
                    }
                    while (serverStream.DataAvailable);

                    
                    serverStream.Flush();
                    serverStream = clientSocket.GetStream();
                    returndata = Encoding.UTF8.GetString(memoryStream.ToArray());
                    if (returndata.Contains("404- Not Found")) break;
                    breakCount++;
                } while (!returndata.Contains("</html>") && breakCount < batchSize);
                msg("Data from Server : " + returndata);
                // Show content
                webBrowser1.DocumentText = returndata;

            }
            catch (Exception ex)
            {
                msg("Exception: " + ex.Message);
            }
            msg("Ready for next Request...");
        }

        public void msg(string mesg)
        {
            textBox1.Text = textBox1.Text + Environment.NewLine + DateTime.Now.ToString() + " >> " + mesg;
        }

        private void LoadServer()
        {
            try
            {
                msg("Connect to Server...");
                clientSocket.Connect(new IPAddress(new byte[] { 127, 0, 0, 1 }), 8888);
                label1.Text = "Client Socket Program - Server Connected ...";
            }
            catch (Exception)
            {
                label1.Text = "Client Socket Program - Server not Connected ...";
                button1.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadServer();
        }

    }
}
