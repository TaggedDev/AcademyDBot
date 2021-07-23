using Interactivity;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Template.Modules
{
    public class HomeworkModule : ModuleBase<SocketCommandContext>
    {
        private readonly InteractivityService _service;
        public HomeworkModule(InteractivityService serivce)
        {
            _service = serivce;
        }

        [Command("btns")]
        public async Task testbtncmd()
        {
            ButtonBuilder button = new ButtonBuilder();
            button.CustomId = "btn_";
            button.Label = "test";
            button.Style = ButtonStyle.Primary;

            ButtonBuilder button1 = new ButtonBuilder();
            button1.CustomId = "123";
            button1.Label = "test";
            button1.Style = ButtonStyle.Danger;

            ButtonBuilder button2 = new ButtonBuilder();
            button2.CustomId = "123";
            button2.Label = "test";
            button2.Style = ButtonStyle.Secondary;

            ButtonBuilder button3 = new ButtonBuilder();
            button3.CustomId = "123";
            button3.Label = "test";
            button3.Style = ButtonStyle.Success;

            ButtonBuilder button4 = new ButtonBuilder();
            button4.Label = "test";
            button4.Style = ButtonStyle.Link;
            button4.Url = @"https://sa-academy.tilda.ws";

            var builder = new ComponentBuilder().WithButton(button, row: 0).WithButton(button1, row: 0).WithButton(button2, row: 0).WithButton(button3, row: 1).WithButton(button4, row:1);
            await Context.Channel.SendMessageAsync("Test buttons!", component: builder.Build());

        }

        [Command("ddown")]
        public async Task testddown()
        {
            var builder = new ComponentBuilder()
              .WithSelectMenu(new SelectMenuBuilder()
              .WithCustomId("dd_")
              .WithPlaceholder("This is a placeholder")
              .WithOptions(new List<SelectMenuOptionBuilder>()
              {
                new SelectMenuOptionBuilder()
                  .WithLabel("Option A")
                  .WithDescription("Evan pog champ")
                  .WithValue("value1"),
                new SelectMenuOptionBuilder()
                  .WithLabel("Option B")
                  .WithDescription("Option B is poggers")
                  .WithValue("value2")
              }));
            await Context.Channel.SendMessageAsync("Test selection!", component: builder.Build());
        }

        [Command("send_hw", RunMode = RunMode.Async)]
        [Summary("Calling command to send a prepared homework")]
        public async Task SendHomework()
        {
            //await ReplyAsync($"Вы прикрепляете домашнее задание к {lessonNum} уроку. Укажите отправьте файл с заданием. Или отправьте любой текст, чтобы прервать отправку");
            var response = await _service.NextMessageAsync(x => x.Author.Id == Context.User.Id);
            if (response != null)
            {
                // if has response
                var attachments = response.Value.Attachments;
                if (attachments.Count == 0)
                {
                    // if the message has no attachs or was canceled
                    await ReplyAsync("Вы не прикрепили файл или отменили команду. Вызовите команду ещё раз");
                    return;
                }
                else if(attachments.Count == 1)
                {
                    var attachment = attachments.First();
                    await ReplyAsync($"Вы прикрепили {attachment.Filename}"); 
                } 
                else
                {
                    await ReplyAsync($"Файл должен быть один. Используйте архив для приложения множества файлов, вы прислали {attachments.Count} файлов");
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
