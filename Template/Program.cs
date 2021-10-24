using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Template.Services;
using Template.Models;
using Octokit;

namespace Template
{
    class Program
    {
        static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

            
        public async Task MainAsync()
        {
            // You should dispose a service provider created using ASP.NET
            // when you are finished using it, at the end of your app's lifetime.
            // If you use another dependency injection framework, you should inspect
            // its documentation for the best way to do this.

            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();

                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                services.GetRequiredService<DiscordSocketConfig>().AlwaysDownloadUsers = true;
                // Tokens should be considered secret data and never hard-coded.
                // We can read from the environment variable to avoid hardcoding.
                await client.LoginAsync(TokenType.Bot, SettingsHandler.DiscordToken);
                await client.SetStatusAsync(UserStatus.Online);
                await client.SetGameAsync($"v{SettingsHandler.Version}", "", ActivityType.Watching);
                await client.StartAsync();
                // Here we initialize the logic required to register our commands.
                await services.GetRequiredService<CommandHandler>().InitializeAsync();
                //DiscordModel.UpdateModel(client);
                await Task.Delay(Timeout.Infinite);
            }
        }

        /// <summary>
        /// Launching start logging
        /// </summary>
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Generate service configuration
        /// </summary>
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<DiscordSocketConfig>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
    }
}