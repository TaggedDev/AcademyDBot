using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Template.Models
{
    public class DiscordModel : ModuleBase<SocketCommandContext>
    {
        private static ulong _guildID;
        private static ulong _newsChannelID;
        private static ulong _studentsRoleID;
        private static DiscordSocketClient _client;

        public static ITextChannel NewsChannel { get; set; }
        public static IGuild Guild { get; set; }
        public static IRole StudentsRole { get; set; }

        public static void UpdateModel(DiscordSocketClient client)
        {
            _client = client;

            using (StreamReader reader = new StreamReader("discordconsts.json"))
            {
                string json = reader.ReadToEnd();
                dynamic obj = JsonConvert.DeserializeObject(json);
                _guildID = obj.GuildID;
                _newsChannelID = obj.NewsChannelID;
                _studentsRoleID = obj.StudentsRoleID;
            }

            Guild = _client.GetGuild(_guildID);
            StudentsRole = Guild.GetRole(_studentsRoleID);
            NewsChannel = GetTextChannel().Result;
        }

        private static async Task<ITextChannel> GetTextChannel() => await Guild.GetTextChannelAsync(_newsChannelID);
    }
}
