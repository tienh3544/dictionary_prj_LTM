using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Client
{
    public class ClientSocket
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;

        public bool Connect(string host, int port)
        {
            try
            {
                client = new TcpClient(host, port);
                var stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Send(string json)
        {
            writer.WriteLine(json);
        }

        public string Receive()
        {
            return reader.ReadLine();
        }

        public void Close()
        {
            client?.Close();
        }
    }
}
