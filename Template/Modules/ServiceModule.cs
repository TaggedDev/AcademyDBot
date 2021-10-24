using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Template.Services
{
    public class ServiceModule : ModuleBase<SocketCommandContext>
    {
        [Command("version")]
        [Alias("v")]
        public async Task CheckVersion() => await ReplyAsync("Current version: " + SettingsHandler.Version);

        [Command("lesson_start")]
        [Alias("l_s", "ls", "lesson", "start", "дуыыщт", "ыефке", "старт", "урок")]
        public async Task RemindAboutLesson(int mins = 0, ITextChannel channel = null)
        {
            if (channel == null)
            {
                if (mins != 0)
                    await ReplyAsync($"<@&786669862226886686>. Занятие начнётся через {mins} минут");
                else
                    await ReplyAsync($"<@&786669862226886686>. Занятие начинается!"); // returns null
            }
            else
            {
                if (mins != 0)
                    await channel.SendMessageAsync($"<@&786669862226886686>. Занятие начнётся через {mins} минут");
                else
                    await channel.SendMessageAsync($"<@&786669862226886686>. Занятие начинается!"); // returns null
            }
        }

        [Command("lesson_start")]
        [Alias("l_s", "ls", "lesson", "start", "дуыыщт", "ыефке", "старт", "урок")]
        public async Task RemindAboutLesson(ITextChannel channel = null, int mins = 0)
        {
            if (channel == null)
            {
                if (mins != 0)
                    await ReplyAsync($"<@&786669862226886686>. Занятие начнётся через {mins} минут");
                else
                    await ReplyAsync($"<@&786669862226886686>. Занятие начинается!"); // returns null
            }
            else
            {
                if (mins != 0)
                    await channel.SendMessageAsync($"<@&786669862226886686>. Занятие начнётся через {mins} минут");
                else
                    await channel.SendMessageAsync($"<@&786669862226886686>. Занятие начинается!"); // returns null
            }
        }
    }
}
