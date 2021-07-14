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
    /// The class defines the resource management stuff like applies and submitting
    /// </summary>
    public class ResourcesModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ResourcesModule> _logger;
        private readonly DiscordSocketClient _client;

        public ResourcesModule(ILogger<ResourcesModule> logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

            [Command("resource")]
        public async Task SendResource(string url = null, [Remainder]string description = null)
        {
            if (url == null)
            {
                await ReplyAsync("Укажите ссылку на ресурс и описание");
                return;
            }
            if (description == null)
            {
                await ReplyAsync("Укажите описание ресурса после ссылки");
                return;
            }

            string text = $"**Новое предложение ресурса:** {url}\n**Описание:** {description}\n**От:** {Context.User.Mention}";
            await _client.GetGuild(863151265939456043).GetTextChannel(863427166662557696).SendMessageAsync(text);
        }

        [Command("apply_resource")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task ApplyResource(string url = null, [Remainder] string desc = null)
        {
            if (url == null || desc == null)
            {
                await ReplyAsync("Неверный формат команды. Правильный формат: !apply_resource url description. Всё поля обязательны");
                return;
            }
            var builder = new EmbedBuilder()
                .WithTitle("Новый ресурс! :new:")
                .WithDescription($"{desc}")
                .WithColor(new Color(0x91DE10))
                .WithTimestamp(DateTimeOffset.FromUnixTimeMilliseconds(1626157171452))
                .WithFooter(footer => {
                    footer
                        .WithText("footer text")
                        .WithIconUrl("https://cdn.discordapp.com/embed/avatars/0.png");
                })
                .WithAuthor(author => {
                    author
                        .WithName("Академия")
                        .WithIconUrl("https://cdn.discordapp.com/embed/avatars/0.png");
                })
                .AddField(":link: Ссылка:", $"{url}");

            var embed = builder.Build();
            await _client.GetGuild(863151265939456043).GetTextChannel(863428462961098762).SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }

    }
}
