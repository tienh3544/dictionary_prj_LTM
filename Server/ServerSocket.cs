using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class ServerSocket
    {
        private TcpListener listener;
        private bool isRunning = false;

        public ServerSocket(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            listener.Start();
            isRunning = true;
            Console.WriteLine("Server started, waiting for clients...");

            while (isRunning)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected!");
                ClientHandler handler = new ClientHandler(client);
                Thread clientThread = new Thread(new ThreadStart(handler.Process));
                clientThread.Start();
            }
        }
    }
}
