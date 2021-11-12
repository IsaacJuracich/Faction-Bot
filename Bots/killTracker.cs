using Discord;
using MinecraftClient;
using orbitFactionBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orbitFactionBot.Bots {
    public class killTracker : ChatBot {
        EasyMatch matcher = new EasyMatch("{", "}");
        public string player;
        public string player2;
        public string rare;
        public string weapon;
        public bool factionGather;
        public bool faction1GET = false;
        public bool faction2GET = false;
        public List<string> faction1 = new List<string>();
        public List<string> faction2 = new List<string>();
        public override void Initialize() {
            if (!Program.config.killTrackerBot || Program.config.killTrackerChannelID == 0 || Program.config.killTrackerCooldown < 1 || Program.config.killTrackerFactions == "null|null")
                return;
            killTrackers();
        }
        public async void killTrackers() {
            await Task.Delay(4500);
            while (true) {
                if (!Program.config.killTrackerBot || Program.config.killTrackerChannelID == 0 || Program.config.killTrackerCooldown < 1 || Program.config.killTrackerFactions == "null|null") 
                    return;
                faction1.Clear();
                faction2.Clear();
                Program.oBot.sendText($"/f who {Program.config.killTrackerFactions.Split('|')[0]}");
                faction1GET = true;
                await Task.Delay(2500);
                faction1GET = false;
                Program.oBot.sendText($"/f who {Program.config.killTrackerFactions.Split('|')[1]}");
                faction2GET = true;
                await Task.Delay(TimeSpan.FromMinutes(Program.config.killTrackerCooldown));
            }
        }
        public override void GetText(string text) {
            text = GetVerbatim(text).Replace("Online", "offline:").Replace("Offline", "offline:").Replace("Offline:", "offline:").Replace("Online:", "offline:");
            handleText(text);
            var k = matcher.match(text, Program.serverConfig.killFormat);
            if (k.Count > 0 && text.Contains("slain")) {
                foreach (string str in k) {
                    if (EasyMatch.isMatchValid(str, matcher)) {
                        if (str.Split('|')[0] == "killer") {
                            player = str.Split('|')[1];
                        }
                        if (str.Split('|')[0] == "died") {
                            player2 = str.Split('|')[1];
                        }
                        if (str.Split('|')[0] == "rare") {
                            rare = str.Split('|')[1];
                        }
                        if (str.Split('|')[0] == "weapon") {
                            weapon = str.Split('|')[1];
                            if (faction1.Contains(player)) {
                                (Discord.Bot.bot.GetChannel(Program.config.killTrackerChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("", $"**{player}** has killed: **{player2}** | `{Program.config.killTrackerFactions.Split('|')[0]}`"));
                                var p = oKillPlayers.getPlayer(player);
                                p.kills = p.kills + 1;
                                JSON.saveKillPlayers(Program.kplayers);
                            }
                            if (faction2.Contains(player)) {
                                (Discord.Bot.bot.GetChannel(Program.config.killTrackerChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("", $"**{player2}** has died to: **{player}** | `{Program.config.killTrackerFactions.Split('|')[0]}`"));
                                var p = oKillPlayers.getPlayer(player);
                                p.kills = p.kills + 1;
                                JSON.saveKillPlayers(Program.kplayers);
                            }
                            if (faction1.Contains(player2)) {
                                (Discord.Bot.bot.GetChannel(Program.config.killTrackerChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("", $"**{player}** has killed: **{player2}** | `{Program.config.killTrackerFactions.Split('|')[1]}`"));
                                var p = oKillPlayers.getPlayer(player);
                                p.deaths = p.deaths + 1;
                                JSON.saveKillPlayers(Program.kplayers);
                            }
                            if (faction2.Contains(player2)) {
                                (Discord.Bot.bot.GetChannel(Program.config.killTrackerChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("", $"**{player2}** has died to: **{player}** | `{Program.config.killTrackerFactions.Split('|')[1]}`"));
                                var p = oKillPlayers.getPlayer(player);
                                p.deaths = p.deaths + 1;
                                JSON.saveKillPlayers(Program.kplayers);
                            }
                        }
                    }
                }
            }
            if (text.ToLower().Contains("online:") && text.Contains("("))
                factionGather = true;
            if (text.ToLower().Contains("offline:") && text.Contains("("))
                factionGather = false;
        }
        public void handleText(string text) {
            if (factionGather && faction1GET) {
                List<string> track = text.Split(Program.config.factionSplitChar).ToList();
                string x = "";
                foreach (string str in track) {
                    x = x + "\n" + str.Replace("*** ", "").Replace("**", "").Replace("*", "").Replace("+", "").Replace("-", "").Trim();
                }
                List<string> format = x.Split('\n').ToList();
                foreach (string str2 in format) {
                    if (!str2.Contains("none") && !str2.Contains("(") && !Program.kplayers.Contains(oKillPlayers.getPlayer(Program.auto.removeTagFromPlayer(str2)))) {
                        faction1.Add(Program.auto.removeTagFromPlayer(str2));
                        Program.kplayers.Add(new oKillPlayers(0, 0, Program.auto.removeTagFromPlayer(str2), Program.config.killTrackerFactions.Split('|')[0]));
                        JSON.saveKillPlayers(Program.kplayers);
                    }
                }
            }
            if (factionGather && faction2GET) {
                List<string> track = text.Split(Program.config.factionSplitChar).ToList();
                string x = "";
                foreach (string str in track) {
                    x = x + "\n" + str.Replace("*** ", "").Replace("**", "").Replace("*", "").Replace("+", "").Replace("-", "").Trim();
                }
                List<string> format = x.Split('\n').ToList();
                foreach (string str2 in format) {
                    if (!str2.Contains("none") && !str2.Contains("(") && !Program.kplayers.Contains(oKillPlayers.getPlayer(Program.auto.removeTagFromPlayer(str2)))) {
                        faction2.Add(Program.auto.removeTagFromPlayer(str2));
                        Program.kplayers.Add(new oKillPlayers(0, 0, Program.auto.removeTagFromPlayer(str2), Program.config.killTrackerFactions.Split('|')[1]));
                        JSON.saveKillPlayers(Program.kplayers);
                    }
                }
            }
        }
    }
}
