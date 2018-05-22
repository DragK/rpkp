using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using rpkp.Services;

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

        private Update _update;

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, SearchCommandArgs args)
        {
            _update = update;
            try
            {
                JObject jsonO = await new SearchCommandService(update, args.ArgsInput.ToString()).Main();
                await SendMessage(jsonO);
            }
            catch (FormatException message)
            {
                await Bot.Client.SendTextMessageAsync(
                        _update.Message.Chat.Id,
                        message.Message,
                        ParseMode.Markdown
                    );
            }            

            return UpdateHandlingResult.Handled;
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
}
