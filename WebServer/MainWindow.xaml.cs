using System;
using System.Threading.Tasks;
using System.Windows;

namespace WebServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void startServerButton_Click(object sender, RoutedEventArgs e)
        {
            // Call startServer() asynchronously to avoid UI freeze
            await startServer();
        }

        Server PrepareServer()
        {
            // Create new Server instance
            var server = new Server();

            // Register ConsoleTextUpdateEvent delegate
            server.ConsoleTextUpdate += (s, e) => Dispatcher.Invoke((Action)delegate()
            {
                outputConsoleTextBox.Text += e.message + "\r\n";
            });

            return server;
        }

        Task startServer()
        {
            var server = PrepareServer();

            // Start Server task in separate thread to avoid UI freeze
            return Task.Run(() => { server.StartServer(); });
        }

        private void stopServerButton_Click(object sender, RoutedEventArgs e)
        {
            outputConsoleTextBox.Text += "Not implemented \r\n";
        }
    }
}
