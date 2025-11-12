using System;
using System.Windows.Forms;

namespace Client
{
    internal static class Program
    {
        public static ClientSocket clientSocket;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            clientSocket = new ClientSocket();
            bool connected = clientSocket.Connect("192.168.1.14", 1234);
            if (!connected)
            {
                MessageBox.Show("Không thể kết nối tới server!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}
