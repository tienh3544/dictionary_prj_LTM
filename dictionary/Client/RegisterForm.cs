using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class RegisterForm : Form
    {
        private string ip;
        private int port;
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private NetworkStream stream;

        public RegisterForm(string ip, int port, TcpClient client, StreamReader reader, StreamWriter writer, NetworkStream stream)
        {
            this.ip = ip;
            this.port = port;
            this.client = client;
            this.reader = reader;
            this.writer = writer;
            this.stream = stream;
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Vui lòng điền đầy đủ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show("Password không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                writer.WriteLine($"REGISTER|{username}|{password}");
                string response = reader.ReadLine();
                string[] parts = response.Split('|');
                if (parts[0] == "SUCCESS")
                {
                    MessageBox.Show("Đăng ký thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoginForm loginForm = new LoginForm(ip, port);
                    loginForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Username đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBackToLogin_Click(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm(ip, port);
            loginForm.Show();
            this.Hide();
        }
    }
}