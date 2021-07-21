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
    /// <summary>
    /// This module is used for Lessons reminder command
    /// </summary>
    [Summary("This module is used for Lessons reminder command")]
    public class RemeinderModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        public RemeinderModule(DiscordSocketClient client) => _client = client;

        /// <summary>
        /// Enable reminder command is used to turn on timer
        /// </summary>
        /// <returns></returns>
        [Summary("Enable reminder command is used to turn on timer")]
        [Command("enable_reminder")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task EnableReminder()
        {
            double interval = 5 * 1000; // milliseconds to 5 sec

            var nowTime = DateTime.Now.Minute;
            var toWait = Math.Abs(nowTime - 55);

            await ReplyAsync($"Таймер запуститься через {toWait} минут, не выключайте бота");

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(toWait * 1000 * 60); // Wait until its hh:55:ss and then turns on the checker

                // Calls the CheckForTime every `interval` time
                System.Timers.Timer checkForTime = new System.Timers.Timer(interval);
                checkForTime.Elapsed += new ElapsedEventHandler(CheckForTime_Elapsed);
                checkForTime.Enabled = true;
                SendReminderMessage();
            }).Start();

            // Sends message when timer is ready and is turned on
            void SendReminderMessage()
            {
                _client.GetGuild(863151265939456043).GetTextChannel(863427166662557696).SendMessageAsync("Таймер запущен!");
            }

            // Calls every `interval` time, checks if its the correct time and date to send a reminder message in #news channel
            void CheckForTime_Elapsed(object sender, ElapsedEventArgs e)
            {
                // TODO:
                // Add the time and date verification
                if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    _client.GetGuild(863151265939456043).GetTextChannel(863427166662557696).SendMessageAsync("Вминаниме!");
                
            }
        }
    }
}
