using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Template.Models;
using Template.Services;

namespace Template.Modules
{
    /// <summary>
    /// Module about schedule generation and interview invites sending
    /// </summary>
    [Summary(" Module about schedule generation and interview invites sending")]
    public class ScheduleModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly string _avatarURL;

        public ScheduleModule(DiscordSocketClient client)
        {
            _client = client;
            _avatarURL = @"https://cdn.discordapp.com/avatars/863761299800326164/82d205c04f0b6157c658f766cf184606.png";
        }

        /// <summary>
        /// Generates timetable for students with certain role
        /// </summary>
        /// <param name="roleToSelect">Role with which select guild users</param>
        [Summary("Generates timetable for students with certain role")]
        [Command("generate_tt")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GenerateTimetable(string date, string time, string flow, SocketRole roleToSelect)
        {
            if (roleToSelect == null)
            {
                await ReplyAsync("Вы не указали роль или пользователей или укажите everyone. `!generate_tt date(dd.mm.yyyy HH:MM) streamName @users/@role`");
                return;
            }
            if (flow == null)
            {
                await ReplyAsync("Вы не указали поток студентов `!generate_tt date(dd.mm.yyyy HH:MM) streamName @user/@role`");
                return;
            }

            List<SocketGuildUser> usersToSend;
            usersToSend = roleToSelect.Members.ToList();

            DateTime interviewDate = new DateTime(1970, 1, 1, 00, 00, 00);
            try
            {
                interviewDate = DateTime.Parse(date + " " + time);
            }
            catch
            {
                await ReplyAsync("Неправильный формат даты в аргументе. dd.mm.yyyy");
            }

            await FillGoogleTable(usersToSend, flow, interviewDate);
        }

        /// <summary>
        /// Generates timetable for certain students
        /// </summary>
        /// <param name="users">the massive of discord users to interview</param>
        [Summary("Generates timetable for certain students")]
        [Command("generate_tt")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GenerateTimetable(string date, string time, string flow, params SocketGuildUser[] users)
        {
            if (users == null)
            {
                await ReplyAsync("Вы не указали пользователей или укажите everyone. `!generate_tt date(dd.mm.yyyy HH:MM) streamName @user/@role`");
                return;
            } 
            if (flow == null)
            {
                await ReplyAsync("Вы не указали поток студентов `!generate_tt date(dd.mm.yyyy HH:MM) streamName @user/@role`");
                return;
            }

            DateTime interviewDate = new DateTime();
            try
            {
                interviewDate = DateTime.Parse(date + " " + time);
            }
            catch
            {
                await ReplyAsync("Неправильный формат даты в аргументе. dd.mm.yyyy");
                return;
            }


            // Converting array[] into List<> because array[] is the only way to input multiply params
            List<SocketGuildUser> socketUsers = new List<SocketGuildUser>();
            socketUsers.AddRange(users);

            await FillGoogleTable(socketUsers, flow, interviewDate);
            await ReplyAsync("Finished executing");
        }

        /// <summary>
        /// Fills the google table with timetable 
        /// </summary>
        /// <param name="users">the array of users to send messages</param>
        /// <param name="ivDuration">TimeSpan interview duration</param>
        /// <param name="breakDuration">TimeSpan break between interviews duration</param>
        /// <param name="lastInterviewEndTime">DateTime the time of the ending of the previous interview</param>
        private async Task FillGoogleTable(List<SocketGuildUser> users, string flow, DateTime date)
        {
            DateTime interviewStartTime, interviewEndTime;
            TimeSpan ivDuration = new TimeSpan(hours: 0, minutes: 30, seconds: 0);
            interviewStartTime = date;
            interviewEndTime = interviewStartTime + ivDuration;

            TimeSpan breakDuration = new TimeSpan(hours: 0, minutes: 5, seconds: 0);

            //DateTime lastInterviewEndTime = SheetsHandler.GetInterviewStart(breakDuration);

            int i = 0;
            var msg = await ReplyAsync($"Started executing : '{i}'\nDelay = .1s");

            foreach (SocketGuildUser user in users)
            {
                try
                {

                    // Check if interview starts later than 20:30
                    DateTime LateTime = new DateTime(interviewStartTime.Year, interviewStartTime.Month, interviewStartTime.Day, 20, 30, 0);
                    bool isTooLateTime = interviewStartTime.TimeOfDay >= LateTime.TimeOfDay;
                    if (isTooLateTime)
                    {
                        interviewStartTime = new DateTime(interviewStartTime.Year,
                                                          interviewStartTime.Month,
                                                          interviewStartTime.Day + 1,
                                                          date.Hour,
                                                          date.Minute,
                                                          date.Second);
                        interviewEndTime = interviewStartTime + ivDuration;
                    }

                    if (string.IsNullOrEmpty(user.Nickname))
                        SheetsHandler.AddRow(user.Username, user.Id, interviewStartTime, interviewEndTime, flow);
                    else
                        SheetsHandler.AddRow(user.Nickname, user.Id, interviewStartTime, interviewEndTime, flow);
                    
                    interviewStartTime = interviewEndTime + breakDuration;
                    interviewEndTime = interviewStartTime + ivDuration;
                    await msg.ModifyAsync(mess => mess.Content = $"Started executing : '{++i}'\nDelay = .1s");
                }
                catch (Exception e)
                {
                    await ReplyAsync($"Error on {i}th element :x:");
                    Console.WriteLine(e);
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Sending interview embed message to students by their IDs in google sheet timetable 
        /// </summary>
        [Summary("Sending interview embed message to students by their IDs in google sheet timetable")]
        [Command("send_tt")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SendInterviewTime()
        {
            // Gets students list from google sheets table
            List<Student> students = SheetsHandler.ReadRow();
            foreach (Student student in students)
            {
                // Formating date and time
                string meetingStartTime = GenerateTime(student.InterviewStart);
                string meetingEndTime = GenerateTime(student.InterviewEnd);
                string meetingDate = GenerateDate(student);

                // Generating and building embed message
                EmbedBuilder builder = GenerateInterviewEmbed(student, meetingStartTime, meetingEndTime, meetingDate);
                var embed = builder.Build();
                var userToSend = _client.GetGuild(863151265939456043).GetUser(student.DiscordId);
                // Send message in Student's DM
                try
                {
                    await userToSend.SendMessageAsync(embed: embed);
                    Console.WriteLine($"Приглашение отправлено ^{userToSend.Nickname}^ ({userToSend.Username})"); 
                }
                catch
                {
                    await ReplyAsync($"{userToSend.Mention} ({userToSend.Id}) заблокировал личные сообщения - пропускаю");
                }
                
                Thread.Sleep(200); // Sleep is necessary because of discord message spam limit
            }
            SheetsHandler.MarkSentInterviews(students);

            // Generates embed message for send_tt command
            EmbedBuilder GenerateInterviewEmbed(Student student, string meetingStartTime, string meetingEndTime, string meetingDate)
            {
                return new EmbedBuilder()
                    .WithTitle("Собеседование! :microphone2:")
                    .WithDescription($"Привет, {student.FirstName} {student.SecondName}, тебе пришло приглашение на собеседование!")
                    .WithColor(new Color(0x3E8DFF))
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText("Академия")
                            .WithIconUrl($"{_avatarURL}");
                    })
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("Академия")
                            .WithIconUrl($"{_avatarURL}");
                    })
                    .AddField(":teacher: Кто тебя интервьюирует?", $"{GenerateTeachersText()}")
                    .AddField(":beach: Где?", $"В канале {student.InterviewChannel}")
                    .AddField(":calendar_spiral:  Когда?", $"{meetingDate}")
                    .AddField(":beginner: Во сколько начнём?", $"в {meetingStartTime}")
                    .AddField(":checkered_flag:  Во сколько закончим?", $"в {meetingEndTime}");

                string GenerateTeachersText(){
                    if (student.InterviewTeacher1Name.Equals(student.InterviewTeacher2Name))
                        return $"С тобой будет {student.InterviewTeacher1Name}";
                    else
                        return $"С тобой будут {student.InterviewTeacher1Name} и {student.InterviewTeacher2Name}";
                }
            }

            // Used to format date for send_tt command embed
            string GenerateDate(Student student)
            {
                string result;
                result = $"{student.InterviewStart.Day} сентября";
                return result;
            }

            // Used to format time for send_tt command embed
            string GenerateTime(DateTime time)
            {
                string result, hoursSTR, minutesSTR;
                int hours = time.Hour, minutes = time.Minute;

                if (hours < 10)
                    hoursSTR = $"0{hours}";
                else
                    hoursSTR = $"{hours}";

                if (minutes < 10)
                    minutesSTR = $"0{minutes}";
                else
                    minutesSTR = $"{minutes}";

                result = $"{hoursSTR}:{minutesSTR}";
                return result;
            }
        }
    }
}