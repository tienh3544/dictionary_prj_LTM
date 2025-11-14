using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Server
{
    public partial class ServerForm : Form
    {
        TcpListener listener;
        bool running = false;

        public ServerForm()
        {
            InitializeComponent();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (running) return;

            running = true;
            listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();

            AddLog("Server started on port 5000");
            lblStatus.Text = "Running";

            while (running)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                AddLog("Client connected");
                _ = HandleClient(client);
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            try
            {
                while (true)
                {
                    string word = await reader.ReadLineAsync();
                    if (word == null) break;

                    AddLog($"Client request: {word}");

                    string result = await Lookup(word);
                    await writer.WriteLineAsync(result);
                }
            }
            catch
            {
                AddLog("Client disconnected");
            }
        }

        private async Task<string> Lookup(string word)
        {
            try
            {
                HttpClient http = new HttpClient();
                string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
                string json = await http.GetStringAsync(url);

                var data = JArray.Parse(json);
                return data[0]["meanings"][0]["definitions"][0]["definition"].ToString();
            }
            catch
            {
                return "Word not found!";
            }
        }

        private void AddLog(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddLog(msg)));
                return;
            }
            lstLog.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " - " + msg);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            running = false;
            listener?.Stop();
            lblStatus.Text = "Stopped";
            AddLog("Server stopped");
        }
    }
}
