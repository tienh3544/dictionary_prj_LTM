using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Client
{
    public partial class HistoryForm : Form
    {
        private string username;
        private string password;
        private ListBox lstHistory;


        public HistoryForm(string user)
        {
            InitializeComponent();
            username = user;
            LoadHistory();
        }

        private void LoadHistory()
        {
            string path = $"history_{username}.json";
            if (!File.Exists(path))
            {
                lstHistory.Items.Add("Chưa có lịch sử tra cứu.");
                return;
            }

            var data = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
            foreach (string item in data)
                lstHistory.Items.Add(item);
        }
    }
}
