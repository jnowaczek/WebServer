using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebServer
{
    public delegate void ConsoleTextUpdateEventHandler(object sender, ConsoleTextEventArgs e);

    class Server
    {
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private bool runServer = true;

        public event ConsoleTextUpdateEventHandler ConsoleTextUpdate;
        
        public Server()
        {
            
        }

        public void StartServer()
        {
            try
            {
                var port = 8080;
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                
                while (runServer)
                {
                    tcpClient = tcpListener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(AcceptTcpClient, tcpClient);

                }
            }
            catch (Exception e)
            {
                DoConsoleTextUpdate("Error starting server: " + e.Message);
            }
        }

        public void HaltServer()
        {
            runServer = false;
        }

        private void AcceptTcpClient(object obj)
        {
            var tcpClient = (TcpClient)obj;
            NetworkStream networkStream = tcpClient.GetStream();

            using (StreamReader networkReader = new StreamReader(networkStream))
            {
                string request = networkReader.ReadLine();
                DoConsoleTextUpdate(request);

                String html = null;

                String[] requestComponents = request.Split(' ');

                StringBuilder builder = new StringBuilder();

                if (requestComponents[0] != "GET")
                {
                    builder.Append("HTTP/1.1 501 NOT IMPLEMENTED\r\n");

                }
                else if (requestComponents[1].Equals("/") || File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + 
                    "\\www\\" + requestComponents[1]))
                {
                    String file;

                    if (requestComponents[1].Equals("/"))
                    {
                        file = "index.html";
                    } else
                    {
                        file = requestComponents[1];
                    }

                    using (StreamReader fileReader = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory 
                        + "\\www\\" + file))
                    {
                        html = fileReader.ReadToEnd();
                    }

                    int contentLength = System.Text.ASCIIEncoding.ASCII.GetByteCount(html);

                    builder.Append("HTTP/1.1 200 OK\r\n");
                    builder.Append("Connection: keep-alive\r\n");
                    builder.Append("Content-Type: text/html; charset=utf-8\r\n");
                    builder.Append("Content-Length: ").Append(contentLength).Append("\r\n");
                }
                else
                {
                    builder.Append("HTTP/1.1 404 NOT FOUND\r\n");

                    using (StreamReader fileReader = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory
                        + "\\www\\404.html"))
                    {
                        html = fileReader.ReadToEnd();
                    }
                }
                
                using (StreamWriter networkWriter = new StreamWriter(networkStream))
                {
                    String responseHeader = builder.ToString();

                    networkWriter.WriteLine(responseHeader);
                    if (html != null)
                    {
                        networkWriter.WriteLine(html);
                    }
                    networkWriter.Flush();

                    networkStream.Close();
                }
            }
        }

        protected virtual void DoConsoleTextUpdate(String s)
        {
            if (ConsoleTextUpdate != null)
            {
                ConsoleTextUpdate(this, new ConsoleTextEventArgs(s));
            }
        }
    }

    public class ConsoleTextEventArgs : EventArgs
    {
        public String message;

        public ConsoleTextEventArgs(String message)
        {
            this.message = message;
        }
    }
}
