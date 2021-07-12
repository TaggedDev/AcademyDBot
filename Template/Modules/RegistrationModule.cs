using Discord.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Template.Modules
{
    public class RegistrationModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<RegistrationModule> _logger;

        public RegistrationModule(ILogger<RegistrationModule> logger) => _logger = logger;

        /// <summary>
        /// Sends registration message in the same channel and adds Duck reaction
        /// </summary>
        /*[Command("generateRegistrationMessage")]
        public async void PingAsync()
        {
            string text = "Привет! :tada::wave:\n" +
                "Добро пожаловать на Discord сервер Академии. Нажми на уточку под сообщением чтобы зарегистрироваться";
            IUserMessage message = await ReplyAsync(text);
            IEmote duckEmoji = new Emoji("🦆");
            await message.AddReactionAsync(duckEmoji);
            _logger.LogInformation($"{Context.User.Username} executed the ping command!");
        }*/
    }
}