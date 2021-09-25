using Discord.Commands;
using System.Threading.Tasks;

namespace Template.Services
{
    public class ServiceModule : ModuleBase<SocketCommandContext>
    {
        [Command("version")]
        [Alias("v")]
        public async Task CheckVersion()
        {
            await ReplyAsync("Current version: " + SettingsHandler.Version);
        }
    }
}
