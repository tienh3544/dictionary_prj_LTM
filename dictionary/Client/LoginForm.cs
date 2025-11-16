using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class LoginForm : Form
    {
        private string ip;
        private int port;
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private NetworkStream stream;

        public LoginForm(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                client.Connect(ip, port);
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kết nối thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng điền đầy đủ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                writer.WriteLine($"LOGIN|{username}|{password}");
                string response = reader.ReadLine();
                string[] parts = response.Split('|');
                if (parts[0] == "SUCCESS")
                {
                    int userId = int.Parse(parts[1]);
                    string user = parts[2];
                    ClientForm clientForm = new ClientForm(ip, port, userId, user, client, reader, writer, stream);
                    clientForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Sai username hoặc password!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGoToRegister_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm(ip, port, client, reader, writer, stream);
            registerForm.Show();
            this.Hide();
        }
    }
}