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


namespace Oh_My_Ping.Proxy {
    internal class Proxy {
        private string address;
        private int port;

        private List<byte[]> toTargetCache = new List<byte[]>();
        private List<byte[]> toClientCache = new List<byte[]>();

        private bool closeTargetSock;
        private bool closeClientSock;


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
                await handle(clientSock);

            }
        }

        private async Task handle(TcpClient clientSock) {
            closeClientSock = false;
            closeTargetSock = false;

            TcpClient targetSock = new TcpClient();
            await targetSock.ConnectAsync(address, port);
            Console.WriteLine("Connected to Target");
            NetworkStream targetStream = targetSock.GetStream();
            NetworkStream clientStream = clientSock.GetStream();

            Task t1 = handleClient(clientStream);
            Task t2 = handleTarget(targetStream);
            Task t3 = toClient(clientSock, clientStream);
            Task t4 = toTarget(targetSock, targetStream);


            await t1; 
            await t2; 
            await t3; 
            await t4;
        }


        private async Task handleClient(NetworkStream clientStream) {

            byte[] data = new byte[4096];
            int i;
            try {
                while (((i = await clientStream.ReadAsync(data, 0, data.Length)) != 0) && !closeClientSock) {
                    byte[] result = data.Take(i).ToArray();
                    //Console.WriteLine("received from Client : " + String.Join(",", result));
                    toTargetCache.Add(result);
                }
                toTargetCache.Add(null);

            } catch (Exception ex) {
                //Console.WriteLine(ex.StackTrace);
            }
        }


        private async Task handleTarget(NetworkStream targetStream) {

            byte[] data = new byte[4096];
            int i;
            try {
                while (((i = await targetStream.ReadAsync(data, 0, data.Length)) != 0) && !closeTargetSock) {
                    byte[] result = data.Take(i).ToArray();
                    //Console.WriteLine("received from Target : " + String.Join(",", result));
                    toClientCache.Add(result);
                }
                toClientCache.Add(null);

            } catch (Exception ex) {
                //Console.WriteLine(ex.StackTrace);
            }
            

        }


        private async Task toTarget(TcpClient targetSock, NetworkStream targetStream) {
            while (!closeTargetSock) {
                while (toTargetCache.Count != 0) {
                    byte[] data = toTargetCache[0];
                    toTargetCache.RemoveAt(0);
                    if (data == null) {
                        closeTargetSock = true;
                        targetSock.Close();
                        return;
                    }
                    await targetStream.WriteAsync(data, 0, data.Length);

                }
                await Task.Delay(1);
            }
        }

        private async Task toClient(TcpClient clientSock, NetworkStream clientStream) {
            while (!closeClientSock) {
                while (toClientCache.Count != 0) {
                    byte[] data = toClientCache[0];
                    toClientCache.RemoveAt(0);
                    if (data == null) {
                        closeClientSock = true;
                        clientSock.Close();
                        return;
                    }
                    await clientStream.WriteAsync(data, 0, data.Length);

                }
                await Task.Delay(1);
            }
        }
    }
}
