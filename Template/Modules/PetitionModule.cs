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
    public class PetitionModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<PetitionModule> _logger;
        private readonly DiscordSocketClient _client;

        public PetitionModule(ILogger<PetitionModule> logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        [Command("petition")]
        public async Task PetitionCommand([Remainder] string text)
        {
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
            await _client.GetGuild(863151265939456043).GetTextChannel(863427166662557696).SendMessageAsync(embed: embed);
        }
    }
}