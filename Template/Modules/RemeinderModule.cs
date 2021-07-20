using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Timers;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace Template.Modules
{
    public class RemeinderModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        public RemeinderModule(DiscordSocketClient client) => _client = client;

        [Command("enable_remeinder")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task EnableRemeinder()
        {
            double interval = 5 * 1000; // milliseconds to one hour

            var nowTime = DateTime.Now.Minute;
            var toWait = Math.Abs(nowTime - 32);

            await ReplyAsync($"Таймер запуститься через {toWait} минут, не выключайте бота");
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(toWait * 1000 * 60);

                System.Timers.Timer checkForTime = new System.Timers.Timer(interval);
                checkForTime.Elapsed += new ElapsedEventHandler(CheckForTime_Elapsed);
                checkForTime.Enabled = true;
                SendReminderMessage();
            }).Start();
        }

        private async void SendReminderMessage()
        {
            await _client.GetGuild(863151265939456043).GetTextChannel(863427166662557696).SendMessageAsync("Таймер запущен!");
        }

        private async void CheckForTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            /*DateTime.Now.Hour == 18 && (DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)*/
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                await _client.GetGuild(863151265939456043).GetTextChannel(863427166662557696).SendMessageAsync("Вминаниме!");
            }
        }
    }
}
