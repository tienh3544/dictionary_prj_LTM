using System;

namespace Server
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.WriteLine("Starting dictionary server...");
            ServerSocket server = new ServerSocket(1234);
            server.Start();
        }
    }
}
