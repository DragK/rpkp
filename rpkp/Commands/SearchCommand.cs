using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace rpkp
{
    public class SearchCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }
        public string ArgsInput { get; set; }
    }
    public class SearchCommand : CommandBase<SearchCommandArgs>
    {
        public SearchCommand() : base(name: "szukaj") { }

        private readonly string uriTemplate = "https://koleo.pl/pl/connections?query%5Bdate%5D={3}+{2}:00&query%5Bstart_station%5D={0}&query%5Bend_station%5D={1}&query%5Bonly_direct%5D=false&query%5Bonly_purchasable%5D=false";
        private Update _update;

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, SearchCommandArgs args)
        {
            _update = update;

            string str = PrepareText(args.ArgsInput.ToString());
            string[] keyWords = new string[] { "z ", " do ", " dnia ", " o " };

            str = ReplaceWordsASemicolonBasedOnParameters(str, keyWords);
            string[] newT = GetArrayOfDataFromMessage(str);

            string urlfilledOfData = String.Format(uriTemplate, newT);

            var text = await MakeRequestToKoleoAsync(urlfilledOfData);
            JObject jsonO = JObject.Parse(text);

            await SendMessage(jsonO);

            return UpdateHandlingResult.Handled;
        }

        private string PrepareText(string message)
        {
            message = message.ToLower();
            message = RemoveDiacritics(message).ToString();

            return message;
        }

        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (string.Compare(c.ToString(), "ł") == 0)
                {
                    stringBuilder.Append("l");
                }
                else if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private string ReplaceWordsASemicolonBasedOnParameters(string text, string[] parameters)
        {
            foreach (string word in parameters)
            {
                text = text.Replace(word, ";");
            }

            return text;
        }

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

        public async Task<string> MakeRequestToKoleoAsync(string uri)
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

        private async Task<Boolean> SendMessage(JObject jsonO)
        {
            if (jsonO["error_messages"] != null)
            {
                IList<JToken> errorList = jsonO["error_messages"].Children().ToList();
                IList<ErrorMessage> errors = new List<ErrorMessage>();
                foreach (JToken error in errorList)
                {
                    ErrorMessage errorObject = error.ToObject<ErrorMessage>();
                    Console.WriteLine(errorObject.message);
                    await Bot.Client.SendTextMessageAsync(
                        _update.Message.Chat.Id,
                        errorObject.message.ToString()
                    );
                }

                return true;
            }

            IList<JToken> results = jsonO["connections"].Children().ToList();
            IList<Connections> searchResults = new List<Connections>();
            foreach (JToken result in results)
            {
                Connections searchResult = result.ToObject<Connections>();
                searchResults.Add(searchResult);

                string message = string.Concat("**Start o** ", searchResult.start_date, "\n**Koniec o** ", searchResult.finish_date, "\n**Czas podróży:** ", searchResult.travel_time, " minut");
                await Bot.Client.SendTextMessageAsync(
                    _update.Message.Chat.Id,
                    message,
                    ParseMode.Markdown
                );
            }
           
            return true;
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
        public string message {get; set; }
    }
}
