using Discord;
using MinecraftClient;
using orbitFactionBot.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace orbitFactionBot.Bots {
    public class Ftop : ChatBot {
        public string pos = "";
        public string factions = "";
        public string value = "";
        public string change = "";
        public string seconds;
        public string faction = "";
        public bool baltop = false;
        EasyMatch matcher = new EasyMatch("{", "}");
        public bool ftopMessage(string text) {
            if (Program.ftop.baltop)
                return false;
            string[] strArray = text.Split(' ');
            return text.Contains("$") && (strArray[0].Contains("1") && !strArray[0].Contains("$") || strArray[0].Contains("2") && !strArray[0].Contains("$") || (strArray[0].Contains("3") && !strArray[0].Contains("$") || strArray[0].Contains("4") && !strArray[0].Contains("$")) || (strArray[0].Contains("5") && !strArray[0].Contains("$") || strArray[0].Contains("6") && !strArray[0].Contains("$") || (strArray[0].Contains("7") && !strArray[0].Contains("$") || strArray[0].Contains("8") && !strArray[0].Contains("$"))) || (strArray[0].Contains("9") && !strArray[0].Contains("$") || strArray[0].Contains("10") && !strArray[0].Contains("$")));
        }
        public override void Update() {
            if (seconds == DateTime.Now.ToString("mm"))
                return;
            if (int.Parse(DateTime.Now.ToString("mm")) % Program.config.ftopCooldown == 0) {
                Program.ftop.pos = null;
                Program.ftop.faction = null;
                Program.ftop.value = null;
                Program.ftop.change = null;
                Program.oBot.sendText($"{Program.config.ftopCommand}");
                Program.ftop.ftopCompiler(Program.config.ftopChannelID);
            }
            seconds = DateTime.Now.ToString("mm");
        }
        public async void ftopCompiler(ulong id) {
            string facs = "";
            string values = "";
            string changes = "";
            await Task.Delay(1000);
            try
            {
                if (this.pos == "" || this.faction == "" || this.value == "" || this.change == "")
                    return;
                List<string> pos = Program.ftop.pos.Split('\n').ToList();
                List<string> factions = Program.ftop.factions.Split('\n').ToList();
                List<string> value = Program.ftop.value.Split('\n').ToList();
                List<string> change = Program.ftop.change.Split('\n').ToList();
                var zip = pos.Zip(factions, (p, f) => new { p, f }).Zip(value, (t, v) => new { pos = t.p, factions = t.f, value = v });
                foreach (var vara in zip)
                {
                    facs = facs + $"{vara.pos} {vara.factions}" + "\n";
                    values = values + vara.value + "\n";
                }
                foreach (string str in change)
                {
                    if (str.Replace("\n", "").Length != 0)
                    {
                        if (str == "0" || str == "1" || str == "2" || str == "3" || str == "4" || str == "5" || str == "5" || str == "6" || str == "7" || str == "8" || str == "9")
                            changes = changes + $"${str}" + "\n";
                        else changes = changes + $"${Program.ftop.ToKMB(decimal.Parse(str.Split(' ')[0]))} {str.Split(' ')[1]}" + "\n";
                    }
                }
                if (string.IsNullOrEmpty(facs.Replace("\n", "")) || string.IsNullOrWhiteSpace(facs.Replace("\n", "")))
                    facs = "null";
                if (string.IsNullOrEmpty(values.Replace("\n", "")) || string.IsNullOrWhiteSpace(values.Replace("\n", "")))
                    values = "null";
                if (string.IsNullOrEmpty(changes.Replace("\n", "")) || string.IsNullOrWhiteSpace(changes.Replace("\n", "")))
                    changes = "null";
                await (Discord.Bot.bot.GetChannel(id) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbedFtop($"FactionTop | page: 1", "**" + facs + "**", values, changes));
                Program.ftop.pos = null;
                Program.ftop.factions = null;
                Program.ftop.value = null;
                Program.ftop.change = null;
            }
            catch (Exception e) {
                ConsoleIO.WriteLine(e.StackTrace);
            }
        }
        public override void GetText(string text) {
            text = GetVerbatim(text).Replace("  ", "");
            var matches = matcher.match(text, Program.serverConfig.ftopFormat);
            if (matches.Count > 0) {
                foreach (string str in matches) {
                    if (EasyMatch.isMatchValid(str, matcher) && Program.ftop.ftopMessage(text)) {
                        if (str.Split('|')[0] == "pos") {
                            pos = pos + str.Split('|')[1].Replace(".", "") + "\n";
                        }
                        if (str.Split('|')[0] == "faction") {
                            if (!oFtop.factionExist(str.Split('|')[1].Replace("-", ""))) {
                                Program.ftops.Add(new oFtop(0, str.Split('|')[1].Replace("-", "")));
                                JSON.saveFtop(Program.ftops);
                                faction = str.Split('|')[1].Replace("-", "");
                            }
                            faction = str.Split('|')[1].Replace("-", "");
                            factions = factions + str.Split('|')[1].Replace("-", "") + "\n";
                        }
                        if (str.Split('|')[0] == "value") {
                            if (Settings.ServerIP.Contains("cffqs394yu897")) {

                            }
                            oFtop ftop = oFtop.getFtop(faction);
                            string toParse = replaceChars(str.Split('|')[1]);
                            if (ulong.Parse(toParse) == ftop.currentValue) {
                                change = change + $"0 **[{CalculateChange(long.Parse(ftop.currentValue.ToString()), long.Parse(toParse))}]**" + "\n";
                            }
                            if (ulong.Parse(toParse) > ftop.currentValue) {
                                change = change + $"+{ulong.Parse(toParse) - ftop.currentValue} **[{CalculateChange(long.Parse(ftop.currentValue.ToString()), long.Parse(toParse))}]**" + "\n";
                            }
                            if (ftop.currentValue > ulong.Parse(toParse)) {
                                change = change + $"-{ftop.currentValue - ulong.Parse(toParse)} **[{CalculateChange(long.Parse(ftop.currentValue.ToString()), long.Parse(toParse))}]**" + "\n";
                            }
                            value = value + str.Split('|')[1] + "\n";
                            ftop.currentValue = ulong.Parse(toParse);
                            JSON.saveFtop(Program.ftops);
                            faction = null;
                        }
                    }
                }
            }
        }
        public string CalculateChange(long previous, long current) {
            return ((current - previous) / (double)previous).ToString("p");
        }
        public string replaceChars(string str) {
            return str.Replace("B", "").Replace("M", "").Replace("K", "").Replace(".", "").Replace("$", "").Replace(",", "");
        }
        public string ToKMB(decimal num) {
            if (num > 999999999 || num < -999999999)
                return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
            else
            if (num > 999999 || num < -999999)
                return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
            else
            if (num > 999 || num < -999)
                return num.ToString("0,.#K", CultureInfo.InvariantCulture);
            else  return num.ToString(CultureInfo.InvariantCulture);
        }
    }
}
