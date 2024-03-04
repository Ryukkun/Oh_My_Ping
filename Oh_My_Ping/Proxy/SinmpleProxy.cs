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
    internal class SimpleProxy {
        private string address;
        private int port;

        public SimpleProxy(string _address) {
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
            TcpClient targetSock = new TcpClient();
            await targetSock.ConnectAsync(address, port);
            Console.WriteLine("Connected to Target");
            NetworkStream targetStream = targetSock.GetStream();
            NetworkStream clientStream = clientSock.GetStream();

            Task t1 = handleClient(targetSock, clientSock, targetStream, clientStream);
            Task t2 = handleTarget(targetSock, clientSock, targetStream, clientStream);


            await t1;
            await t2;
        }


        private async Task handleClient(TcpClient targetSock, TcpClient clientSock, NetworkStream targetStream, NetworkStream clientStream) {

            byte[] data = new byte[4096];
            int i;
            try {
                while ((i = await clientStream.ReadAsync(data, 0, data.Length)) != 0) {
                    //Console.WriteLine("received from Client : " + String.Join(",", result));
                    await targetStream.WriteAsync(data, 0, i);
                }

                targetSock.Close();
                clientSock.Close();
            } catch (Exception) {
                //Console.WriteLine(ex.StackTrace);
            }
        }


        private async Task handleTarget(TcpClient targetSock, TcpClient clientSock, NetworkStream targetStream, NetworkStream clientStream) {

            byte[] data = new byte[4096];
            int i;
            try {
                while ((i = await targetStream.ReadAsync(data, 0, data.Length)) != 0) {
                    //Console.WriteLine("received from Target : " + String.Join(",", result));
                    await clientStream.WriteAsync(data, 0, i);
                }

                targetSock.Close();
                clientSock.Close();
            } catch (Exception) {
                //Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
