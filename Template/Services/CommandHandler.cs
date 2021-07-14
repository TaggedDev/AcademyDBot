using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Template.Services
{
    /// <summary>
    /// CommandHandler is a core class to handle incoming events. 
    /// </summary>
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;

        private readonly ulong _regMessageID;
        private readonly ulong _studentRoleID;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;

            // Get the IDs from json file
            using (StreamReader reader = new StreamReader("appsettings.json"))
            {
                string json = reader.ReadToEnd();
                dynamic obj = JsonConvert.DeserializeObject(json);
                _regMessageID = obj.regMessageId;
                _studentRoleID = obj.studentRoleId;
            }
        }

        /// <summary>
        /// Subscription on events and calling the subscripted functions 
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns></returns>
        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.ReactionAdded += OnReactionAdded;
            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        /// <summary>
        /// Calls when bot recieves the message and checks the correct conditions
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            if (!message.HasStringPrefix(_config["prefix"], ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
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
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // Adds student role to a person who put a duck emoji to registration message
            if (reaction.MessageId != _regMessageID) return;
            if (reaction.Emote.Name != "🦆") return;

            var role = (channel as SocketGuildChannel).Guild.GetRole(_studentRoleID);
            await (reaction.User.Value as SocketGuildUser).AddRoleAsync(role);
            var user = reaction.User.Value;

            // Message student he has registered successfully
            string text = "**Вы успешно зарегистрировались на сервере академии!**\n" +
                "Что теперь? Первая информация здесь: <#863428367356002314> (нажми на рупор)";
            await user.SendMessageAsync(text);
        }
    }
}