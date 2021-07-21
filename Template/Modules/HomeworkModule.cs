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
        public async Task Test_NextMessageAsync(int lessonNum = -1)
        {
            if (lessonNum < 1)
            {
                await ReplyAsync("Неверный формат команды. Вы не указали номер занятия, образец: !send_hw 1");
                return;
            }

            await ReplyAsync($"Вы прикрепляете домашнее задание к {lessonNum} уроку. Укажите отправьте файл с заданием. Или отправьте любой текст, чтобы прервать отправку");
            var response = await NextMessageAsync();
            if (response != null)
            {
                // if has response
                var attachments = response.Attachments;
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
