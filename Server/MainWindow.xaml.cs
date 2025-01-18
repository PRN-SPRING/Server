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
using System.IO;
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
        private const int TEXT_MESSAGE = 1;
        private const int FILE_MESSAGE = 2;


        public MainWindow()
        {
            InitializeComponent();
            Directory.CreateDirectory("UploadedFiles");
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PortInput.Text, out int port) && port > 0 && port <= 65535)
            {
                Task.Run(() =>
                {
                    string host = "127.0.0.1";
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

        //private void ProcessMessage(object parm)
        //{
        //    string data;
        //    int count;
        //    TcpClient senderClient = parm as TcpClient;

        //    try
        //    {
        //        byte[] bytes = new byte[256];
        //        NetworkStream senderStream = senderClient.GetStream();

        //        while ((count = senderStream.Read(bytes, 0, bytes.Length)) != 0)
        //        {
        //            data = Encoding.ASCII.GetString(bytes, 0, count);
        //            LogMessage($"Received from client: {data}");

        //            // Broadcast to all other connected clients
        //            lock (connectedClients)
        //            {
        //                foreach (var client in connectedClients)
        //                {
        //                    if (client != senderClient) // Don't send the message back to the sender
        //                    {
        //                        try
        //                        {
        //                            NetworkStream stream = client.GetStream();
        //                            byte[] msg = Encoding.ASCII.GetBytes(data);
        //                            stream.Write(msg, 0, msg.Length);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            LogMessage($"Error sending to a client: {ex.Message}");
        //                        }
        //                    }
        //                }
        //            }

        //            LogMessage($"Broadcasted: {data}");
        //        }

        //    }
        //    //catch (Exception ex)
        //    //{
        //    //    LogMessage($"Error: {ex.Message}");
        //    //}
        //    finally
        //    {
        //        lock (connectedClients)
        //        {
        //            connectedClients.Remove(senderClient);
        //        }
        //        senderClient.Close();
        //        LogMessage("Client Disconnect");
        //    }
        //}

        private void ProcessMessage(object parm)
        {
            TcpClient senderClient = parm as TcpClient;
            NetworkStream senderStream = senderClient.GetStream();

            try
            {
                while (true)
                {
                    // Read message type
                    byte[] messageTypeBuffer = new byte[1];
                    int bytesRead = senderStream.Read(messageTypeBuffer, 0, 1);
                    if (bytesRead == 0) break;

                    int messageType = messageTypeBuffer[0];

                    switch (messageType)
                    {
                        case TEXT_MESSAGE:
                            HandleTextMessage(senderClient, senderStream);
                            break;
                        case FILE_MESSAGE:
                            HandleFileMessage(senderClient, senderStream);
                            break;
                        default:
                            LogMessage($"Unknown message type: {messageType}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing message: {ex.Message}");
            }
            finally
            {
                lock (connectedClients)
                {
                    connectedClients.Remove(senderClient);
                }
                senderClient.Close();
                LogMessage("Client disconnected");
            }
        }

        private void HandleTextMessage(TcpClient senderClient, NetworkStream senderStream)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = senderStream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                LogMessage($"Received text message: {message}");

                // Broadcast to all other clients
                BroadcastMessage(senderClient, buffer, bytesRead);
            }
        }

        private void HandleFileMessage(TcpClient senderClient, NetworkStream senderStream)
        {
            try
            {
                // Read filename length
                byte[] lengthBuffer = new byte[4];
                senderStream.Read(lengthBuffer, 0, 4);
                int fileNameLength = BitConverter.ToInt32(lengthBuffer, 0);

                // Read filename
                byte[] fileNameBuffer = new byte[fileNameLength];
                senderStream.Read(fileNameBuffer, 0, fileNameLength);
                string fileName = Encoding.UTF8.GetString(fileNameBuffer);

                // Read file size
                byte[] sizeBuffer = new byte[4];
                senderStream.Read(sizeBuffer, 0, 4);
                int fileSize = BitConverter.ToInt32(sizeBuffer, 0);

                // Read file data
                byte[] fileBuffer = new byte[fileSize];
                int totalBytesRead = 0;

                while (totalBytesRead < fileSize)
                {
                    int remainingBytes = fileSize - totalBytesRead;
                    int bytesRead = senderStream.Read(fileBuffer, totalBytesRead,
                        Math.Min(4096, remainingBytes));

                    if (bytesRead == 0) break;
                    totalBytesRead += bytesRead;

                    Dispatcher.Invoke(() => {
                        if (MessageList.Items.Count > 0)
                            MessageList.Items[MessageList.Items.Count - 1] = $"{DateTime.Now:t} - Upload progress: {totalBytesRead / fileSize * 100}%";
                        else
                            MessageList.Items.Add($"{DateTime.Now:t} - Upload progress: {totalBytesRead / fileSize * 100}%");
                    });
                }

                // Save file
                string filePath = Path.Combine("UploadedFiles", fileName);
                File.WriteAllBytes(filePath, fileBuffer);

                LogMessage($"File received and saved: {fileName} ({fileSize} bytes)");

                // Broadcast success message to all clients
                string successMessage = $"File '{fileName}' has been uploaded to server.";
                byte[] broadcastMessage = Encoding.UTF8.GetBytes(successMessage);
                BroadcastMessage(senderClient, broadcastMessage, broadcastMessage.Length);
            }
            catch (Exception ex)
            {
                LogMessage($"Error handling file upload: {ex.Message}");
            }
        }

        private void BroadcastMessage(TcpClient sender, byte[] message, int messageLength)
        {
            lock (connectedClients)
            {
                foreach (var client in connectedClients)
                {
                    if (client != sender) // Don't send back to sender
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            stream.Write(message, 0, messageLength);
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error broadcasting to client: {ex.Message}");
                        }
                    }
                }
            }
            LogMessage("Message broadcasted to all clients");
        }

        //private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        //{
        //    string message = MessageInput.Text;
        //    if (!string.IsNullOrWhiteSpace(message))
        //    {
        //        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        //        lock (connectedClients)
        //        {
        //            foreach (var client in connectedClients)
        //            {
        //                try
        //                {
        //                    NetworkStream stream = client.GetStream();
        //                    stream.Write(messageBytes, 0, messageBytes.Length);
        //                }
        //                catch (Exception ex)
        //                {
        //                    LogMessage($"Error sending message to a client: {ex.Message}");
        //                }
        //            }
        //        }
        //        LogMessage($"Server sent: {message}");
        //        MessageInput.Clear();
        //    }
        //}

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageInput.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                // Send message type first (TEXT_MESSAGE = 1)
                byte[] messageType = new byte[] { TEXT_MESSAGE };
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                lock (connectedClients)
                {
                    foreach (var client in connectedClients)
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            stream.Write(messageType, 0, 1);
                            stream.Write(messageBytes, 0, messageBytes.Length);
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error sending message to client: {ex.Message}");
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

