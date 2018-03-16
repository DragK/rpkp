using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace rpkp
{
    public class StartCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }
        public string ArgsInput { get; set; }
    }
    public class StartCommand : CommandBase<StartCommandArgs>
    {
        public StartCommand() : base(name: "start") { }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, StartCommandArgs args)
        {
            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id, 
                $"Cześć {update.Message.Chat.FirstName}!"
            );

            return UpdateHandlingResult.Handled;
        }
    }
}
