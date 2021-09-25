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
    /// Module with voicechat related commands
    /// </summary>
    public class VoicechatModule : ModuleBase<SocketCommandContext>
    {
        [Summary("VoiceChecker command is used to collect and mark all the users in the voice channel in the all-users-table")]
        [Command("commit_vc")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task VoiceChecker()
        {
            SocketVoiceChannel audioChannel = GetAuthorChannel();
            
            // If the executor is not in the voice channel
            if (audioChannel == null)
            {
                await ReplyAsync("Вы не находитесь в канале.");
                return;
            }

            var today = DateTime.Now.Date.Date;
            var usersInChannel = audioChannel.Users;
            string message = $"Участники в канале {audioChannel.Name} [{today.Year}.{today.Month}.{today.Day} {today.Hour}:{today.Minute}]";
            foreach (SocketGuildUser user in usersInChannel)
            {
                message += $"\n{user.Mention}";
            }
            await ReplyAsync(message);
            await ReplyAsync($"{usersInChannel.Count} участников зарегистрировано в канале {audioChannel.Name}");

            // Finds the voice channel where the command executor is currently in. If there is no VC with user - returns null
            SocketVoiceChannel GetAuthorChannel()
            {
                foreach (SocketVoiceChannel voiceChannel in Context.Guild.VoiceChannels)
                {
                    IReadOnlyCollection<SocketGuildUser> users = voiceChannel.Users;
                    foreach (var user in users)
                        if (user.Id == Context.Message.Author.Id)
                            return voiceChannel;
                }
                return null;
            }
        }
    }
}
