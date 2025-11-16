using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class ClientForm : Form
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private NetworkStream stream;
        private int userId;
        private string username;
        private CancellationTokenSource cts;  // Cho timeout và cancel

        public ClientForm(string ip, int port, int userId, string username, TcpClient client, StreamReader reader, StreamWriter writer, NetworkStream stream)
        {
            this.userId = userId;
            this.username = username;
            this.client = client;
            this.reader = reader;
            this.writer = writer;
            this.stream = stream;
            cts = new CancellationTokenSource();
            InitializeComponent();
            lblStatus.Text = "Status: Connected";
            txtResult.Text = $"Kết nối thành công! Chào {username}, nhập từ để tra cứu.";
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            if (client == null || !client.Connected)
            {
                MessageBox.Show("Chưa kết nối đến server!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string word = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(word))
            {
                MessageBox.Show("Vui lòng nhập từ cần tra!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnSearch.Enabled = false;
                btnSearch.Text = "Searching...";
                progressBar.Visible = true;

                writer.WriteLine($"SEARCH|{userId}|{word}");
                // Sửa: Thay WaitAsync bằng Task.WhenAny + Task.Delay cho timeout (tương thích .NET Framework)
                var readTask = Task.Run(() => reader.ReadLine(), cts.Token);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), cts.Token);
                var completedTask = await Task.WhenAny(readTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Timeout: Server không phản hồi");
                }
                string response = await readTask;
                string[] parts = response.Split('|');
                if (parts[0] == "RESULT")
                {
                    DisplayResult(word, string.Join("|", parts.Skip(1)));
                }
                else
                {
                    txtResult.Text = "Lỗi tra cứu";
                }
            }
            catch (TimeoutException ex)
            {
                txtResult.Text = ex.Message;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtResult.Text = $"Lỗi: {ex.Message}";
            }
            finally
            {
                btnSearch.Enabled = true;
                btnSearch.Text = "Tra cứu";
                progressBar.Visible = false;
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

        private async void btnPronounce_Click(object sender, EventArgs e)
        {
            string word = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(word))
            {
                try
                {
                    progressBar.Visible = true;
                    writer.WriteLine($"PRONOUNCE|{word}");
                    // Sửa: Timeout tương tự
                    var readTask = Task.Run(() => reader.ReadLine(), cts.Token);
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                    var completedTask = await Task.WhenAny(readTask, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        throw new TimeoutException("Timeout phát âm");
                    }
                    await readTask;  // Đợi response
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi phát âm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    progressBar.Visible = false;
                }
            }
            else
            {
                MessageBox.Show("Nhập từ để phát âm!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnHistory_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar.Visible = true;
                writer.WriteLine($"HISTORY|{userId}");
                // Sửa: Timeout tương tự
                var readTask = Task.Run(() => reader.ReadLine(), cts.Token);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), cts.Token);
                var completedTask = await Task.WhenAny(readTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Timeout tải lịch sử");
                }
                string response = await readTask;
                string[] parts = response.Split('|');
                if (parts[0] == "HISTORY")
                {
                    HistoryForm historyForm = new HistoryForm(string.Join("|", parts.Skip(1)));
                    historyForm.Show();
                }
                else
                {
                    MessageBox.Show("Lỗi tải lịch sử!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                cts.Cancel();
                writer?.WriteLine("LOGOUT");
                writer?.Close();
                reader?.Close();
                stream?.Close();
                client?.Close();
                lblStatus.Text = "Status: Logged Out";
                txtResult.Text = "Đã đăng xuất.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng xuất: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoginForm loginForm = new LoginForm("127.0.0.1", 5000);
            loginForm.Show();
            this.Hide();
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
                txtResult.Text = "Đã ngắt kết nối. Nhấn Connect để kết nối lại.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đóng kết nối: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tính năng reconnect chưa triển khai. Khởi động lại app.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                cts.Cancel();
                writer?.Close();
                reader?.Close();
                stream?.Close();
                client?.Close();
            }
            catch { }
        }

        private void tableLayoutPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void progressBar_Click(object sender, EventArgs e)
        {

        }
    }
}
