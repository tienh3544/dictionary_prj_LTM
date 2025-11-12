using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Server
{
    public static class HistoryManager
    {
        public static void Save(string username, string word)
        {
            string path = $"history_{username}.json";
            List<string> history = new List<string>();

            if (File.Exists(path))
                history = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));

            history.Add($"{DateTime.Now}: {word}");
            File.WriteAllText(path, JsonConvert.SerializeObject(history, Newtonsoft.Json.Formatting.Indented));
        }
    }
}
