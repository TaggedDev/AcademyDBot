using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Modules
{
    public class HomeworkModule : InteractiveBase
    {
        [Command("send_hw", RunMode = RunMode.Async)]
        [Summary("Calling command to send a prepared homework")]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("Укажите номер задания");
            var response = await NextMessageAsync();
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("You did not reply before the timeout");
        }
    }
}
