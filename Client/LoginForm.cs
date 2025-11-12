using System;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Client
{
    public partial class LoginForm : Form
    {

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();

            var request = new JObject
            {
                ["type"] = "login",
                ["username"] = user,
                ["password"] = pass
            };

            Program.clientSocket.Send(request.ToString());
            string response = Program.clientSocket.Receive();
            var resObj = JObject.Parse(response);

            if (resObj["status"].ToString() == "ok")
            {
                MainForm main = new MainForm(user);
                main.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show(resObj["message"].ToString(), "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
            this.Hide();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}
