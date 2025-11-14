using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class ClientForm : Form
    {
        TcpClient client;
        StreamReader reader;
        StreamWriter writer;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient(txtIP.Text, int.Parse(txtPort.Text));
                var stream = client.GetStream();

                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                lblStatus.Text = "Connected";
            }
            catch
            {
                lblStatus.Text = "Failed to connect";
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (client == null || !client.Connected)
            {
                MessageBox.Show("Not connected!");
                return;
            }

            string word = txtSearch.Text.Trim();
            if (word == "")
            {
                MessageBox.Show("Please enter a word!");
                return;
            }

            writer.WriteLine(word);
            string result = reader.ReadLine();

            txtResult.Text = result;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                client?.Close();
                lblStatus.Text = "Disconnected";
            }
            catch { }
        }
    }
}
