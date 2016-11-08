using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;

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

                String html;

                using (StreamReader fileReader = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory + "\\index.html"))
                {
                    html = fileReader.ReadToEnd();
                }

                int contentLength = System.Text.ASCIIEncoding.ASCII.GetByteCount(html);

                using (StreamWriter networkWriter = new StreamWriter(networkStream))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("HTTP/1.1 200 OK \r\n");
                    builder.Append("Connection: keep-alive \r\n");
                    builder.Append("Content-Type: text/html \r\n");
                    builder.Append("Content-Length: ").Append(contentLength).Append(" \r\n");

                    networkWriter.Write("HTTP/1.1 200 OK");
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
