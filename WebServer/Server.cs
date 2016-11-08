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
                tcpListener = new TcpListener(IPAddress.Loopback, port);
                tcpListener.Start();
                
                while (runServer)
                {
                    tcpClient = tcpListener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(AcceptTcpClient, tcpClient);

                }
            }
            catch (Exception e)
            {
                ConsoleTextEventArgs args = new ConsoleTextEventArgs("Error starting server: " + e.Message);
                OnConsoleTextUpdate(args);
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

            using (StreamReader reader = new StreamReader(networkStream))
            {
                string response = reader.ReadToEnd();

                using (StreamWriter writer = new StreamWriter(networkStream))
                {
                    writer.Write("I wonder what happens now");
                    writer.Flush();

                    networkStream.Close();
                }
            }
        }

        protected virtual void OnConsoleTextUpdate(ConsoleTextEventArgs e)
        {
            if (ConsoleTextUpdate != null)
            {
                ConsoleTextUpdate(this, e);
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
