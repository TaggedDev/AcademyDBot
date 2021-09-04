using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
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
        public async Task GenerateTimetable(SocketRole roleToSelect, string flow = "Default")
        {
            if (roleToSelect == null)
            {
                await ReplyAsync("Вы не указали роль выбора студентов");
                return;
            }
            if (flow == null)
            {
                await ReplyAsync("Вы не указали поток студентов");
                return;
            }

            List<SocketGuildUser> usersToSend;
            usersToSend = roleToSelect.Members.ToList();

            await FillGoogleTable(usersToSend, flow);
        }

        /// <summary>
        /// Generates timetable for certain students
        /// </summary>
        /// <param name="users">the massive of discord users to interview</param>
        [Summary("Generates timetable for certain students")]
        [Command("generate_tt")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GenerateTimetable(string flow = "Default", params SocketGuildUser[] users)
        {
            if (users == null)
            {
                await ReplyAsync("Вы не указали пользователей или укажите everyone");
                return;
            } 
            if (flow == null)
            {
                await ReplyAsync("Вы не указали поток студентов");
                return;
            }

            // Converting array[] into List<> because array[] is the only way to input multiply params
            List<SocketGuildUser> socketUsers = new List<SocketGuildUser>();
            socketUsers.AddRange(users);

            await FillGoogleTable(socketUsers, flow);
            await ReplyAsync("Finished executing");
        }

        /// <summary>
        /// Fills the google table with timetable 
        /// </summary>
        /// <param name="users">the array of users to send messages</param>
        /// <param name="ivDuration">TimeSpan interview duration</param>
        /// <param name="breakDuration">TimeSpan break between interviews duration</param>
        /// <param name="lastInterviewEndTime">DateTime the time of the ending of the previous interview</param>
        private async Task FillGoogleTable(List<SocketGuildUser> users, string flow)
        {
            TimeSpan ivDuration = new TimeSpan(hours: 0, minutes: 30, seconds: 0);
            TimeSpan breakDuration = new TimeSpan(hours: 0, minutes: 5, seconds: 0);
            DateTime lastInterviewEndTime = SheetsHandler.GetInterviewStart(breakDuration);

            int i = 0;
            var msg = await ReplyAsync($"Started executing : '{i}'\nDelay = .1s");

            foreach (SocketGuildUser user in users)
            {
                try
                {
                    // Check whether time is not normal.
                    bool isCorrectTime = lastInterviewEndTime.Hour >= 20 && lastInterviewEndTime.Minute >= 30 ||
                        lastInterviewEndTime.DayOfWeek == DayOfWeek.Saturday && lastInterviewEndTime.DayOfWeek == DayOfWeek.Sunday;
                    // If is then we go to the next day and start with 16:00.
                    if (isCorrectTime)
                    {
                        lastInterviewEndTime = new DateTime(lastInterviewEndTime.Year,
                                                                lastInterviewEndTime.Month,
                                                                lastInterviewEndTime.Day + GeneratePauseTime(lastInterviewEndTime),
                                                                16, 00, 00);
                    }
                    SheetsHandler.AddRow(user.Nickname, user.Id, lastInterviewEndTime, lastInterviewEndTime + ivDuration, flow);
                    lastInterviewEndTime = lastInterviewEndTime + ivDuration + breakDuration;
                    await msg.ModifyAsync(mess => mess.Content = $"Started executing : '{++i}'\nDelay = .1s");
                }
                catch
                {
                   await ReplyAsync($"Error on {i}th element :x:");
                }
                Thread.Sleep(100);
            }

            static int GeneratePauseTime(DateTime lastInterviewEndTime)
            {
                return lastInterviewEndTime.DayOfWeek switch
                {
                    DayOfWeek.Friday => 3,
                    DayOfWeek.Saturday => 2,
                    _ => 1,
                };
            }
        }

        /// <summary>
        /// Sending interview embed message to students by their IDs in google sheet timetable 
        /// </summary>
        [Summary("Sending interview embed message to students by their IDs in google sheet timetable")]
        [Command("send_tt")]
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

                // Send message in Student's DM
                await _client.GetGuild(863151265939456043).GetUser(student.DiscordId).SendMessageAsync(embed: embed);
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