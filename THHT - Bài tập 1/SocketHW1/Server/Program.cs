using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), 8888);
            //TcpListener serverSocket = new TcpListener(8888);
            TcpClient clientSocket = default(TcpClient);
            try
            {
                int counter = 0;
                serverSocket.Start();
                Console.WriteLine(" >> " + "Server Started");
                counter = 0;
                while (true)
                {
                    counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();
                    Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
                    handleClinet client = new handleClinet();
                    
                    client.startClient(clientSocket, Convert.ToString(counter));
                }
                clientSocket.Close();
                serverSocket.Stop();
                Console.WriteLine(" >> " + "exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                clientSocket.Close();
                serverSocket.Stop();
                Console.WriteLine(" >> " + "exit");
                Console.ReadLine();
            }
        }
    }
    //Class to handle each client request separatly
    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;
            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();

                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    dataFromClient = System.Text.Encoding.UTF8.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine(" >> " + "From client-" + clNo + dataFromClient);

                    rCount = Convert.ToString(requestCount);
                    //serverResponse = "Server to clinet(" + clNo + ") " + rCount + " >> " + dataFromClient;
                    serverResponse = GetHtmlContent(dataFromClient);
                    sendBytes = Encoding.UTF8.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                    Console.WriteLine(" >> " + serverResponse);
                }
                catch(System.IO.IOException ex)
                {
                    clientSocket.Close();
                    Console.WriteLine(" >> Client-" + clNo + " closed!");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                    return;
                }
            }
        }

        private string GetHtmlContent(string dataFromClient)
        {
            string urlAddress = "http://google.com";
            if (dataFromClient.Length > 0) urlAddress = dataFromClient;
            string data = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    data = readStream.ReadToEnd();


                    // Populate the html string here

                    RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                    Regex regx = new Regex("<body>(?<theBody>.*)</body>", options);

                    Match match = regx.Match(data);

                    if (match.Success)
                    {
                        data = match.Groups["theBody"].Value;
                    }

                    response.Close();
                    readStream.Close();
                }
            }
            catch (Exception)
            {
            }

            if (data.Length > 0) return data;
            else
                return "404- Not Found";
        }
    }
}
