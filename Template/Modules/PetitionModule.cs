using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Template.Modules
{
    /// <summary>
    /// Calls defines the anonymous petitions to the admins. As far anonymous as Discord.NET can be
    /// </summary>
    [Summary("Calls defines the anonymous petitions to the admins. As far anonymous as Discord.NET can be")]
    public class PetitionModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        public PetitionModule(DiscordSocketClient client) => _client = client;

        /// <summary>
        /// Command provides a student ability to generate an anonymous petition or yell for help.
        /// </summary>
        /// <param name="text">The long description of the petition</param>
        [Summary("Command provides a student ability to generate an anonymous petition or yell for help.")]
        [Command("petition")]
        public async Task PetitionCommand([Remainder] string text = null)
        {
            if (text == null)
            {
                await ReplyAsync("Неверный формат команды. Правильный формат: !petition `long description`. Все поля обязательны!");
                return;
            }

            var builder = new EmbedBuilder()
                .WithColor(new Color(0xC70F0A))
                .WithTimestamp(DateTime.Now)
                .WithFooter(footer => {
                    footer
                        .WithText($"Бот Академии | От: {Context.User.Mention}")
                        .WithIconUrl(@"https://cdn.discordapp.com/avatars/863761299800326164/82d205c04f0b6157c658f766cf184606.png");
                })
                .AddField("Новая жалоба", $"{text}"); ;

            var embed = builder.Build();
            // I am not using Context.Guild because this command must be able to be executed in bot's DMs
            // _client.GetGuild() works for both variants
            await _client.GetGuild(863151265939456043).GetTextChannel(863427166662557696).SendMessageAsync(embed: embed);
        }
    }
}