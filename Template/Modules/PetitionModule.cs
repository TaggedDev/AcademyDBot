using Discord;
using Discord.Commands;
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

        public PetitionModule(ILogger<PetitionModule> logger) => _logger = logger;

        [Command("petition")]
        public async Task PetitionCommand([Remainder] string text)
        {
            var builder = new EmbedBuilder()
                .WithColor(new Color(0xC70F0A))
                .WithTimestamp(DateTime.Now)
                .WithFooter(footer => {
                    footer
                        .WithText("Бот Академии")
                        .WithIconUrl(@"https://cdn.discordapp.com/avatars/863761299800326164/82d205c04f0b6157c658f766cf184606.png");
                })
                .AddField("Новая жалоба", $"{text}"); ;

            var embed = builder.Build();
            await Context.Guild.GetTextChannel(863427166662557696).SendMessageAsync(embed: embed);
        }
    }
}