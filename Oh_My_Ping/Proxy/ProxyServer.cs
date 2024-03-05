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
using System.Collections;
using System.Timers;


namespace Oh_My_Ping.Proxy {
    internal class ProxyServer {
        private string address;
        private int port;
        private TcpListener server;
        public bool isRunning = true;
        private Proxy proxyClient = null;



        public ProxyServer(string _address) {
            (address, port) = getAddress(_address);

            loop_server();
        }


        public static (string, int) getAddress(string address) {
            string[] s = address.Split(':');
            if (s.Length == 1) {
                return (address, 25565);

            } else {
                return (s[0], int.Parse(s[1]));
            }
        }



        private async void loop_server() {
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 25565);

            server.Start();
            Console.Write("Waiting for a connection... ");
            

            try {
                while (isRunning) {
                    MainWindow.changeStatus("Waiting for a connection...", 1);
                    TcpClient clientSock = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Connected!");
                    MainWindow.changeStatus("Connecting!!", 2);
                    proxyClient = new ProxyServer.Proxy(clientSock, address, port);
                    await proxyClient.start();
                }
            } catch (InvalidOperationException) { 
                // stop時に発生
            } catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
            }
        }


        public void close() {
            isRunning = false;
            server.Stop();
            if (proxyClient?.isClosed() == false) {
                proxyClient.close();
            }
        }




        private class Proxy {

            string address;
            int port;

            NetworkStream targetStream;
            NetworkStream clientStream;
            TcpClient clientSock;
            TcpClient targetSock;

            private List<Packet> toTargetCache = new List<Packet>();
            private List<Packet> toClientCache = new List<Packet>();

            private bool closeTargetSock;
            private bool closeClientSock;

            Task toTargetTask = null;
            Task toClientTask = null;



            public Proxy(TcpClient _clientSock, string _address, int _port) {
                clientSock = _clientSock;
                address = _address; 
                port = _port;
            }


            public bool isClosed() {
                return (closeTargetSock && closeClientSock);
            }


            public void close() { 
                clientSock.Close();
                targetSock.Close();
                closeClientSock = true;
                closeTargetSock = true;
            }



            public async Task start() {
                closeClientSock = false;
                closeTargetSock = false;

                targetSock = new TcpClient();
                await targetSock.ConnectAsync(address, port);
                Console.WriteLine("Connected to Target");
                targetStream = targetSock.GetStream();
                clientStream = clientSock.GetStream();

                Task t1 = handleClient();
                Task t2 = handleTarget();
                Task t3 = toClientLoop();
                Task t4 = toTargetLoop();

                await t1; 
                await t2; 
            }


            private async Task handleClient() {

                byte[] data = new byte[4096];
                int i;

                try {
                    while (((i = await clientStream.ReadAsync(data, 0, data.Length)) != 0) && !closeClientSock) {
                        byte[] result = data.Take(i).ToArray();
                        //Console.WriteLine("received from Client : " + String.Join(",", result));
                        toTargetCache.Add(new Packet(result));
                        toTarget();
                    }
                    toTargetCache.Add(null);
                    toTarget();

                } catch (Exception) {
                    //Console.WriteLine(ex.StackTrace);
                }
            }


            private async Task handleTarget() {

                byte[] data = new byte[4096];
                int i;

                try {
                    while (((i = await targetStream.ReadAsync(data, 0, data.Length)) != 0) && !closeTargetSock) {
                        byte[] result = data.Take(i).ToArray();
                        //Console.WriteLine("received from Target : " + String.Join(",", result));
                        toClientCache.Add(new Packet(result));
                        toClient();
                    }
                    toClientCache.Add(null);
                    toClient();

                } catch (Exception) {
                    //Console.WriteLine(ex.StackTrace);
                }
            

            }

            private async Task toTargetLoop() {
                while (!closeTargetSock) {
                    toTarget();
                    await Task.Delay(1);
                }
            }


            private void toTarget() {
                if (toTargetTask?.IsCompleted != false && !closeTargetSock) {
                    toTargetTask = _toTarget();
                }
            }



            private async Task _toTarget() {
                while ((toTargetCache.Count != 0) && !closeTargetSock) {
                    Packet packet = toTargetCache[0];
                    if (packet == null) {
                        closeTargetSock = true;
                        close();
                        toTargetCache.RemoveAt(0);
                        return;
                    }
                    if (!(packet.time+MainWindow.delay/2 - 8 < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())) {
                        return;
                    }

                    toTargetCache.RemoveAt(0);
                    await targetStream.WriteAsync(packet.data, 0, packet.data.Length);
                }
            }





            private async Task toClientLoop() {
                while (!closeClientSock) {
                    toClient();
                    await Task.Delay(1);
                }
            }


            private void toClient() {
                if (toClientTask?.IsCompleted != false && !closeClientSock) {
                    toClientTask = _toClient();
                }
            }



            private async Task _toClient() {
                while ((toClientCache.Count != 0) && !closeClientSock) {
                    Packet packet = toClientCache[0];
                    if (packet == null) {
                        closeClientSock = true;
                        close();
                        toTargetCache.RemoveAt(0);
                        return;
                    }
                    if (!(packet.time + MainWindow.delay/2 - 8 < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())) {
                        return;
                    }

                    toClientCache.RemoveAt(0);
                    await clientStream.WriteAsync(packet.data, 0, packet.data.Length);
                } 
            }
        }


        private class Packet {
            public byte[] data;
            public long time;
            public Packet(byte[] _data) {
                data = _data;
                time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }
    }
}
