using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections;

namespace RCp_Phase1 {
    static class Program {

        private static string HTTP_CODE_PERM_REDIRECT = "301";
        private static string HTTP_CODE_TEMP_REDIRECT = "302";
        private static readonly ArrayList MyResponses = new ArrayList();

        public static void Main(string[] args)
        {
            if(args == null || args.Length != 1 )
                Console.WriteLine("An url and port must be defined in order to complete the execution.");

            try
            {
                Console.WriteLine("Enter IP address: xxx.xxx.xxx.xxx");
                var ipAddress = "https://" + Console.ReadLine();
                var uri = new Uri(ipAddress);
                ManageRequest(uri);
                foreach (var resp in MyResponses)
                {
                    Console.WriteLine("--- HTTP Response ---");
                    Console.WriteLine(resp);
                }
                    
            }
            catch (Exception ex) {
                Console.WriteLine($"As error has ocurred: {ex}");
            }

            Console.Read();
        }

        private static void ManageRequest(Uri uri)
        {
            var host = ResolveName(uri.Host);
            var path = uri.AbsolutePath;
            var port = uri.Port;
            string httpCode, response;

            do {
                response = SendRequest(host, path, port);
                MyResponses.Add(response);
                var lines = response.Split("\r\n");
                httpCode = lines[0].Split(" ")[1];
                
                if (httpCode == HTTP_CODE_TEMP_REDIRECT || httpCode == HTTP_CODE_PERM_REDIRECT) {
                    //make request to the new location
                    var redirectUri = new Uri(lines.First(s => s.StartsWith("location", StringComparison.OrdinalIgnoreCase)).Split(" ")[1]);
                    host = ResolveName(redirectUri.Host);
                    path = redirectUri.AbsolutePath;
                    port = redirectUri.Port;
                }

            } while (httpCode == HTTP_CODE_TEMP_REDIRECT || httpCode == HTTP_CODE_PERM_REDIRECT);
        }

        private static string SendRequest(string host, string path, int port) {

            // Create a socket connection with the specified server and port.
            using (Socket socket = ConnectSocket(host, port)) {

                if (socket?.Connected != true)
                    throw new Exception("Connection failed");

                // Send request to the server.
                var request = $"GET {path} HTTP/1.1\r\nHost: {host}\r\n\r\n";
                var bytesSent = Encoding.ASCII.GetBytes(request);
                var bytesReceived = new byte[256];
                socket.Send(bytesSent, bytesSent.Length, 0);

                // The following will block until the page is received.
                var response = string.Empty; int bytes;
                do {
                    bytes = socket.Receive(bytesReceived, bytesReceived.Length, 0);
                    response += Encoding.ASCII.GetString(bytesReceived, 0, bytes);
                }
                while (bytes == 256);

                return response;
            }
        }

        private static string ResolveName(string host) { 
            return Dns.GetHostEntry(host)
                                .AddressList
                                .First(a => a.AddressFamily == AddressFamily.InterNetwork)
                                .ToString();
        }

        private static Socket ConnectSocket(string server, int port)
        {
            IPEndPoint ipe = IPEndPoint.Parse($"{server}:{port}");
            var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipe);
            return socket;
        }
    }
}
