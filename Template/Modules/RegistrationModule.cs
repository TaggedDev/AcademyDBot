using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Template.Modules
{
    public class RegistrationModule : ModuleBase<SocketCommandContext>
    {
        [Command("add_student")]
        [Alias("student", "stud", "ыегвуте", "фвв_ыегвуте")]
        public async Task AddDatabaseStudent(SocketGuildUser user)
        {

            await ReplyAsync($":white_check_mark: {user.Nickname} был добавлен в БД");
        } 
    }
}
