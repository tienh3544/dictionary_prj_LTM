using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class ClientForm : Form
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private NetworkStream stream;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                client.Connect(txtIP.Text, int.Parse(txtPort.Text));
                stream = client.GetStream();

                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                lblStatus.Text = "Status: Connected";
                btnConnect.Enabled = false;
                btnClose.Enabled = true;

                txtResult.Text = "Kết nối thành công! Nhập từ để tra cứu.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Status: Failed";
                MessageBox.Show($"Kết nối thất bại: {ex.Message}", "Lỗi",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (client == null || !client.Connected)
            {
                MessageBox.Show("Chưa kết nối đến server!", "Lỗi",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string word = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(word))
            {
                MessageBox.Show("Vui lòng nhập từ cần tra!", "Cảnh báo",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnSearch.Enabled = false;
                btnSearch.Text = "Searching...";

                writer.WriteLine(word);
                string result = reader.ReadLine();

                if (string.IsNullOrEmpty(result))
                {
                    txtResult.Text = "Không nhận được phản hồi từ server";
                    return;
                }

                DisplayResult(word, result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtResult.Text = $"Lỗi: {ex.Message}";
            }
            finally
            {
                btnSearch.Enabled = true;
                btnSearch.Text = "Search";
            }
        }

        private void DisplayResult(string word, string result)
        {
            string[] parts = result.Split('|');

            StringBuilder sb = new StringBuilder();

            if (parts.Length >= 4)
            {
                string phonetic = parts[0];
                string nghia = parts[1];
                string mota = parts[2];
                string pos = parts[3];

                sb.AppendLine($"Từ: {word}");
                sb.AppendLine($"Phát âm: {phonetic}");
                sb.AppendLine($"Nghĩa: {nghia}");
                sb.AppendLine($"Mô tả: {mota}");
                sb.AppendLine($"Từ loại: {pos}");
            }
            else
            {
                sb.AppendLine($"Lỗi: {result}");
            }

            txtResult.Text = sb.ToString();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                writer?.Close();
                reader?.Close();
                stream?.Close();
                client?.Close();

                lblStatus.Text = "Status: Disconnected";
                btnConnect.Enabled = true;
                btnClose.Enabled = false;
                txtResult.Text = "Đã ngắt kết nối";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đóng kết nối: {ex.Message}", "Lỗi",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnSearch_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                writer?.Close();
                reader?.Close();
                stream?.Close();
                client?.Close();
            }
            catch
            {
                // Ignore
            }
        }
    }
}