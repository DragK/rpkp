using Newtonsoft.Json.Linq;
using rpkp.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace rpkp.Services
{
    public class SearchCommandService
    {
        private readonly string uriTemplate = "https://koleo.pl/pl/connections?query%5Bdate%5D={3}+{2}:00&query%5Bstart_station%5D={0}&query%5Bend_station%5D={1}&query%5Bonly_direct%5D=false&query%5Bonly_purchasable%5D=false";
        private string[] _data { get; set; }

        public SearchCommandService(Update update, string str)
        {
            string[] keyWords = new string[] { "z ", " do ", " dnia ", " o " };

            str = PrepareText(str, keyWords);
            _data = GetArrayOfDataFromMessage(str);

            Console.WriteLine(_data.Length);

            if (_data.Length != 4) throw new FormatException("Zły format. Spróbuj tego: \n**z** __punkt startowy__ **do** __punkty końcowy__ **o** __godzina__ **dnia** __dzień podróży__");
        }

        public async Task<JObject> Main()
        {
            string urlfilledOfData = String.Format(uriTemplate, _data);

            var text = await MakeRequestToKoleoAsync(urlfilledOfData);

            return JObject.Parse(text);
        }

        private string PrepareText(string message, string[] parameters) => message.ToLower().RemoveDiacritics().ReplaceAWordOnSemicolonBasedOnParameters(parameters);

        private string[] GetArrayOfDataFromMessage(string message)
        {
            List<string> listOfData = new List<string>();

            foreach (var el in message.Split(';'))
            {
                if (el.Length < 1)
                {
                    continue;
                }

                if (el.IndexOf(" ") != -1)
                {
                    listOfData.Add(el.Replace(" ", "-"));
                }
                else if (el.IndexOf(":") != -1)
                {
                    listOfData.Add(el.Replace(":", "%3A"));
                }
                else
                {
                    listOfData.Add(el);
                }
            }

            return listOfData.ToArray();
        }

        private async Task<string> MakeRequestToKoleoAsync(string uri)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(uri)
            };

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

            var response = await client.GetAsync(uri);
            var result = await response.Content.ReadAsStringAsync();

            return result.ToString();
        }
    }

    class Connections
    {
        public string start_date { get; set; }
        public string finish_date { get; set; }
        public string travel_time { get; set; }
    }

    class ErrorMessage
    {
        public string message { get; set; }
    }
}
