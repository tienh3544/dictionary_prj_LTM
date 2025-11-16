using System;
using System.Windows.Forms;

namespace Client
{
    public partial class HistoryForm : Form
    {
        public HistoryForm(string historyData)
        {
            InitializeComponent();
            LoadHistory(historyData);
        }

        private void LoadHistory(string historyData)
        {
            lstHistory.Items.Clear();
            if (!string.IsNullOrEmpty(historyData))
            {
                string[] entries = historyData.Split(';');
                foreach (string entry in entries)
                {
                    lstHistory.Items.Add(entry);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}