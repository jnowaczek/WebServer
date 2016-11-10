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

        // Register event provider
        public event ConsoleTextUpdateEventHandler ConsoleTextUpdate;

        public Server()
        {

        }

        public void StartServer()
        {
            try
            {
                var port = 8080;
                var totalThreads = 0;

                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();

                while (runServer)
                {
                    tcpClient = tcpListener.AcceptTcpClient();
                    /*
                     * Spawn a new thread from the thread pool for each new TCP connection. Default thread pool size
                     * is 1024 I think
                    */
                    ThreadPool.QueueUserWorkItem(AcceptTcpClient, tcpClient);

                    int maxThreads;
                    int availableThreads;
                    // Apparently there's no way to discard an out variable you don't want
                    int dontCare;

                    ThreadPool.GetMaxThreads(out maxThreads, out dontCare);
                    ThreadPool.GetAvailableThreads(out availableThreads, out dontCare);

                    totalThreads += 1;
                    DoConsoleTextUpdate("Threads available in pool: + " + availableThreads + " out of " + maxThreads
                        + " total\r\nThreads created so far: " + totalThreads);
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
            try
            {
                var tcpClient = (TcpClient)obj;
                NetworkStream networkStream = tcpClient.GetStream();

                using (StreamReader networkReader = new StreamReader(networkStream))
                {
                    string request = networkReader.ReadLine();
                    DoConsoleTextUpdate(request);

                    String html = null;

                    // Split request by spaces, first word is request type
                    if (request == null)
                    {
                        throw new Exception("Invalid request");
                    }
                    String[] requestComponents = request.Split(' ');

                    StringBuilder builder = new StringBuilder();

                    // Return status code 501 for methods that do not work
                    if (requestComponents[0].Equals("GET"))
                    {
                        // Serve index.html if '/' is requested, otherwise serve the page if it exists
                        if (requestComponents[1].Equals("/"))
                        {
                            requestComponents[1] = "/index.html";
                        }

                        if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "\\www\\" + requestComponents[1]))
                        {
                            /* 
                             * Read HTML file into memory, this will read non-text files but images and most other 
                             * binary files will fail, somehow the bytes get mangled in here
                             */
                            using (StreamReader fileReader = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory
                                + "\\www\\" + requestComponents[1]))
                            {
                                html = fileReader.ReadToEnd();
                            }

                            // Server has hardcoded Content-Type so content other than text will likely not display properly
                            builder.Append("HTTP/1.1 200 OK\r\n");
                        }
                        // If the page requested doesn't exist serve the 404 page and error code.
                        else
                        {
                            builder.Append("HTTP/1.1 404 NOT FOUND\r\n");

                            using (StreamReader fileReader = new StreamReader(System.AppDomain.CurrentDomain.BaseDirectory
                                + "\\www\\404.html"))
                            {
                                html = fileReader.ReadToEnd();
                            }
                        }

                        // Get the content length of the HTML file (in bytes)
                        int contentLength = System.Text.ASCIIEncoding.ASCII.GetByteCount(html);

                        builder.Append("Connection: keep-alive\r\n");
                        builder.Append("Content-Type: text/html; charset=utf-8\r\n");
                        builder.Append("Content-Length: ").Append(contentLength).Append("\r\n");
                    }
                    else if (requestComponents[0].Equals("POST"))
                    {
                        builder.Append("HTTP/1.1 501 NOT IMPLEMENTED\r\n");
                    }
                    else
                    {
                        builder.Append("HTTP/1.1 501 NOT IMPLEMENTED\r\n");
                    }

                    using (StreamWriter networkWriter = new StreamWriter(networkStream))
                    {
                        // Finalize the response header
                        String responseHeader = builder.ToString();

                        // Have to write the header before the HTML or it doesn't work
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
            catch (Exception e)
            {
                DoConsoleTextUpdate(e.Message);
            }
        }

        /* 
         * Helper method for passing text to the console. Since the methods that need to write text to the console
         * are in different threads, they cannot directly modify the Text attribute of the TextBox control. The
         * way around this is to use events (just like the button click events) to pass messages between threads.
        */
        protected virtual void DoConsoleTextUpdate(String s)
        {
            if (ConsoleTextUpdate != null)
            {
                ConsoleTextUpdate(this, new ConsoleTextEventArgs(s));
            }
        }
    }


    // Class to provide event data
    public class ConsoleTextEventArgs : EventArgs
    {
        public String message;

        public ConsoleTextEventArgs(String message)
        {
            this.message = message;
        }
    }
}
