using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server
{
    public class ClientHandler
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private string username;

        public ClientHandler(TcpClient client)
        {
            this.client = client;
            var stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        }

        public async void Process()
        {
            try
            {
                while (true)
                {
                    string data = await reader.ReadLineAsync();
                    if (data == null) break;

                    JObject request = JObject.Parse(data);
                    string type = request["type"]?.ToString();

                    switch (type)
                    {
                        case "register":
                            HandleRegister(request);
                            break;
                        case "login":
                            HandleLogin(request);
                            break;
                        case "search":
                            await HandleSearch(request);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client disconnected: {ex.Message}");
            }
        }

        private void HandleRegister(JObject req)
        {
            string user = req["username"].ToString();
            string pass = req["password"].ToString();

            if (AccountManager.Register(user, pass))
                writer.WriteLine("{\"status\":\"ok\", \"message\":\"Register success\"}");
            else
                writer.WriteLine("{\"status\":\"fail\", \"message\":\"User already exists\"}");
        }

        private void HandleLogin(JObject req)
        {
            string user = req["username"].ToString();
            string pass = req["password"].ToString();

            if (AccountManager.Login(user, pass))
            {
                username = user;
                writer.WriteLine("{\"status\":\"ok\", \"message\":\"Login success\"}");
            }
            else
                writer.WriteLine("{\"status\":\"fail\", \"message\":\"Invalid credentials\"}");
        }

        private async Task HandleSearch(JObject req)
        {
            string word = req["word"].ToString();
            string result = await DictionaryAPI.Lookup(word);
            HistoryManager.Save(username, word);
            writer.WriteLine(result);
        }
    }
}
