using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Template.Services
{
    /// <summary>
    /// Bot system settigns and constants
    /// </summary>
    class SettingsHandler
    {
        private static ulong _regMessageId;
        private static ulong _studentRoleId;
        private static string _discordToken;
        private static string _prefix;

        public static ulong RegMessageId { get => _regMessageId; set => _regMessageId = value; }
        public static ulong StudentRoleId { get => _studentRoleId; set => _studentRoleId = value; }
        public static string DiscordToken { get => _discordToken; set => _discordToken = value; }
        public static string Prefix { get => _prefix; set => _prefix = value; }

        static SettingsHandler()
        {
            using StreamReader reader = new StreamReader("appsettings.json");
            string json = reader.ReadToEnd();

            dynamic obj = JsonConvert.DeserializeObject(json);
            RegMessageId = obj.regMessageId;
            StudentRoleId = obj.studentRoleId;
            DiscordToken = obj.token;
            Prefix = obj.prefix;
        }


    }
}
