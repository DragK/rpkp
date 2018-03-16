using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Framework.Abstractions;

namespace rpkp
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;

        public Startup(IHostingEnvironment env)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddRouting()
                .AddTelegramBot<RpkpBot>(_configuration.GetSection("Rpkp"))
                .AddUpdateHandler<StartCommand>()
                .AddUpdateHandler<SearchCommand>()
                .Configure();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var routes = new MyRoutes(app);

            var source = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                Console.WriteLine("## Press Enter to stop bot manager...");
                Console.ReadLine();
                source.Cancel();
            });
            Task.Factory.StartNew(async () => {
                var botManager = app.ApplicationServices.GetRequiredService<IBotManager<RpkpBot>>();
                while (!source.IsCancellationRequested)
                {
                    await Task.Delay(3_000);
                    await botManager.GetAndHandleNewUpdatesAsync();
                }
                Console.WriteLine("## Bot manager stopped.");
                Environment.Exit(0);
            }).ContinueWith(t => {
                if (t.IsFaulted) throw t.Exception;
            });
        }
    }
}
