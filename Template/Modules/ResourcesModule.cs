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
        private readonly DiscordSocketClient _client;
        public ResourcesModule(DiscordSocketClient client) => _client = client;

        /// <summary>
        /// This command sends resource message suggestion to the admins chat
        /// </summary>
        /// <param name="url">resource URL</param>
        /// <param name="description">resource Description</param>
        [Command("resource")]
        public async Task SendResource(string url = null, [Remainder]string description = null)
        {
            if (url == null || description == null)
            {
                await ReplyAsync("Неверный формат команды. Правильный формат: !resource `URL` `long description`. Всё поля обязательны");
                return;
            }

            string text = $"**Новое предложение ресурса:** {url}\n**Описание:** {description}\n**От:** {Context.User.Mention}";
            // I am not using Context.Guild because this command must be able to be executed in bot's DMs
            // _client.GetGuild() works for both variants
            await _client.GetGuild(863151265939456043).GetTextChannel(863427166662557696).SendMessageAsync(text);
        }

        /// <summary>
        /// Command is used by admins and teachers to approve custom or their own resources in #resources channel
        /// </summary>
        /// <param name="url">resource URL</param>
        /// <param name="desc">resource Description</param>
        [Command("apply_resource")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task ApplyResource(string url = null, [Remainder] string desc = null)
        {
            if (url == null || desc == null)
            {
                await ReplyAsync("Неверный формат команды. Правильный формат: !apply_resource `URL` `long description`. Всё поля обязательны");
                return;
            }

            var builder = new EmbedBuilder()
                .WithTitle("Новый ресурс! :new:")
                .WithDescription($"{desc}")
                .WithColor(new Color(0x91DE10))
                .WithTimestamp(DateTime.Now)
                .WithFooter(footer => {
                    footer
                        .WithText("Академия")
                        .WithIconUrl($"{_client.CurrentUser.GetAvatarUrl()}");
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
