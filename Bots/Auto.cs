using Discord;
using MinecraftClient;
using orbitFactionBot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace orbitFactionBot.Bots {
    public class Auto : ChatBot {
        public string seconds;
        public string chatData;
        public ulong chatDataID = 0;
        public bool sendChatData = false;
        public string embedTitle;
        EasyMatch matcher = new EasyMatch("{", "}");
        public bool factionGather = false;
        public string layerLayers = "";
        public string layerUsernames = "";
        public bool layerTracker = false;
        public List<string> checker = new List<string>();
        public override void Update() {
            if (seconds == DateTime.Now.ToString("ss"))
                return;
            if (int.Parse(DateTime.Now.ToString("ss")) % 4 == 0) {
                if (string.IsNullOrWhiteSpace(chatData))
                    return;
                if (sendChatData)
                    (Discord.Bot.bot.GetChannel(Program.oBot.chatDataID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed(embedTitle, $"```{Program.config.embedCSS}{chatData}```"));
                chatData = "";
                sendChatData = false;
            }
            seconds = DateTime.Now.ToString("ss");
            if (int.Parse(DateTime.Now.ToString("ss")) % 30 == 0) {
                ConsoleIO.WriteLine("updated");
                checker = GetOnlinePlayers().ToList();
            }
            seconds = DateTime.Now.ToString("ss");
        }
        public override void GetText(string text) {
            if (!string.IsNullOrEmpty(text) || !string.IsNullOrWhiteSpace(text)) {
                text = GetVerbatim(text).Replace("Online", "offline:").Replace("Offline", "offline:").Replace("Offline:", "offline:").Replace("Online:", "offline:");
                chatData = chatData + "\n" + text;
                if (layerTracker) {
                    if (text.ToLower().Contains("online:") && text.Contains("(")) {
                        if (text.Split(',').Count() >= 1) {
                            string lilboat = text.Split(':')[2];
                            List<string> track = lilboat.Split(Program.config.factionSplitChar).ToList();
                            string x = "";
                            foreach (string str in track) {
                                x = x + "\n" + str.Replace("*** ", "").Replace("**", "").Replace("*", "").Replace("+", "").Replace("-", "").Trim();
                            }
                            List<string> format = x.Split('\n').ToList();
                            foreach (string str2 in format) {
                                var matches = matcher.match(str2, Program.config.layerTrackerFormat);
                                if (matches.Count > 0) {
                                    foreach (string str in matches) {
                                        if (str.Split('|')[0].Replace(":", "") == "layer" && str.Split('|')[1] != removeTagFromPlayer(str2) && IsDigitsOnly(str.Split('|')[1])) {
                                            layerLayers = layerLayers + replaceChar(str.Split('|')[1]) + "\n";
                                            ConsoleIO.WriteLine(layerLayers);
                                        }
                                        if (str.Split('|')[0].Replace(":", "") == "player") {
                                            layerUsernames = layerUsernames + str.Split('|')[1] + "\n";
                                            ConsoleIO.WriteLine(layerUsernames);
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            factionGather = true;
                            handleText(text);
                        }
                    }
                    if (text.ToLower().Contains("offline:") && text.Contains("(")) {
                        factionGather = false;
                    }
                }
            }
        }
        public bool playerHasTag(string str) {
            if (str.Split(' ').Count() > 1)
                return true;
            return false;
        }
        public string removeTagFromPlayer(string str) {
            if (playerHasTag(str)) {
                return str.Split(' ')[str.Split(' ').Count() - 1];
            }
            return str;
        }
        public string replaceChar(string str) {
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(str, "");
        }
        public bool IsDigitsOnly(string str) {
            foreach (char c in str) {
                if (c < '0' || c > '9')
                    return false;
            } return true;
        }
        public void handleText(string text) {
            if (Program.auto.factionGather) {
                List<string> track = text.Split(Program.config.factionSplitChar).ToList();
                string x = "";
                foreach (string str in track) {
                    x = x + "\n" + str.Replace("*** ", "").Replace("**", "").Replace("*", "").Replace("+", "").Replace("-", "").Trim();
                }
                List<string> format = x.Split('\n').ToList();
                foreach (string str2 in format) {
                    var matches = matcher.match(str2, Program.config.layerTrackerFormat);
                    if (matches.Count > 0) {
                        foreach (string str in matches) {
                            if (str.Split('|')[0].Replace(":", "") == "layer" && str.Split('|')[1] != removeTagFromPlayer(str2)) {
                                layerLayers = layerLayers + replaceChar(str.Split('|')[1]) + "\n";
                                ConsoleIO.WriteLine(layerLayers);
                            }
                            if (str.Split('|')[0].Replace(":", "") == "player") {
                                layerUsernames = layerUsernames + str.Split('|')[1] + "\n";
                                ConsoleIO.WriteLine(layerUsernames);
                            }
                        }
                    }
                }
            }
        }
        public override void Initialize() {
            Program.auto.autoFwho();
            Program.auto.autoFlist();
            Program.auto.autoLayer();
            Program.auto.claimTracker();
        }
        public async void claimTracker() {
            await Task.Delay(10000);
            if (!Program.config.claimTrackerBot || Program.config.claimTrackerChannelID == 0)
                return;
            while (true) {
                Program.oBot.sendText("/f list");
                claimTrackerSend();
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
        public async void claimTrackerSend() {
            return;
            if (!Program.config.claimTrackerBot || Program.config.claimTrackerChannelID == 0)
                return;
            await Task.Delay(2500);
            await (Discord.Bot.bot.GetChannel(Program.config.claimTrackerChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Claim Update | [{Program.oBot.claimtosend.Split('\n').Count() - 1}]", $"{Program.oBot.claimtosend}"));
            Program.oBot.claimtosend = null;
        }
        public async void autoFwho() {
            await Task.Delay(10000);
            if (!Program.config.autoFWhoBot || Program.config.autoFWhoChannelID == 0 || Program.config.autoFWhoCooldown < 30 || Program.config.autoFWhoFaction == "N/A" || Program.config.autoFWhoFaction == "")
                return;
            while (true) {
                Program.oBot.chatData = "";
                Program.oBot.embedTitle = $"AutoFWho | {Program.config.autoFWhoFaction}";
                Program.oBot.sendText("/f who " + Program.config.autoFWhoFaction);
                Program.oBot.sendChatData = true;
                Program.oBot.chatDataID = Program.config.autoFWhoChannelID;
                await Task.Delay(TimeSpan.FromSeconds(Program.config.autoFWhoCooldown));
            }
        }
        public async void autoFlist() {
            await Task.Delay(10000);
            if (!Program.config.autoFListBot || Program.config.autoFListChannelID == 0 || Program.config.autoFListCooldown < 30)
                return;
            while (true) {
                Program.oBot.chatData = "";
                Program.oBot.embedTitle = $"AutoFList | page: 1";
                Program.oBot.sendText("/f list");
                Program.oBot.sendChatData = true;
                Program.oBot.chatDataID = Program.config.autoFListChannelID;
                await Task.Delay(TimeSpan.FromSeconds(Program.config.autoFListCooldown));
            }
        }
        public async void autoLayer() {
            await Task.Delay(10000);
            if (!Program.config.layerTrackerBot && Program.config.layerTrackerChannelID == 0 && Program.config.layerTrackerCooldown == 0)
                return;
            while (true) {
                layerTracker = true;
                Program.oBot.sendText("/f who");
                returnHandler("layer");
                await Task.Delay(TimeSpan.FromMinutes(Program.config.layerTrackerCooldown));
            }
        }
        public async void returnHandler(string call) {
            await Task.Delay(2500);
            if (call == "layer") {
                if (layerLayers == "" || layerUsernames == "") {
                    await (Discord.Bot.bot.GetChannel(Program.config.layerTrackerChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Layer Tracker [0]", $"```null```"));
                }
                else {
                    var st = layerLayers.Split('\n').ToList();
                    var st1 = layerUsernames.Split('\n').ToList();
                    string tosend = "";
                    foreach (var vara in st.Zip(st1, (x, x1) => new { st = x, st1 = x1 })) {
                        if (vara.st != "" && vara.st1 != "")
                            tosend = tosend + $"{vara.st} - {vara.st1}" + "\n";
                    }
                    await (Discord.Bot.bot.GetChannel(Program.config.layerTrackerChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Layer Tracker [{st.Count() - 1}]", $"```{tosend}```"));
                    layerLayers = null;
                    layerUsernames = null;
                }
            }
        }
    }
}
