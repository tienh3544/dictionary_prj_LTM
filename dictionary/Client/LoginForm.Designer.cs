namespace Client
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnGoToRegister;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnGoToRegister = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(100, 20);
            this.lblTitle.Text = "ĐĂNG NHẬP";

            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(30, 70);
            this.lblUsername.Text = "Tài khoản:";

            this.txtUsername.Location = new System.Drawing.Point(150, 67);
            this.txtUsername.Size = new System.Drawing.Size(180, 23);

            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(30, 110);
            this.lblPassword.Text = "Mật khẩu:";

            this.txtPassword.Location = new System.Drawing.Point(150, 107);
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(180, 23);

            this.btnLogin.Location = new System.Drawing.Point(210, 160);
            this.btnLogin.Size = new System.Drawing.Size(120, 35);
            this.btnLogin.Text = "Đăng nhập";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

            this.btnGoToRegister.Location = new System.Drawing.Point(50, 160);
            this.btnGoToRegister.Size = new System.Drawing.Size(120, 35);
            this.btnGoToRegister.Text = "Đăng ký";
            this.btnGoToRegister.Click += new System.EventHandler(this.btnGoToRegister_Click);

            this.ClientSize = new System.Drawing.Size(380, 220);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.btnGoToRegister);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Client";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
