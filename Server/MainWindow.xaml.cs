using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Server
{
    public partial class MainWindow : Window
    {
        private TcpListener server = null;
        private int clientCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                string host = "127.0.0.1";
                int port = 13000;
                ExecuteServer(host, port);
            });
        }


        private void ExecuteServer(string host, int port)
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse(host);
                server = new TcpListener(localAddr, port);
                server.Start();
                LogMessage("Server started... Waiting for connections...");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    clientCount++;
                    LogMessage($"Client #{clientCount} connected.");
                    Thread thread = new Thread(new ParameterizedThreadStart(ProcessMessage));
                    thread.Start(client);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Exception: {ex.Message}");
            }
        }

        private void ProcessMessage(object parm)
        {
            string data;
            int count;
            try
            {
                TcpClient client = parm as TcpClient;
                byte[] bytes = new byte[256];
                NetworkStream stream = client.GetStream();
                while ((count = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    data = Encoding.ASCII.GetString(bytes, 0, count);
                    LogMessage($"Received: {data}");
                    data = data.ToUpper();
                    byte[] msg = Encoding.ASCII.GetBytes(data);
                    stream.Write(msg, 0, msg.Length);
                    LogMessage($"Sent: {data}");
                }
                client.Close();
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            // Update the UI with the log message (safe cross-thread invocation)
            Dispatcher.Invoke(() =>
            {
                MessageList.Items.Add($"{DateTime.Now:t} - {message}");
                MessageList.SelectedIndex = MessageList.Items.Count - 1; // Scroll to the last message
            });
        }
    }
}
