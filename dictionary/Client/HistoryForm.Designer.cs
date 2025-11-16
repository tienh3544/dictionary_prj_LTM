namespace Client
{
    partial class HistoryForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox lstHistory;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblTitle;

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
            this.lstHistory = new System.Windows.Forms.ListBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(140, 15);
            this.lblTitle.Text = "LỊCH SỬ";

            this.lstHistory.FormattingEnabled = true;
            this.lstHistory.ItemHeight = 15;
            this.lstHistory.Location = new System.Drawing.Point(20, 50);
            this.lstHistory.Size = new System.Drawing.Size(360, 200);

            this.btnClose.Location = new System.Drawing.Point(140, 260);
            this.btnClose.Size = new System.Drawing.Size(120, 35);
            this.btnClose.Text = "Đóng";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.ClientSize = new System.Drawing.Size(400, 310);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lstHistory);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Client";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
