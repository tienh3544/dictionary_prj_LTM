using System;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Client
{
    public partial class RegisterForm : Form
    {
        private void btnRegister_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();

            var request = new JObject
            {
                ["type"] = "register",
                ["username"] = user,
                ["password"] = pass
            };

            Program.clientSocket.Send(request.ToString());
            string response = Program.clientSocket.Receive();
            var resObj = JObject.Parse(response);

            MessageBox.Show(resObj["message"].ToString(),
                resObj["status"].ToString() == "ok" ? "Thành công" : "Thất bại",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            if (resObj["status"].ToString() == "ok")
            {
                new LoginForm().Show();
                this.Hide();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            new LoginForm().Show();
            this.Hide();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void lblUsername_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
