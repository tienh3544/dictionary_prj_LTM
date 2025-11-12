using System;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Client
{
    public partial class MainForm : Form
    {
        private string username;

        public MainForm(string user)
        {
            InitializeComponent();
            username = user;
            lblWelcome.Text = $"Xin chào, {username}!";
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string word = txtWord.Text.Trim();
            if (string.IsNullOrEmpty(word)) return;

            var request = new JObject
            {
                ["type"] = "search",
                ["username"] = username,
                ["word"] = word
            };

            Program.clientSocket.Send(request.ToString());
            string response = Program.clientSocket.Receive();

            var resObj = JObject.Parse(response);
            txtResult.Text = $"{resObj["word"]}\n\n{resObj["phonetic"]}\n\n{resObj["definition"]}";
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            HistoryForm form = new HistoryForm(username);
            form.ShowDialog();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Program.clientSocket.Close();
            Application.Restart();
        }
    }
}
