using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class InitialForm : Form
    {
        public InitialForm()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIP.Text.Trim();
            string portStr = txtPort.Text.Trim();

            // Validation: Kiểm tra empty
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(portStr))
            {
                MessageBox.Show("Vui lòng điền đầy đủ IP và Port!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIP.Clear();
                txtPort.Clear();
                txtIP.Focus();
                return;
            }

            // Validation: Port phải là số
            int port;
            if (!int.TryParse(portStr, out port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Port phải là số từ 1 đến 65535!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPort.Clear();
                txtPort.Focus();
                return;
            }

            // Validation: IP format
            if (!IPAddress.TryParse(ip, out _))
            {
                MessageBox.Show("IP không hợp lệ! Ví dụ: 127.0.0.1 hoặc 192.168.1.1", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtIP.Clear();
                txtIP.Focus();
                return;
            }

            btnConnect.Enabled = false;
            btnConnect.Text = "Connecting...";

            try
            {
                // Nâng cấp: Thử connect thực sự tới server (TCP) với timeout
                using (TcpClient testClient = new TcpClient())
                {
                    var connectTask = testClient.ConnectAsync(ip, port);
                    var timeoutTask = Task.Delay(5000);  // Timeout 5s
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        throw new TimeoutException("Không thể kết nối tới server. Kiểm tra IP/Port.");
                    }
                    await connectTask;  // Đảm bảo connect thành công
                }

                // Thành công: Mở LoginForm
                LoginForm loginForm = new LoginForm(ip, port);
                loginForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}. Vui lòng kiểm tra và nhập lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtIP.Clear();
                txtPort.Clear();
                txtIP.Focus();  // Quay lại nhập
            }
            finally
            {
                btnConnect.Enabled = true;
                btnConnect.Text = "Kết nối";
            }
        }
    }
}