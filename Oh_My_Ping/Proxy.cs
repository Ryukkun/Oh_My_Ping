using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Windows;


namespace Oh_My_Ping {
    internal class Proxy {
        private string address;
        private int port;

        private MemoryStream toServerCache = new MemoryStream();
        private MemoryStream toClientCache = new MemoryStream();
        public Proxy(string _address) {
            string[] s = _address.Split(':');
            if (s.Length == 1) {
                address = _address;
                port = 25565;

            } else {
                address = s[0];
                port = int.Parse(s[1]);
            }

            loop_server();
        }


        private async void loop_server() {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 25565);
            server.Start();
            Console.Write("Waiting for a connection... ");

            while (true) {
                TcpClient clientSock = await server.AcceptTcpClientAsync();
                Console.WriteLine("Connected!");
                handle(clientSock);

            }
        }

        private async Task handle(TcpClient clientSock) {
            TcpClient targetSock = new TcpClient();
            await targetSock.ConnectAsync(address, port);
            Console.WriteLine("Connected to Target");
            NetworkStream targetStream = targetSock.GetStream();
            NetworkStream clientStream = clientSock.GetStream();

            handleClient(targetSock, clientSock, targetStream, clientStream);
            handleTarget(targetSock, clientSock, targetStream, clientStream);
        }


        private async Task handleClient(TcpClient targetSock, TcpClient clientSock, NetworkStream targetStream, NetworkStream clientStream) {
 
            byte[] data = new byte[4096];
            int i;
            while ((i = await clientStream.ReadAsync(data, 0, data.Length)) != 0) {
                Console.WriteLine("received from Client : " + String.Join(",", data.Take(i)));
                await targetStream.WriteAsync(data, 0, i);
            }

            targetStream.Close();
            clientStream.Close();
            targetSock.Close();
            clientSock.Close();
        }


        private async Task handleTarget(TcpClient targetSock, TcpClient clientSock, NetworkStream targetStream, NetworkStream clientStream) {

            byte[] data = new byte[4096];
            int i;
            while ((i = await targetStream.ReadAsync(data, 0, data.Length)) != 0) {
                Console.WriteLine("received from Targett : " + String.Join(",", data.Take(i)));
                await clientStream.WriteAsync(data, 0, i);
            }

            targetStream.Close();
            clientStream.Close();
            targetSock.Close();
            clientSock.Close();
        }



        private async Task fromServer() {
            TcpClient client = null;
            //new Thread(new ThreadStart(fromCliant)).Start();

            try {
                int port;

                client = new TcpClient();

                // Start Connect
                string[] _address = address.Split(':');
                if (_address.Length == 1) {
                    port = 25565;
                
                } else {
                    address = _address[0];
                    port = int.Parse(_address[1]);
                }


                // Buffer for reading data

                byte[] bytes = new byte[4096];
                int i;
                String data = null;

                // Enter the listening loop.
                while (true) {
                    if (toServerCache.Length != 0) {
                        serverStream?.Close();
                        client.Connect(address, port);
                        serverStream = client.GetStream();

                        while ((i = await toServerCache.ReadAsync(bytes, 0, bytes.Length)) != 0) {
                            serverStream.Write(bytes, 0, i);
                        }
                        toServerCache.Dispose();
                        
                    }

                    data = null;
                    await Task.Delay(10);
                    if (serverStream == null) { continue; }
                    while ((i = await serverStream.ReadAsync(bytes, 0, bytes.Length)) != 0) {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received from server: {0}", data);

                        // Process the data sent by the client.
                        //data = data.ToUpper();

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        toClientCache.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent to client: {0}", data);
                    }
                }
            } catch (SocketException e) {
                Console.WriteLine("SocketException: {0}", e);
            } finally {
                client.Close();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }



        private async Task fromClient() {
            TcpListener server = null;
            try {
                // Set the TcpListener on port 13000.
                Int32 port = 25565;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                byte[] bytes = new byte[1024];
                String data = null;

                Console.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Connected!");

                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();
                clientStream?.Close();
                clientStream = stream;


                // Enter the listening loop.
                while (true) {


                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0) {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        //data = data.ToUpper();

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        toServerCache.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    if (toClientCache.Length != 0) {
                        while ((i = await toClientCache.ReadAsync(bytes, 0, bytes.Length)) != 0) {
                            clientStream.Write(bytes, 0, i);
                        }
                        toClientCache.Dispose();

                    }
                }
            } catch (SocketException e) {
                Console.WriteLine("SocketException: {0}", e);
            } finally {
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
