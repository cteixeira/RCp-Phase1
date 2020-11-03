using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace RCp_Phase1 {
    static class Program {

        public static void Main(string[] args)
        {
            string host = "127.0.0.1";
            int port = 80;
            string result = SocketSendReceive(host, port);
            Console.WriteLine(result);
        }

        // This method requests the home page content for the specified server.
        private static string SocketSendReceive(string server, int port)
        {
            string request = "GET / HTTP/1.1\r\nHost: " + server + "\r\nConnection: Close\r\n\r\n";

            Byte[] bytesSent = Encoding.ASCII.GetBytes(request);
            Byte[] bytesReceived = new Byte[256];
            string page = "";

            // Create a socket connection with the specified server and port.
            using (Socket socket = ConnectSocket(server, port))
            {
                if (socket == null || !socket.Connected)
                    return "Connection failed";

                // Send request to the server.
                socket.Send(bytesSent, bytesSent.Length, 0);

                // Receive the server home page content.
                int bytes = 0;
                page = "Default HTML page on " + server + ":\r\n";

                // The following will block until the page is transmitted.
                do
                {
                    //TODO: RESPONSE RETURNS 302, NEED TO MAKE A NEW REQUEST
                    bytes = socket.Receive(bytesReceived, bytesReceived.Length, 0);
                    page += Encoding.ASCII.GetString(bytesReceived, 0, bytes);
                }
                while (bytes > 0);
            }

            return page;
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
