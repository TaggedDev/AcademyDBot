/*using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Template.Models;
using System.Text.RegularExpressions;

namespace Template.Modules
{
    public class HomeworkModule : ModuleBase<SocketCommandContext>
    {
        private readonly InteractivityService _service;
        private readonly ButtonBuilder _attachNew = new ButtonBuilder()
        {
            CustomId = "btn_hw_attachNew",
            Style = ButtonStyle.Primary,
            Label = "Прикрепить новый",
            //Emote = new Emoji(":new:"),
        };
        private readonly ButtonBuilder _editPrevious = new ButtonBuilder()
        {
            CustomId = "btn_hw_editPrevious",
            Style = ButtonStyle.Secondary,
            Label = "Редактировать ДЗ",
            //Emote = new Emoji(":pencil:"),
        };
        private readonly string _githubRegexPattern = "^[a-zA-Z]+://github\\.com/[a-zA-Z]+/[a-zA-Z]+$";
        public HomeworkModule(InteractivityService serivce)
        {
            _service = serivce;
        }

        [Command("homework", RunMode = RunMode.Async)]
        [Alias("hw", "menu", "menu_hw", "hwmenu", "menuhw", "рц", "домашка", "dz", "lp")]
        [Summary("Method calls the homework menu")]
        public async Task CallHomeworkMenu()
        {
            if (!Context.IsPrivate)
            {
                await ReplyAsync("Команду можно исполнять только в личных сообщениях бота");
                return;
            }
            var builder = new ComponentBuilder().WithButton(_attachNew, row: 0).WithButton(_editPrevious, row: 0);
            string text = "Используйте кнопки из меню";
            await ReplyAsync(text, component: builder.Build());
        }

        /// <summary>
        /// Sends a dropdown menu after a button in CallHomeworkMenu() was pressed
        /// </summary>
        /// <param name="userId">User who invoked the command id</param>
        /// <param name="channel">DMs with user</param>
        public async Task SendAddHomeworkMessage(ISocketMessageChannel channel)
        {
            Lesson[] lessons = GetLastFiveLessons();
            List<SelectMenuOptionBuilder> lessonOptions = GenerateDropdownOptions(lessons);

            var dropdownBuilder = new ComponentBuilder().WithSelectMenu(new SelectMenuBuilder()
            {
                CustomId = "dd_lessons",
                Placeholder = "Уроки",
                Options = lessonOptions
            });

            await channel.SendMessageAsync("Выберите урок из меню", component: dropdownBuilder.Build());

            // Generates and returns last five lessons 
            Lesson[] GetLastFiveLessons()
            {
                Lesson[] lessons = new Lesson[5];
                for (int i = 0; i < 5; i++)
                {
                    Lesson lesson = new Lesson() { Date = DateTime.Now, Number = (byte)(i + 1), Topic = "C#" };
                    lessons[i] = lesson;
                }
                return lessons;
            }
            // Generates and returns the dropdown options at lessons array
            List<SelectMenuOptionBuilder> GenerateDropdownOptions(Lesson[] lessons)
            {
                List<SelectMenuOptionBuilder> lessonOptions = new List<SelectMenuOptionBuilder>();
                foreach (Lesson lesson in lessons)
                {
                    SelectMenuOptionBuilder lessonOption = new SelectMenuOptionBuilder()
                    {
                        Label = $"[{lesson.Date}] {lesson.Topic}",
                        Description = $"{lesson.HomeworkDescription}",
                        Value = $"dd_hw_attach{lesson.Number}"
                    };
                    lessonOptions.Add(lessonOption);
                }
                return lessonOptions;
            }
        }

        /// <summary>
        /// Method waits for uploading file or a github link and loads it in the table
        /// </summary>
        /// <param name="lessonNumber">the number of the lesson to attach the hw</param>
        /// <param name="channel">the channel where the function was called</param>
        public async Task WaitForHomeworkFile(int lessonNumber = Int32.MinValue, ISocketMessageChannel channel = null)
        {
            // Wrong arguments checkup
            if (lessonNumber == Int32.MinValue)
                return;
            if (channel == null)
                return;

            await channel.SendMessageAsync("Прикрепите файл с заданием или ссылку на гитхаб");
            var response = await _service.NextMessageAsync();
            if (response != null)
            {
                // if has response and its not an empty message
                if (response.Value == null)
                    return;

                var attachments = response.Value.Attachments;
                if (attachments.Count == 0)
                {
                    // If user sent no file attachments
                    string content = response.Value.Content;
                    if (Regex.IsMatch(content, _githubRegexPattern))
                    {
                        // If user sent a github link
                        await channel.SendMessageAsync($"Вы прикрепили {content} к уроку {lessonNumber}");
                        return;
                    }
                    else
                    {
                        // if the message has no attachs or github link was send
                        await channel.SendMessageAsync("Вы не прикрепили файл или отменили команду. Вызовите команду ещё раз");
                        return;
                    }
                }
                else if (attachments.Count == 1)
                {
                    // If the attachment is only one
                    var attachment = attachments.First();
                    await channel.SendMessageAsync($"Вы прикрепили {attachment.Filename} к уроку {lessonNumber}");
                }
                else
                {
                    // If there are more attachments
                    await channel.SendMessageAsync($"Файл должен быть один. Используйте архив для приложения множества файлов, вы прислали {attachments.Count} файлов");
                    return;
                }
            }
            else
            {
                // if the time is up
                await ReplyAsync("Вы не успели прикрепить домашнее, вызовите команду ещё раз");
                return;
            }
        }
    }
}
*/