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
    public class ScheduleModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ScheduleModule> _logger;
        private readonly DiscordSocketClient _client;
        private readonly string _avatarURL;

        public ScheduleModule(ILogger<ScheduleModule> logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
            _avatarURL = @"https://cdn.discordapp.com/avatars/863761299800326164/82d205c04f0b6157c658f766cf184606.png";
        }

        /// <summary>
        /// Generates timetable for students with certain role
        /// </summary>
        /// <param name="roleToSelect">Role with which select guild users</param>
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

            var guildUsers = Context.Guild.Users;
            List<SocketUser> usersToSend = new List<SocketUser>();

            if (roleToSelect.Id != Context.Guild.Id) // roleToSelect is not @everyone 
            {
                foreach (SocketGuildUser selectedUser in guildUsers) // goes through all users in guild
                    if (selectedUser.Roles.Any(r => r.Id == roleToSelect.Id)) // if the user has roleToSelect role, then 
                        if (!selectedUser.IsBot) // if it is not a bot indeed
                            usersToSend.Add(selectedUser); // Adds user to array of users with roleToSelect role                    
            }
            else // roleToSelect is @everyone
            {
                foreach (SocketUser everyUser in guildUsers)
                    if (!everyUser.IsBot)
                        usersToSend.Add(everyUser);
            }

            await FillGoogleTable(usersToSend, flow);
        }

        /// <summary>
        /// Generates timetable for certain students
        /// </summary>
        /// <param name="users">the massive of discord users to interview</param>
        [Command("generate_tt")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GenerateTimetable(string flow = "Default", params SocketUser[] users)
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
            List<SocketUser> socketUsers = new List<SocketUser>();
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
        private async Task FillGoogleTable(List<SocketUser> users, string flow)
        {
            TimeSpan ivDuration = new TimeSpan(hours: 0, minutes: 30, seconds: 0);
            TimeSpan breakDuration = new TimeSpan(hours: 0, minutes: 5, seconds: 0);
            DateTime lastInterviewEndTime = SheetsHandler.GetInterviewStart(breakDuration);

            int i = 0;
            var msg = await ReplyAsync($"Started executing : '{i}'\nDelay = .1s");

            foreach (SocketUser user in users)
            {
                //try
                {

                    // Check whether time is unnormal.
                    // If yes, then we go to the next day and start with 16:00.
                    if (lastInterviewEndTime.Hour >= 20 && lastInterviewEndTime.Minute >= 30 || 
                        lastInterviewEndTime.DayOfWeek == DayOfWeek.Saturday && lastInterviewEndTime.DayOfWeek == DayOfWeek.Sunday)
                    {
                        lastInterviewEndTime = new DateTime(lastInterviewEndTime.Year,
                                                                lastInterviewEndTime.Month,
                                                                lastInterviewEndTime.Day + SetPause(lastInterviewEndTime),
                                                                16, 00, 00);
                    }
                    SheetsHandler.AddRow(user.Id, lastInterviewEndTime, lastInterviewEndTime + ivDuration, flow);
                    lastInterviewEndTime = lastInterviewEndTime + ivDuration + breakDuration;
                    await msg.ModifyAsync(mess => mess.Content = $"Started executing : '{++i}'\nDelay = .1s");
                }
                //catch
                {
                   // await ReplyAsync($"Error on {i}th element :x:");
                }
                Thread.Sleep(100);
            }

            static int SetPause(DateTime lastInterviewEndTime)
            {
                switch (lastInterviewEndTime.DayOfWeek)
                {
                    case DayOfWeek.Friday:
                        return 3;
                    case DayOfWeek.Saturday:
                        return 2;
                    default:
                        return 1;
                } 
            }
        }

       

        /// <summary>
        /// Sending interview embed message to students by their IDs in google sheet timetable 
        /// </summary>
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
                EmbedBuilder builder = GenerateEmbed(student, meetingStartTime, meetingEndTime, meetingDate);
                var embed = builder.Build();

                // Send message in Student's DM
                await _client.GetGuild(863151265939456043).GetUser(student.DiscordId).SendMessageAsync(embed: embed);
                Thread.Sleep(200); // Sleep is necessary because of discord message spam limit
            }
        }

        /// <summary>
        /// Generates embed message for send_tt command
        /// </summary>
        /// <param name="student">Student object built from google sheet</param>
        /// <param name="meetingStartTime">Formated readable start time</param>
        /// <param name="meetingEndTime">Formated readable end time</param>
        /// <param name="meetingDate">Formated readable date</param>
        /// <returns>Embed builder</returns>
        private EmbedBuilder GenerateEmbed(Student student, string meetingStartTime, string meetingEndTime, string meetingDate)
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
                .AddField(":teacher: Кто тебя интервьюирует?", "С тобой будут {lehrerNameEins} und {lehrerNameZwei}")
                .AddField(":beach: Где?", "В канале {channelName}")
                .AddField(":calendar_spiral:  Когда?", $"{meetingDate}")
                .AddField(":beginner: Во сколько начнём?", $"в {meetingStartTime}")
                .AddField(":checkered_flag:  Во сколько закончим?", $"в {meetingEndTime}");
        }

        /// <summary>
        /// Used to format date for send_tt command embed
        /// </summary>
        /// <param name="student">Student for who this time is being generated</param>
        /// <returns>DD:September string</returns>
        private string GenerateDate(Student student)
        {
            string result;
            result = $"{student.InterviewStart.Day} сентября";
            return result;
        }

        /// <summary>
        /// Used to format time for send_tt command embed
        /// </summary>
        /// <param name="time">Time to format</param>
        /// <returns>HH:MM string</returns>
        private string GenerateTime(DateTime time)
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