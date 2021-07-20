using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Template.Modules
{
    public class VoicechatModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<VoicechatModule> _logger;
        private readonly DiscordSocketClient _client;

        public VoicechatModule(ILogger<VoicechatModule> logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        [Command("commit_vc")]
        public async Task VoiceChecker()
        {
            var vChannel = Context.Guild.GetVoiceChannel(866328191442223135);
            var users = vChannel.Users;
            await ReplyAsync($"{users.Count} участников зарегистрировано в канале {vChannel.Name}");
        }


    }
}
