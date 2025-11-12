using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Server
{
    public static class AccountManager
    {
        private static readonly string path = "accounts.json";
        private static List<Account> accounts = new List<Account>();

        static AccountManager()
        {
            if (File.Exists(path))
                accounts = JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(path));
        }

        public static bool Register(string user, string pass)
        {
            if (accounts.Exists(a => a.Username == user)) return false;
            accounts.Add(new Account { Username = user, Password = pass });
            File.WriteAllText(path, JsonConvert.SerializeObject(accounts, Newtonsoft.Json.Formatting.Indented));
            return true;
        }

        public static bool Login(string user, string pass)
        {
            return accounts.Exists(a => a.Username == user && a.Password == pass);
        }
    }

    public class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
