using Discord;
using System;
using System.Threading.Tasks;

namespace orbitFactionBot.Utils {
    public class oPlayers {
        public string playerName;
        public ulong discordID;
        public ulong depositAmount;
        public ulong withdrawAmount;
        public int wallChecks;
        public int bufferChecks;
        public int rpostChecks;
        public System.Collections.Generic.List<string> messageHistory;
        public string whitelistCode;

        public oPlayers(string playerName, ulong discordID, ulong depositAmount, ulong withdrawAmount, int wallChecks, int bufferChecks, int rpostChecks, System.Collections.Generic.List<string> messageHistory, int routpostchecks, string whitelistCode) {
            this.playerName = playerName;
            this.discordID = discordID;
            this.depositAmount = depositAmount;
            this.withdrawAmount = withdrawAmount;
            this.wallChecks = wallChecks;
            this.bufferChecks = bufferChecks;
            this.messageHistory = messageHistory;
            this.rpostChecks = rpostChecks;
            this.whitelistCode = whitelistCode;
        }
        public static bool playerExist(ulong id) {
            foreach (oPlayers players in Program.players) {
                if (players.discordID == id)
                    return true;
            }
            return false;
        }
        public static bool playerExist2(string str) {
            foreach (oPlayers players in Program.players) {
                if (players.playerName == str)
                    return true;
            }
            return false;
        }
        public static string getPlayerName(ulong id) {
            foreach (oPlayers players in Program.players) {
                if (players.discordID == id)
                    return players.playerName;
            }
            return null;
        }
        public static oPlayers getPlayer(string name) {
            foreach (oPlayers players in Program.players) {
                if (players.playerName == name)
                    return players;
            }
            return new oPlayers("", 0, 0, 0, 0, 0, 0, new System.Collections.Generic.List<string>(), 0, "");
        }
        public static async void whitelistProcess(ulong id) {
            await Task.Delay(TimeSpan.FromSeconds(60));
            foreach (oPlayers players in Program.players) {
                if (players.discordID == id && players.playerName == "") {
                    Program.players.Remove(players);
                    JSON.savePlayers(Program.players);
                    await (orbitFactionBot.Discord.Bot.bot.GetChannel(Program.config.whitelistChannel) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Failed", $"<@{id}> Has failed their **whitelist**, their whitelistLog will be **terminated**"));
                }
            }
        }
    }
}
