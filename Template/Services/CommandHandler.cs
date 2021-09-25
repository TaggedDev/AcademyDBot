using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Template.Services
{
    /// <summary>
    /// CommandHandler is a core class to handle incoming events. 
    /// </summary>
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        //private readonly HomeworkModule _hwModule;
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

    }
}