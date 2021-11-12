using Discord;
using System;
using System.Linq;

namespace orbitFactionBot.Utils {
    class Discord {
        public static Random random = new Random();
        public static Embed oEmbed(string title, string desc) {
            return new EmbedBuilder().WithTitle(title).WithDescription(desc).WithColor(Program.embedColor).WithFooter("OrbitDevelopment").WithCurrentTimestamp().Build();
        }
        public static Embed oEmbedFtop(string title, string pos_faction, string value, string change) {
            return new EmbedBuilder().WithTitle(title).AddField("Factions:", pos_faction, true).AddField("Values:", value, true).AddField("Change:", change, true).WithColor(Program.embedColor).WithFooter("OrbitDevelopment").WithCurrentTimestamp().Build();
        }
        public static string randomCode(int amt) {
            return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToLower(), amt).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
