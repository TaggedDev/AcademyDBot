using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Template.Models;
using Template.Services;

namespace Template.Modules
{
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
        /// Connects to the google spreadsheet and generates the timetable for interviews
        /// </summary>
        /// <param name="users">the massive of discord users to interview</param>
        /// <returns></returns>
        [Command("generate_tt")]
        public async Task GenerateTimetable(params SocketUser[] users)
        {
            DateTime beginDate = new DateTime(year: 2021, month: 9, day: 10, hour: 16, minute: 0, second: 0); // 2021.09.10 in 16:00 we start first interview
            TimeSpan ivDuration = new TimeSpan(hours: 0, minutes: 30, seconds: 0);
            TimeSpan breakDuration = new TimeSpan(hours: 0, minutes: 5, seconds: 0);
            DateTime lastInterviewEndTime = beginDate;

            int i = 0;
            var msg = await ReplyAsync($"Started executing : '{i}'");

            foreach (SocketUser user in users)
            {
                try
                {
                    SheetsHandler.AddRow(user.Id, lastInterviewEndTime, lastInterviewEndTime + ivDuration);
                    lastInterviewEndTime = lastInterviewEndTime + ivDuration + breakDuration;
                    await msg.ModifyAsync(mess => mess.Content = $"Started executing : '{++i}'");
                }
                catch
                {
                    await ReplyAsync($"Error on {i}th element :x:");
                }
                Thread.Sleep(100);
            }
            await ReplyAsync("Finished executing");
        }
        
        /// <summary>
        /// Sending interview embed message to students by their IDs in google sheet timetable 
        /// </summary>
        /// <returns></returns>
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