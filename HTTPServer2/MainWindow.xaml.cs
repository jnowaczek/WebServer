using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HTTPServer2
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

        private Server PrepareServer()
        {
            // Create new Server instance
            var server = new Server();

            // Register ConsoleTextUpdateEvent delegate
            server.ConsoleTextUpdate += (s, e) => Dispatcher.Invoke((Action)delegate ()
            {
                outputConsoleTextBox.Text += e.message + "\r\n";
            });

            return server;
        }

        Task startServer()
        {
            var server = PrepareServer();

            // Start Server task in separate thread to avoid UI freeze
            return Task.Run(() => { server.RunServer(); });
        }

        private void stopServerButton_Click(object sender, RoutedEventArgs e)
        {
            outputConsoleTextBox.Text += "Not implemented \r\n";
        }
    }
}
