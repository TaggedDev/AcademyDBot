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
        private static string _discordToken;
        private static string _prefix;
        private static string _version;

        public static string DiscordToken { get => _discordToken; set => _discordToken = value; }
        public static string Prefix { get => _prefix; set => _prefix = value; }
        public static string Version { get => _version; set => _version = value; }

        static SettingsHandler()
        {
            using StreamReader reader = new StreamReader("appsettings.json");
            string json = reader.ReadToEnd();

            dynamic obj = JsonConvert.DeserializeObject(json);
            DiscordToken = obj.token;
            Prefix = obj.prefix;

            using StreamReader versionReader = new StreamReader("version.json");
            string versionJson = reader.ReadToEnd();

            dynamic vObj = JsonConvert.DeserializeObject(versionJson);
            Version = vObj.version;
        }


    }
}
