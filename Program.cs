using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace RCp_Phase1 {
    static class Program {

        private static string HTTP_CODE_REDIRECT = "302";

        public static void Main(string[] args)
        {
            if(args == null || args.Length != 1 )
                Console.WriteLine("An url and port must be defined in order to complete the execution.");

            try
            {
                var uri = new Uri(args[0]);
                string result = ManageRequest(uri, uri.Port);
                Console.WriteLine(result);
            }
            catch (Exception ex) {
                Console.WriteLine($"As error has ocurred: {ex}");
            }

            Console.Read();
        }

        private static string ManageRequest(Uri uri, int port)
        {
            // Create a socket connection with the specified server and port.
            using (Socket socket = ConnectSocket(uri.Host, port))
            {
                if (socket == null || !socket.Connected)
                    return "Connection failed";

                string url = uri.AbsoluteUri;
                string httpCode, response;

                do
                {
                    response = SendRequest(socket, uri.Host, url);

                    var lines = response.Split("\r\n");
                    httpCode = lines[0].Split(" ")[1];

                    if (httpCode == HTTP_CODE_REDIRECT)  //make request to the new location
                        url = lines.First(s => s.StartsWith("location", StringComparison.OrdinalIgnoreCase)).Split(" ")[1];

                } while (httpCode == HTTP_CODE_REDIRECT);

                return response;
            }
        }

        private static string SendRequest(Socket socket, string host, string url) {

            string request = $"GET {url} HTTP/1.1\r\nHost: {host}\r\n\r\n"; //"\r\nConnection: Close\r\n\r\n";

            string response = string.Empty;
            var bytesSent = Encoding.ASCII.GetBytes(request);
            var bytesReceived = new byte[256];
            int bytes;

            // Send request to the server.
            socket.Send(bytesSent, bytesSent.Length, 0);

            // The following will block until the page is received.
            do { 
                bytes = socket.Receive(bytesReceived, bytesReceived.Length, 0);
                response += Encoding.ASCII.GetString(bytesReceived, 0, bytes);
            }
            while (bytes > 0);

            return response;
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
