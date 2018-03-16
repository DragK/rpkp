using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;

namespace rpkp
{
    public class RpkpBot : BotBase<RpkpBot>
    {
        public RpkpBot(IOptions<BotOptions<RpkpBot>> botOptions)
            : base(botOptions) { }

        public override Task HandleUnknownUpdate(Update update) => Task.CompletedTask;

        public override Task HandleFaultedUpdate(Update update, Exception e) => Task.CompletedTask;
    }
}
