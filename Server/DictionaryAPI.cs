using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Server
{
    public static class DictionaryAPI
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> Lookup(string word)
        {
            try
            {
                string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
                var json = await client.GetStringAsync(url);
                JArray data = JArray.Parse(json);

                string phonetic = data[0]["phonetic"]?.ToString() ?? "";
                string meaning = data[0]["meanings"]?[0]?["definitions"]?[0]?["definition"]?.ToString() ?? "";

                return $"{{\"word\":\"{word}\",\"phonetic\":\"{phonetic}\",\"definition\":\"{meaning}\"}}";
            }
            catch
            {
                return $"{{\"word\":\"{word}\",\"definition\":\"Not found\"}}";
            }
        }
    }
}
