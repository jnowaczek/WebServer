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
            await startServer();
        }

        Server PrepareServer()
        {
            var server = new Server();
            server.ConsoleTextUpdate += (s, e) => Dispatcher.Invoke((Action)delegate()
            {
                outputConsoleTextBox.Text += e.message + "\r\n";
            });
            return server;
        }

        Task startServer()
        {
            var server = PrepareServer();
            return Task.Run(() => { server.StartServer(); });
        }

        private void stopServerButton_Click(object sender, RoutedEventArgs e)
        {
            outputConsoleTextBox.Text += "Not implemented \r\n";
        }
    }
}
