//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;

//namespace Server
//{
//    public partial class MainWindow : Window
//    {
//        private TcpListener server = null;
//        private int clientCount = 0;

//        public MainWindow()
//        {
//            InitializeComponent();
//        }

//        private void StartServerButton_Click(object sender, RoutedEventArgs e)
//        {
//            Task.Run(() =>
//            {
//                string host = "127.0.0.1";
//                int port = 13000;
//                ExecuteServer(host, port);
//            });
//        }


//        private void ExecuteServer(string host, int port)
//        {
//            try
//            {
//                IPAddress localAddr = IPAddress.Parse(host);
//                server = new TcpListener(localAddr, port);
//                server.Start();
//                LogMessage("Server started... Waiting for connections...");

//                while (true)
//                {
//                    TcpClient client = server.AcceptTcpClient();
//                    clientCount++;
//                    LogMessage($"Client #{clientCount} connected.");
//                    Thread thread = new Thread(new ParameterizedThreadStart(ProcessMessage));
//                    thread.Start(client);
//                }
//            }
//            catch (Exception ex)
//            {
//                LogMessage($"Exception: {ex.Message}");
//            }
//        }

//        private void ProcessMessage(object parm)
//        {
//            string data;
//            int count;
//            try
//            {
//                TcpClient client = parm as TcpClient;
//                byte[] bytes = new byte[256];
//                NetworkStream stream = client.GetStream();
//                while ((count = stream.Read(bytes, 0, bytes.Length)) != 0)
//                {
//                    data = Encoding.ASCII.GetString(bytes, 0, count);
//                    LogMessage($"Received: {data}");
//                    data = data.ToUpper();
//                    byte[] msg = Encoding.ASCII.GetBytes(data);
//                    stream.Write(msg, 0, msg.Length);
//                    LogMessage($"Sent: {data}");
//                }
//                client.Close();
//            }
//            catch (Exception ex)
//            {
//                LogMessage($"Error: {ex.Message}");
//            }
//        }

//        private void LogMessage(string message)
//        {
//            // Update the UI with the log message (safe cross-thread invocation)
//            Dispatcher.Invoke(() =>
//            {
//                MessageList.Items.Add($"{DateTime.Now:t} - {message}");
//                MessageList.SelectedIndex = MessageList.Items.Count - 1; // Scroll to the last message
//            });
//        }
//    }
//}
using System;
using System.Collections.Generic;
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
        private readonly List<TcpClient> connectedClients = new List<TcpClient>(); // Track connected clients

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PortInput.Text, out int port) && port > 0 && port <= 65535)
            {
                Task.Run(() =>
                {
                    string host = "192.168.43.20";
                    ExecuteServer(host, port);
                });
            }
            else
            {
                LogMessage("Invalid port number. Please enter a number between 1 and 65535.");
            }
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
                    lock (connectedClients)
                    {
                        connectedClients.Add(client);
                    }
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

        //private void ProcessMessage(object parm)
        //{
        //    string data;
        //    int count;
        //    try
        //    {
        //        TcpClient client = parm as TcpClient;
        //        byte[] bytes = new byte[256];
        //        NetworkStream stream = client.GetStream();
        //        while ((count = stream.Read(bytes, 0, bytes.Length)) != 0)
        //        {
        //            data = Encoding.ASCII.GetString(bytes, 0, count);
        //            LogMessage($"Received: {data}");
        //            data = data.ToUpper();
        //            byte[] msg = Encoding.ASCII.GetBytes(data);
        //            stream.Write(msg, 0, msg.Length);
        //            LogMessage($"Sent: {data}");
        //        }
        //        lock (connectedClients)
        //        {
        //            connectedClients.Remove(client);
        //        }
        //        client.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogMessage($"Error: {ex.Message}");
        //    }
        //}
        private void ProcessMessage(object parm)
        {
            string data;
            int count;
            TcpClient senderClient = parm as TcpClient;

            try
            {
                byte[] bytes = new byte[256];
                NetworkStream senderStream = senderClient.GetStream();

                while ((count = senderStream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    data = Encoding.ASCII.GetString(bytes, 0, count);
                    LogMessage($"Received from client: {data}");

                    // Broadcast to all other connected clients
                    lock (connectedClients)
                    {
                        foreach (var client in connectedClients)
                        {
                            if (client != senderClient) // Don't send the message back to the sender
                            {
                                try
                                {
                                    NetworkStream stream = client.GetStream();
                                    byte[] msg = Encoding.ASCII.GetBytes(data);
                                    stream.Write(msg, 0, msg.Length);
                                }
                                catch (Exception ex)
                                {
                                    LogMessage($"Error sending to a client: {ex.Message}");
                                }
                            }
                        }
                    }

                    LogMessage($"Broadcasted: {data}");
                }

            }
            //catch (Exception ex)
            //{
            //    LogMessage($"Error: {ex.Message}");
            //}
            finally
            {
                lock (connectedClients)
                {
                    connectedClients.Remove(senderClient);
                }
                senderClient.Close();
                LogMessage("Client Disconnect");
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageInput.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                lock (connectedClients)
                {
                    foreach (var client in connectedClients)
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            stream.Write(messageBytes, 0, messageBytes.Length);
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error sending message to a client: {ex.Message}");
                        }
                    }
                }
                LogMessage($"Server sent: {message}");
                MessageInput.Clear();
            }
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                MessageList.Items.Add($"{DateTime.Now:t} - {message}");
                MessageList.SelectedIndex = MessageList.Items.Count - 1; // Scroll to the last message
            });
        }

        private void PortInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}

