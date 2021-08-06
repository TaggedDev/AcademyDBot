using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Template.Models;

namespace Template.Modules
{
    public sealed class RegistrationModule : ModuleBase<SocketCommandContext>
    {
        [Command("add_student")]
        [Alias("student", "stud", "ыегвуте", "фвв_ыегвуте")]
        public async Task AddDatabaseStudent(SocketGuildUser user)
        {
            try
            {
                Student.AddStudentToDB(user.Id, user.Nickname.Split(" ")[0], user.Nickname.Split(" ")[1], DateTime.Now);
                await ReplyAsync($":white_check_mark: {user.Nickname} был добавлен в БД");
            }
            catch (SqlException)
            {   
                await ReplyAsync($":x: {user.Nickname} уже есть в БД");
            }
        } 
    }
}
