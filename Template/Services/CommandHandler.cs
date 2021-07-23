using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

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
            // Hook Interacitivities to handle them
            _discord.InteractionCreated += OnInteractionAsync;
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
            if (!message.HasStringPrefix(SettingsHandler.Prefix, ref argPos) && !message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;

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
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            // Adds student role to a person who put a duck emoji to registration message
            if (reaction.MessageId != SettingsHandler.RegMessageId) return;
            if (reaction.Emote.Name != "🦆") return;

            var role = _discord.GetGuild(863151265939456043).GetRole(SettingsHandler.StudentRoleId);
            await (reaction.User.Value as SocketGuildUser).AddRoleAsync(role);
            var user = reaction.User.Value;

            // Message student he has registered successfully
            string text = "**Вы успешно зарегистрировались на сервере академии!**\n" +
                "Что теперь? Первая информация здесь: <#863428367356002314> (нажми на рупор)";
            await user.SendMessageAsync(text);
        }

        /// <summary>
        /// Handles Interactions when triggered
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task OnInteractionAsync(SocketInteraction arg)
        {
            switch (arg.Type) // We want to check the type of this interaction
            {
                //Slash commands : in plans
                case InteractionType.ApplicationCommand:
                    /*await MySlashCommandHandler(arg);*/ 
                    break;
                //Button clicks/selection dropdowns
                case InteractionType.MessageComponent:
                    await OnMessageComponentTriggered(arg);
                    break;
                //Unused
                case InteractionType.Ping:
                    break;
                //Unknown/Unsupported
                default:
                    Console.WriteLine("Unsupported interaction type: " + arg.Type);
                    break;
            }
        }

        private async Task OnMessageComponentTriggered(SocketInteraction arg)
        {
            // Parse the arg
            var parsedArg = (SocketMessageComponent)arg;

            var customId = parsedArg.Data.CustomId; // custom id which was clicked
            var user = (SocketGuildUser)arg.User; // user who called the event
            var guild = user.Guild; // guild where the user called the event
            var channel = parsedArg.Channel; // channel where the user called the event

            if (customId.Substring(0, customId.IndexOf('_')).Equals("btn"))
            {
                // if the caller is a button
                await channel.SendMessageAsync($"{user} нажал на кнопку с ID: {customId} в канале {channel.Name} сервера {guild.Name}");
            }
            else if (customId.Substring(0, customId.IndexOf('_')).Equals("dd"))
            {
                // if the caller is a dropdown menu
                await channel.SendMessageAsync($"{user} выбрал элемент выпадающего меню с  ID: {customId}. Value: {parsedArg.Data.Values.First()} в канале {channel.Name} сервера {guild.Name}");
            }
            else
            {
                // in case I forgot about my convention
                await channel.SendMessageAsync(":x: Ошибка! сообщите тех. администратору");
            }

            
        }
    }
}