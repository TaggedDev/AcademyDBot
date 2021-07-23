using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.IO;

namespace Template.Services
{
    /// <summary>
    /// CommandHandler is a core class to handle incoming events. 
    /// </summary>
    [Summary("CommandHandler is a core class to handle incoming events.")]
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        private readonly ulong _regMessageID;
        private readonly ulong _studentRoleID;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            // Hook CommandExecuted to handle post-command-execution logic.
            _commands.CommandExecuted += OnCommandExecutedAsync;
            // Hook MessageReceived so we can process each message to see if it qualifies as a command.
            _discord.MessageReceived += OnMessageReceivedAsync;
            // Hook ReactionAdded to handle reaction added events
            _discord.ReactionAdded += OnReactionAdded;
            
            // Get the fields from json file
            using (StreamReader reader = new StreamReader("appsettings.json"))
            {
                string json = reader.ReadToEnd();
                dynamic obj = JsonConvert.DeserializeObject(json);
                _regMessageID = obj.regMessageId;
                _studentRoleID = obj.studentRoleId;
            }
        }

        /// <summary>
        /// Register modules that are public and inherit ModuleBase<T>.
        /// </summary>
        public async Task InitializeAsync() => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        /// <summary>
        /// Calls when bot recieves the message and checks the correct conditions
        /// </summary>
        private async Task OnMessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix("!", ref argPos) && !message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        /// <summary>
        /// Calls when message is executed
        /// </summary>
        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }

        /// <summary>
        /// Calls when bot recieves the reaction added event
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel">The channel where the reaction was added</param>
        /// <param name="reaction">The reaction was added</param>
        /// <returns></returns>
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            // Adds student role to a person who put a duck emoji to registration message
            if (reaction.MessageId != _regMessageID) return;
            if (reaction.Emote.Name != "🦆") return;

            var role = _discord.GetGuild(863151265939456043).GetRole(_studentRoleID);
            await (reaction.User.Value as SocketGuildUser).AddRoleAsync(role);
            var user = reaction.User.Value;

            // Message student he has registered successfully
            string text = "**Вы успешно зарегистрировались на сервере академии!**\n" +
                "Что теперь? Первая информация здесь: <#863428367356002314> (нажми на рупор)";
            await user.SendMessageAsync(text);
        }
    }
}