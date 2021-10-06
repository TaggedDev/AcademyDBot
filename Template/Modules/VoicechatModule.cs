using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var today = DateTime.Now.Date;
            var usersInChannel = audioChannel.Users;
            string message = $"Участники в канале `{audioChannel.Name}` [{today.Year}.{today.Month}.{today.Day} {today.Hour}:{today.Minute}]";
            string tutorsMSG = $"\n<@&863158732170592297>\n", listenersMSG = "\n<@&863158815210078248>\n", orgsMSG = "\n<@&863158341094342696>\n";
            int listeners = 0, tutors = 0, orgs = 0;
            foreach (SocketGuildUser user in usersInChannel)
            {
                if (user.Roles.Any(x => x.Id == 863158341094342696))
                {
                    orgsMSG += $"\n{user.Mention}";
                    orgs++;
                }
                else if (user.Roles.Any(x => x.Id == 863158732170592297))
                {
                    tutorsMSG += $"\n{user.Mention}";
                    tutors++;
                }
                else
                {
                    listenersMSG += $"\n{user.Mention}";
                    listeners++;
                }
            }
            await ReplyAsync(message + orgsMSG + tutorsMSG + listenersMSG);
            await ReplyAsync($"{usersInChannel.Count} участников зарегистрировано в канале {audioChannel.Name}. {listeners}/{tutors}/{orgs} Слушателей/Преподавателей/Организаторов");

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
