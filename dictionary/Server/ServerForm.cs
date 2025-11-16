using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Server
{
    public partial class ServerForm : Form
    {
        private TcpListener listener;
        private bool running = false;
        private HttpClient httpClient;
        private SpeechSynthesizer synthesizer;
        private string connectionString = "Server=localhost;Database=DictionaryDB;Integrated Security=True;";
        private int clientCount = 0;

        public ServerForm()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            synthesizer = new SpeechSynthesizer();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (running) return;

            try
            {
                running = true;
                int port = 5000;
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                string ip = GetLocalIPAddress();
                AddLog($"IP: {ip}");
                AddLog($"Port: {port}");
                lblStatus.Text = "Đang chạy...";
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                AddLog("Máy chủ đã khởi động");

                Task.Run(async () => await ListenForClients());
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi: {ex.Message}");
                running = false;
            }
        }

        private async Task ListenForClients()
        {
            while (running)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Interlocked.Increment(ref clientCount);
                    AddLog($"Client đã kết nối. Tổng client: {clientCount}");
                    _ = Task.Run(async () => await HandleClient(client));
                }
                catch (Exception ex)
                {
                    if (running) AddLog($"Lỗi accept client: {ex.Message}");
                }
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            try
            {
                while (client.Connected)
                {
                    string command = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(command)) break;

                    AddLog($"Nhận lệnh: {command}");
                    string response = await ProcessCommand(command);
                    await writer.WriteLineAsync(response);
                    AddLog($"Đã gửi phản hồi");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi xử lý client: {ex.Message}");
            }
            finally
            {
                try
                {
                    reader?.Close();
                    writer?.Close();
                    stream?.Close();
                    client?.Close();
                }
                catch { }
                Interlocked.Decrement(ref clientCount);
                AddLog($"Client ngắt kết nối. Tổng client: {clientCount}");
            }
        }

        private async Task<string> ProcessCommand(string command)
        {
            string[] parts = command.Split('|');
            string action = parts[0];

            switch (action)
            {
                case "LOGIN":
                    if (parts.Length >= 3)
                    {
                        string username = parts[1];
                        string password = parts[2];
                        string hashedPassword = HashPassword(password);
                        int userId = ValidateLogin(username, hashedPassword);
                        if (userId != -1)
                        {
                            return $"SUCCESS|{userId}|{username}";
                        }
                        else
                        {
                            return "FAIL|Invalid credentials";
                        }
                    }
                    break;
                case "REGISTER":
                    if (parts.Length >= 3)
                    {
                        string username = parts[1];
                        string password = parts[2];
                        string hashedPassword = HashPassword(password);
                        if (RegisterUser(username, hashedPassword))
                        {
                            return "SUCCESS|Registered";
                        }
                        else
                        {
                            return "FAIL|Username exists";
                        }
                    }
                    break;
                case "SEARCH":
                    if (parts.Length >= 3)
                    {
                        int userId = int.Parse(parts[1]);
                        string word = parts[2];
                        string result = await LookupWord(word);
                        SaveToHistory(userId, word);
                        return $"RESULT|{result}";
                    }
                    break;
                case "HISTORY":
                    if (parts.Length >= 2)
                    {
                        int userId = int.Parse(parts[1]);
                        string history = GetHistory(userId);
                        return $"HISTORY|{history}";
                    }
                    break;
                case "PRONOUNCE":
                    if (parts.Length >= 2)
                    {
                        string word = parts[1];
                        synthesizer.Speak(word); // Phát âm trên server
                        return "PRONOUNCED";
                    }
                    break;
            }
            return "ERROR|Invalid command";
        }

        private async Task<string> LookupWord(string word)
        {
            try
            {
                string nghia = await TranslateWithGoogle(word);
                string apiResult = await GetDictionaryDefinition(word);
                if (apiResult != "not_found")
                {
                    string[] parts = apiResult.Split('|');
                    string phonetic = parts.Length > 0 ? parts[0] : "";
                    string definition = parts.Length > 1 ? parts[1] : "";
                    string pos = parts.Length > 2 ? parts[2] : "danh từ";
                    string mota = string.IsNullOrEmpty(definition) ? "" : await TranslateWithGoogle(definition);
                    return $"{phonetic}|{nghia}|{mota}|{pos}";
                }
                else
                {
                    return $"|{nghia}||danh từ";
                }
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi API: {ex.Message}");
                return SimpleLookup(word);
            }
        }

        private async Task<string> GetDictionaryDefinition(string word)
        {
            try
            {
                string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{Uri.EscapeDataString(word.ToLower())}";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return ParseDictionaryResponse(json, word);
                }
                else
                {
                    return "not_found";
                }
            }
            catch
            {
                return "not_found";
            }
        }

        private string ParseDictionaryResponse(string json, string word)
        {
            try
            {
                JArray data = JArray.Parse(json);
                JObject firstEntry = data[0] as JObject;
                string phonetic = firstEntry["phonetic"]?.ToString() ?? "";
                JArray meanings = firstEntry["meanings"] as JArray;
                string definition = "";
                string pos = "danh từ";
                if (meanings != null && meanings.Count > 0)
                {
                    JObject firstMeaning = meanings[0] as JObject;
                    string partOfSpeech = firstMeaning["partOfSpeech"]?.ToString() ?? "";
                    pos = TranslatePartOfSpeech(partOfSpeech);
                    JArray definitions = firstMeaning["definitions"] as JArray;
                    if (definitions != null && definitions.Count > 0)
                    {
                        JObject firstDef = definitions[0] as JObject;
                        definition = firstDef["definition"]?.ToString() ?? "";
                    }
                }
                return $"{phonetic}|{definition}|{pos}";
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi parse dictionary: {ex.Message}");
            }
            return "not_found";
        }

        private async Task<string> TranslateWithGoogle(string text)
        {
            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=vi&dt=t&q={Uri.EscapeDataString(text)}";
                string json = await httpClient.GetStringAsync(url);
                JArray data = JArray.Parse(json);
                string translated = data[0][0][0]?.ToString() ?? text;
                return translated;
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi Google Translate: {ex.Message}");
                return text;
            }
        }

        private string TranslatePartOfSpeech(string partOfSpeech)
        {
            var posMap = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["noun"] = "danh từ",
                ["verb"] = "động từ",
                ["adjective"] = "tính từ",
                ["adverb"] = "trạng từ",
                ["pronoun"] = "đại từ",
                ["preposition"] = "giới từ",
                ["conjunction"] = "liên từ",
                ["interjection"] = "thán từ"
            };
            return posMap.TryGetValue(partOfSpeech, out string translated) ? translated : partOfSpeech;
        }

        private string SimpleLookup(string word)
        {
            var dictionary = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["hello"] = "|Xin chào|Lời chào|thán từ",
                ["hi"] = "|Xin chào|Lời chào thân mật|thán từ",
                ["computer"] = "|Máy tính|Thiết bị điện tử|danh từ",
                ["book"] = "|Sách|Tập hợp trang giấy|danh từ",
                ["water"] = "|Nước|Chất lỏng|danh từ",
                ["house"] = "|Nhà|Nơi ở|danh từ",
                ["love"] = "|Yêu|Tình cảm|danh từ",
                ["friend"] = "|Bạn|Người thân|danh từ",
                ["school"] = "|Trường học|Nơi học tập|danh từ",
                ["work"] = "|Làm việc|Hoạt động|động từ",
                ["apple"] = "|Quả táo|Trái cây|danh từ",
                ["dog"] = "|Con chó|Động vật|danh từ",
                ["cat"] = "|Con mèo|Động vật|danh từ",
                ["sun"] = "|Mặt trời|Ngôi sao|danh từ",
                ["moon"] = "|Mặt trăng|Vệ tinh|danh từ",
                ["tree"] = "|Cây|Thực vật|danh từ",
                ["car"] = "|Xe hơi|Phương tiện|danh từ",
                ["phone"] = "|Điện thoại|Thiết bị|danh từ",
                ["music"] = "|Âm nhạc|Nghệ thuật|danh từ",
                ["time"] = "|Thời gian|Khái niệm|danh từ"
            };
            return dictionary.ContainsKey(word.ToLower()) ? dictionary[word.ToLower()] : "|||Không tìm thấy|Từ không có trong từ điển";
        }

        private int ValidateLogin(string username, string hashedPassword)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id FROM Users WHERE username = @username AND password_hash = @password";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        private bool RegisterUser(string username, string hashedPassword)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE username = @username";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@username", username);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0) return false;
                }
                string insertQuery = "INSERT INTO Users (username, password_hash) VALUES (@username, @password)";
                using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@username", username);
                    insertCmd.Parameters.AddWithValue("@password", hashedPassword);
                    insertCmd.ExecuteNonQuery();
                }
                return true;
            }
        }

        private void SaveToHistory(int userId, string word)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO History (user_id, word, timestamp) VALUES (@userId, @word, @timestamp)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@word", word);
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GetHistory(int userId)
        {
            string history = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT word, timestamp FROM History WHERE user_id = @userId ORDER BY timestamp DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string word = reader["word"].ToString();
                            string timestamp = reader["timestamp"].ToString();
                            history += $"{timestamp} - {word};";
                        }
                    }
                }
            }
            return history.TrimEnd(';');
        }

        private string HashPassword(string password)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "127.0.0.1";
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi lấy IP: {ex.Message}");
                return "127.0.0.1";
            }
        }

        private void AddLog(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddLog), msg);
                return;
            }
            lstLog.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " - " + msg);
            lstLog.SelectedIndex = lstLog.Items.Count - 1;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            running = false;
            listener?.Stop();
            httpClient?.Dispose();
            synthesizer?.Dispose();
            lblStatus.Text = "Đã dừng";
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            AddLog("Máy chủ đã dừng");
        }
    }
}
