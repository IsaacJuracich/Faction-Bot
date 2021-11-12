using Discord;
using Discord.WebSocket;
using MinecraftClient;
using MinecraftClient.Mapping;
using MinecraftClient.Protocol;
using MinecraftClient.Protocol.Handlers; 
using MinecraftClient.Protocol.Handlers.PacketPalettes;
using Newtonsoft.Json;
using orbitFactionBot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.InteropServices;
using Ionic.Zip;
using Org.BouncyCastle.Crypto.Utilities;
using System.Runtime.Remoting.Messaging;
using System.Net;
using MinecraftClient.Inventory;

namespace orbitFactionBot.Bots {
    public class OrbitBot : ChatBot {
        EasyMatch matcher = new EasyMatch("{", "}");
        public string seconds;
        public string chatData;
        public ulong chatDataID = 0;
        public bool sendChatData = false;
        public string embedTitle;
        public bool checkShields = false;
        public string shieldCheck;
        public string fwhoFaction;
        public bool userRequest = false;
        public SocketUser user;
        public ReplayHandler replay;
        public ReplayHandler replayClip;
        public bool baseRegion = false;
        public ulong baseRegionID = 0;
        public string tosend;
        public string fac = "";
        public string claims = "";
        public string claimtosend;
        public string ping;
        Location floatf = new Location(0, 0, 0);
        public string lText;
        public override void Update() {
            if (seconds == DateTime.Now.ToString("ss"))
                return;
            if (int.Parse(DateTime.Now.ToString("ss")) % 1 == 0) {
                Program.players = JsonConvert.DeserializeObject<List<oPlayers>>(File.ReadAllText("!data\\players.json"));
                Program.config = JsonConvert.DeserializeObject<oSettings>(File.ReadAllText("config.json"));
                Program.ftops = JsonConvert.DeserializeObject<List<oFtop>>(File.ReadAllText("!data\\ftop.json"));
                if (Program.config.leaderBoardUpdates) {
                    foreach (string str in Program.config.leaderBoardsToUpdate) {
                        if (str == "walls") {
                            List<oPlayers> sorted = Program.players.OrderBy(x => x.wallChecks).ToList();
                            sorted.Reverse();
                            
                        }
                        if (str == "buffers") {
                            List<oPlayers> sorted = Program.players.OrderBy(x => x.bufferChecks).ToList();
                            sorted.Reverse();

                        }
                        if (str == "rpost") {
                            List<oPlayers> sorted = Program.players.OrderBy(x => x.rpostChecks).ToList();
                            sorted.Reverse();

                        }
                        if (str == "deposit") {
                            List<oPlayers> sorted = Program.players.OrderBy(x => x.depositAmount).ToList();
                            sorted.Reverse();

                        }
                        if (str == "withdraw") {
                            List<oPlayers> sorted = Program.players.OrderBy(x => x.withdrawAmount).ToList();
                            sorted.Reverse();

                        }
                    }
                }
            }
            seconds = DateTime.Now.ToString("ss");
            if (int.Parse(DateTime.Now.ToString("ss")) % 4 == 0) {
                if (string.IsNullOrWhiteSpace(chatData))
                    return;
                if (userRequest) {
                    if (Program.ftop.baltop)
                        Program.ftop.baltop = false;
                    user.SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed(embedTitle, $"```{Program.config.embedCSS}{chatData}```"));
                    userRequest = false;
                    user = null;
                }
                if (sendChatData) {
                    (Discord.Bot.bot.GetChannel(Program.oBot.chatDataID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed(embedTitle, $"```{Program.config.embedCSS}{chatData}```"));
                }
                chatData = "";
                sendChatData = false;
            }
            seconds = DateTime.Now.ToString("ss");
        }
        public override void OnNetworkPacket(int packetID, List<byte> packetData, bool isLogin, bool isInbound) {
            if (replay != null)
                replay.AddPacket(packetID, packetData, isLogin, isInbound);
            if (replayClip != null)
                replayClip.AddPacket(packetID, packetData, isLogin, isInbound);

        }
        public async void captchaSender(string code) {
            await Task.Delay(1000);
            SendText(code);
            await Task.Delay(1000);
            SendText(code);
        }
        public override void GetText(string text) {
            if (!string.IsNullOrEmpty(text) || !string.IsNullOrWhiteSpace(text) || !text.ToLower().Contains("queue")) {
                string chatColorText = text;
                text = GetVerbatim(text);
                string player = null;
                string message = null;
                chatData = chatData + "\n" + text;
                lText = text;
                if (baseRegion) {
                    if (chatColorText.Contains("§6")) {
                        tosend = tosend + text + "\n";
                    }
                }
                if (text.ToLower().StartsWith("ping average") && text.Contains(":")) {
                    ping = text.Split(' ')[2];
                    Program.oBot.sendText($"[Misc] Current Ping: {ping} | TPS: {GetServerTPS().ToString().Split('.')[0]}");
                    ping = null;
                }
                if (GetOnlinePlayers().Contains(text.Split(' ')[0].Replace(":", "").Replace("*", "").Replace("+", "").Replace("-", "")) && text.Split(' ')[0].Contains(":")) {
                    player = text.Split(' ')[0].Replace(":", "").Replace("*", "").Replace("+", "").Replace("-", "");
                    message = text.Split(':')[1];
                    commandHandler(player, message);
                }
                if (checkShields && text.ToLower().Contains("shield")) {
                    var m = matcher.match(text, Program.serverConfig.checkShieldsFormat);
                    if (m.Count > 0) {
                        foreach (string str in m) {
                            if (EasyMatch.isMatchValid(str, matcher)) {
                                if (str.Split('|')[0].Trim() == "status")
                                    shieldCheck = shieldCheck + str.Split('|')[1] + "\n";
                            }
                        }
                    }
                }
                var m2 = matcher.match(text, Program.serverConfig.captchaFormat);
                if (m2.Count > 0 && Program.config.captchaLogChannelID != 0 && text.Split(' ')[0] == Program.serverConfig.captchaFormat.Split(' ')[0]) {
                    foreach (string str in m2) {
                        if (EasyMatch.isMatchValid(str, matcher) && text.ToLower().Contains("type")) {
                            if (str.Split('|')[0] == "code" && str.Split('|')[1].ToLower() != "another") {
                                if (str.Split('|')[1].Contains("request"))
                                    return;
                                Stopwatch sw = new Stopwatch();
                                sw.Start();
                                Program.oBot.captchaSender(str.Split('|')[1]);
                                (Discord.Bot.bot.GetChannel(Program.config.captchaLogChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("CaptchaSolved", $"Captcha Solved in **{sw.Elapsed.TotalSeconds}** seconds | captchaCode: **`{str.Split('|')[1]}`**"));
                            }
                        }
                    }
                }
                var m3 = matcher.match(text, Program.serverConfig.openFactionFlagFormat);
                if (text.ToLower().Contains("flags")) {
                    if (m3.Count > 0 && Program.config.openFactionFlagBot && Program.config.openFactionFlagChannelID != 0 && Program.serverConfig.openFactionFlagFormat != "N/A" || Program.serverConfig.openFactionFlagFormat != "") {
                        foreach (string str in m3) {
                            if (EasyMatch.isMatchValid(str, matcher)) {
                                if (str.Split('|')[0] == "check") {
                                    int arrayID = 0;
                                    foreach (string x in Program.serverConfig.openFactionFlagFormat.Split(' ')) {
                                        if (!x.StartsWith(matcher.chars) && !x.EndsWith(matcher.chars1))
                                            arrayID++;
                                        else break;
                                    }
                                    if (chatColorText.Split(' ')[arrayID].Contains("§a")) {
                                        if (Program.config.openFactionAutoJoin)
                                            sendText("/f join " + Program.oBot.fwhoFaction);
                                        (Discord.Bot.bot.GetChannel(Program.config.openFactionFlagChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"FactionOpen | {Program.oBot.fwhoFaction}", $"**__{Program.oBot.fwhoFaction}__** Has left their factionFlag **`open`** | Depending on settings, I have attempted to **`join`**"));
                                    }
                                }
                            }
                        }
                    }
                }
                if (text.ToLower().Contains("faction.")) {
                    var m4 = matcher.match(text, Program.serverConfig.bankFormat);
                    if (m4.Count > 0 && Program.config.bankBot && Program.config.bankLogChannelID != 0 && Program.config.bankLogMessageIG != "" && Program.config.bankLogMessageDC != "" && Program.serverConfig.bankFormat != "N/A" || Program.serverConfig.bankFormat != "") {
                        int count = 0;
                        string bplayer = null;
                        string btype = null;
                        string bamount = null;
                        foreach (string str in m4) {
                            if (EasyMatch.isMatchValid(str, matcher)) {
                                count++;
                                if (str.Split('|')[0] == "player") {
                                    bplayer = str.Split('|')[1].Replace("You", GetUsername());
                                }
                                if (str.Split('|')[0] == "type") {
                                    btype = str.Split('|')[1];
                                }
                                if (str.Split('|')[0] == "amount") {
                                    bamount = str.Split('|')[1];
                                }
                                if (count == m4.Count()) {
                                    oPlayers x = oPlayers.getPlayer(bplayer);
                                    if (btype == "gave") {
                                        x.depositAmount = x.depositAmount + ulong.Parse(bamount.Replace("$", "").Replace(",", "").Replace(".", ""));
                                        JSON.savePlayers(Program.players);
                                    }
                                    if (btype == "took") {
                                        x.withdrawAmount = x.withdrawAmount + ulong.Parse(bamount.Replace("$", "").Replace(",", "").Replace(".", ""));
                                        JSON.savePlayers(Program.players);
                                    }
                                    (Discord.Bot.bot.GetChannel(Program.config.bankLogChannelID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("BankAlert", Program.config.bankLogMessageDC.Replace("[player]", bplayer).Replace("[discord.mention]", $"{Discord.Bot.bot.GetUser(x.discordID).Mention}").Replace("[discord.fullname]", $"{Discord.Bot.bot.GetUser(x.discordID)}").Replace("[discord.name]", $"{Discord.Bot.bot.GetUser(x.discordID).Username}").Replace("[amount]", bamount.Replace("$", "")).Replace("[type]", btype)));
                                    sendText(Program.config.bankLogMessageIG.Replace("[player]", bplayer).Replace("[discord.mention]", $"{Discord.Bot.bot.GetUser(x.discordID).Mention}").Replace("[discord.fullname]", $"{Discord.Bot.bot.GetUser(x.discordID)}").Replace("[discord.name]", $"{Discord.Bot.bot.GetUser(x.discordID).Username}").Replace("[amount]", bamount.Replace("$", "")).Replace("[type]", btype));
                                }
                            }
                        }
                    }
                }
                if (text.Contains("/") && text.ToLower().Contains("online")) {
                    var m5 = matcher.match(text, Program.serverConfig.claimFormat);
                    if (m5.Count > 0 && Program.config.claimTrackerBot && Program.config.claimTrackerChannelID != 0) {
                        foreach (string str in m5) {
                            if (EasyMatch.isMatchValid(str, matcher)) {
                                if (str.Split('|')[0] == "faction") {
                                    fac = str.Split('|')[1];
                                }
                                if (str.Split('|')[0] == "track") {
                                    claims = str.Split('|')[1];
                                }
                                if (str.Split('|')[0] == "max") {
                                    if (!oClaims.factionExist(fac)) {
                                        Program.claims.Add(new oClaims(0, fac));
                                        JSON.saveClaims(Program.claims);
                                    }
                                    oClaims c = oClaims.getClaims(fac);
                                    if (ulong.Parse(claims) == c.currentValue)
                                        claimtosend = claimtosend + $"**{fac}**'s claims haven't *`changed`*" + "\n";
                                    if (ulong.Parse(claims) > c.currentValue)
                                        claimtosend = claimtosend + $"**{fac}**'s claims have went up: *`{ulong.Parse(claims) - c.currentValue}`*" + "\n";
                                    if (c.currentValue > ulong.Parse(claims))
                                        claimtosend = claimtosend + $"**{fac}**'s claims have went down: *`{c.currentValue - ulong.Parse(claims)}`*" + "\n";
                                    c.currentValue = ulong.Parse(claims);
                                    JSON.saveClaims(Program.claims);
                                    fac = null;
                                    claims = null;
                                }
                            }
                        }
                    }
                }
            }
        }
        public void commandHandler(string player, string chat) {
            if (chat.Split(' ').Count() == 2) {
                string message = chat.Replace(" ", "");
                if (message.StartsWith("ob-")) {
                    foreach (oPlayers x in Program.players) {
                        if (x.playerName == "" && x.whitelistCode == message) {
                            x.playerName = player;
                            JSON.savePlayers(Program.players);
                            Program.oBot.sendText($"{Program.config.whitelistMessageIG.Replace("[discord.fullname]", $"{Discord.Bot.bot.GetUser(x.discordID)}").Replace("[discord.mention]", $"{Discord.Bot.bot.GetUser(x.discordID).Mention}").Replace("[discord.name]", $"{Discord.Bot.bot.GetUser(x.discordID)}").Replace("[player]", player)}");
                            (Discord.Bot.bot.GetChannel(Program.config.whitelistChannel) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Player Whitelisted", $"{Program.config.whitelistMessageDC.Replace("[discord.fullname]", $"{Discord.Bot.bot.GetUser(x.discordID)}").Replace("[discord.mention]", $"{Discord.Bot.bot.GetUser(x.discordID).Mention}").Replace("[discord.name]", $"{Discord.Bot.bot.GetUser(x.discordID)}").Replace("[player]", player)}"));
                        }
                    }
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}rposttop")) {
                    List<oPlayers> sorted = Program.players.OrderBy(x => x.rpostChecks).ToList();
                    sorted.Reverse();
                    int count = 0;
                    string tosend = null;
                    foreach (oPlayers players in sorted) {
                        if (count == 5) break;
                        count++;
                        if (players.rpostChecks > 0 && players.playerName != "")
                            tosend = tosend + $"#[{count}] {players.playerName} - {players.rpostChecks}" + " | ";
                    }
                    if (tosend == null)
                        sendText("No Data Gathered");
                    else sendText(tosend);
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}walltop")) {
                    List<oPlayers> sorted = Program.players.OrderBy(x => x.wallChecks).ToList();
                    sorted.Reverse();
                    int count = 0;
                    string tosend = null;
                    foreach (oPlayers players in sorted) {
                        if (count == 5) break;
                        count++;
                        if (players.wallChecks > 0 && players.playerName != "")
                            tosend = tosend + $"#[{count}] {players.playerName} - {players.wallChecks}" + " | ";
                    }
                    if (tosend == null)
                        sendText("No Data Gathered");
                    else sendText(tosend);
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}buffertop")) {
                    List<oPlayers> sorted = Program.players.OrderBy(x => x.bufferChecks).ToList();
                    sorted.Reverse();
                    int count = 0;
                    string tosend = null;
                    foreach (oPlayers players in sorted) {
                        if (count == 5) break;
                        count++;
                        if (players.bufferChecks > 0 && players.playerName != "")
                            tosend = tosend + $"#[{count}] {players.playerName} - {players.bufferChecks}" + " | ";
                    }
                    if (tosend == null)
                        sendText("No Data Gathered");
                    else sendText(tosend);
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}bankdtop")) {
                    List<oPlayers> sorted = Program.players.OrderBy(x => x.depositAmount).ToList();
                    sorted.Reverse();
                    int count = 0;
                    string tosend = null;
                    foreach (oPlayers players in sorted) {
                        if (count == 5) break;
                        count++;
                        if (players.depositAmount > 0 && players.playerName != "")
                            tosend = tosend + $"#[{count}] {players.playerName} - {players.depositAmount}" + " | ";
                    }
                    if (tosend == null)
                        sendText("No Data Gathered");
                    else sendText(tosend);
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}bankwtop")) {
                    List<oPlayers> sorted = Program.players.OrderBy(x => x.withdrawAmount).ToList();
                    sorted.Reverse();
                    int count = 0;
                    string tosend = null;
                    foreach (oPlayers players in sorted) {
                        if (count == 5) break;
                        count++;
                        if (players.withdrawAmount > 0 && players.playerName != "")
                            tosend = tosend + $"#[{count}] {players.playerName} - {players.withdrawAmount}" + " | ";
                    }
                    if (tosend == null)
                        sendText("No Data Gathered");
                    else sendText(tosend);
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}flist")) {
                    Program.oBot.chatData = "";
                    Program.oBot.embedTitle = $"InGame Request | factionList";
                    Program.oBot.sendText("/f list");
                    Program.oBot.userRequest = true;
                    Program.oBot.user = Discord.Bot.bot.GetUser(oPlayers.getPlayer(player).discordID);
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}baltop")) {
                    Program.oBot.chatData = "";
                    Program.oBot.embedTitle = $"InGame Request | balanceTop";
                    Program.oBot.sendText("/baltop");
                    Program.oBot.userRequest = true;
                    Program.oBot.user = Discord.Bot.bot.GetUser(oPlayers.getPlayer(player).discordID);
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}claim")) {
                    if (!Program.config.archonClaimBot || Program.config.archonClaimAmount == 0) {
                        sendText($"[ArchonClaimBot] Invalid Settings | {player}");
                        return;
                    }
                    sendText($"/tpa {player}");
                    sendText($"[ArchonClaimBot] Tping to {player}");
                    ArchonClaimer(player);
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}ping")) {
                    sendText("/ping");
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}yawandpitch")) {
                    Program.oBot.sendText($"[Misc] YawAngle: {GetYaw()} | PitchAngle: {GetPitch()}");
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}vanish")) {
                    sendText($"[VanishBot] Fetching vanished players");
                    vanishTracker("ig", 0);
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}walls")) {
                    if (Program.checker.cooldownCheck("!data/walls.txt", Program.config.wallCooldown)) {
                        return;
                    }
                    if (!oPlayers.playerExist2(player))
                        return;
                    oPlayers x = oPlayers.getPlayer(player);
                    x.wallChecks = x.wallChecks + 1;
                    JSON.savePlayers(Program.players);
                    sendText($"[WallBot] | Walls have been checked by: {player} | wallAmount: {x.wallChecks}");
                    File.WriteAllText("!data/walls.txt", player);
                    var eb = new EmbedBuilder().WithColor(Program.embedColor).WithThumbnailUrl("https://mc-heads.net/avatar/" + player).AddField("**WallChecker**", player, true).AddField("**LB Position**", $"#0", true).AddField("**Wall Amount**", $"{x.wallChecks}", true);
                    (Discord.Bot.bot.GetChannel(Program.config.wallChannelID) as ITextChannel).SendMessageAsync(embed: eb.Build());
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}buffers")) {
                    ConsoleIO.WriteLine(Program.config.bufferCooldown.ToString());
                    if (Program.checker.cooldownCheck("!data/buffers.txt", Program.config.bufferCooldown)) {
                        return;
                    }
                    if (!oPlayers.playerExist2(player))
                        return;
                    oPlayers x = oPlayers.getPlayer(player);
                    x.bufferChecks = x.bufferChecks + 1;
                    JSON.savePlayers(Program.players);
                    sendText($"[BufferBot] | Buffers have been checked by: {player} | bufferAmount: {x.bufferChecks}");
                    File.WriteAllText("!data/buffers.txt", player);
                    var eb = new EmbedBuilder().WithColor(Program.embedColor).WithThumbnailUrl("https://mc-heads.net/avatar/" + player).AddField("**BufferChecker**", player, true).AddField("**LB Position**", $"#0", true).AddField("**Buffer Amount**", $"{x.bufferChecks}", true);
                    (Discord.Bot.bot.GetChannel(Program.config.bufferChannelID) as ITextChannel).SendMessageAsync(embed: eb.Build());
                }
                if (message.ToLower().Equals($"{Program.config.mcPrefix}rpost")) {
                    ConsoleIO.WriteLine(Program.config.bufferCooldown.ToString());
                    if (Program.checker.cooldownCheck("!data/rpost.txt", Program.config.rpostCooldown)) {
                        return;
                    }
                    if (!oPlayers.playerExist2(player))
                        return;
                    oPlayers x = oPlayers.getPlayer(player);
                    x.rpostChecks = x.rpostChecks + 1;
                    JSON.savePlayers(Program.players);
                    sendText($"[RpostBot] | RpostWalls have been checked by: {player} | rpostAmount: {x.rpostChecks}");
                    File.WriteAllText("!data/rpost.txt", player);
                    var eb = new EmbedBuilder().WithColor(Program.embedColor).WithThumbnailUrl("https://mc-heads.net/avatar/" + player).AddField("**RpostChecker**", player, true).AddField("**LB Position**", $"#0", true).AddField("**Rpost Amount**", $"{x.rpostChecks}", true);
                    (Discord.Bot.bot.GetChannel(Program.config.rpostChannelID) as ITextChannel).SendMessageAsync(embed: eb.Build());
                }
            }
            else {
                string message = chat.TrimStart();
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}announcement")) {
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}sneak")) {
                    Sneak(bool.Parse(message.Split(' ')[1]));
                    if (bool.Parse(message.Split(' ')[1]) == true)
                        Program.oBot.sendText($"[Misc] Bot has sneaked");
                    if (bool.Parse(message.Split(' ')[1]) == false)
                        Program.oBot.sendText($"[Misc] Bot has unsneaked");
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}move")) {
                    MinecraftClient.Mapping.Direction direction = MinecraftClient.Mapping.Direction.Up;
                    if (message.Split(' ')[1].ToLower() == "north")
                        direction = MinecraftClient.Mapping.Direction.North;
                    if (message.Split(' ')[1].ToLower() == "south")
                        direction = MinecraftClient.Mapping.Direction.South;
                    if (message.Split(' ')[1].ToLower() == "east")
                        direction = MinecraftClient.Mapping.Direction.East;
                    if (message.Split(' ')[1].ToLower() == "west")
                        direction = MinecraftClient.Mapping.Direction.West;
                    Program.client.MoveTo(Movement.Move(GetCurrentLocation(), direction));
                    Program.oBot.sendText($"[Misc] Bot has moved: {direction}");
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}look")) {
                    MinecraftClient.Mapping.Direction direction = MinecraftClient.Mapping.Direction.Up;
                    if (message.Split(' ')[1].ToLower() == "north")
                        direction = MinecraftClient.Mapping.Direction.North;
                    if (message.Split(' ')[1].ToLower() == "south")
                        direction = MinecraftClient.Mapping.Direction.South;
                    if (message.Split(' ')[1].ToLower() == "east")
                        direction = MinecraftClient.Mapping.Direction.East;
                    if (message.Split(' ')[1].ToLower() == "west")
                        direction = MinecraftClient.Mapping.Direction.West;
                    Program.client.UpdateLocation(GetCurrentLocation(), direction);
                    Program.oBot.sendText($"[Misc] Bot has looked: {direction}");
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}dig")) {
                    Location loc = new Location(double.Parse(message.Split(' ')[1]), double.Parse(message.Split(' ')[2]), double.Parse(message.Split(' ')[3]));
                    DigBlock(loc, true, true);
                    Program.oBot.sendText($"[Misc] Bot has digged at: {loc.ToString().Replace(".", ",")}");
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}changeslot")) {
                    if (int.Parse(message.Split(' ')[1]) > 9 || int.Parse(message.Split(' ')[1]) < 0) {
                        Program.oBot.sendText($"[Misc] Invalid slot number");
                        return;
                    }
                    ChangeSlot(short.Parse(message.Split(' ')[1]));
                    Program.oBot.sendText($"[Misc] Bot has changedSlot to: {message.Split(' ')[1]}");
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}fwho")) {
                    Program.oBot.chatData = "";
                    Program.oBot.embedTitle = $"InGame Request | factionShow";
                    Program.oBot.sendText($"/f who {message.Split(' ')[1]}");
                    Program.oBot.userRequest = true;
                    Program.oBot.user = Discord.Bot.bot.GetUser(oPlayers.getPlayer(player).discordID);
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}seen")) {
                    Program.oBot.chatData = "";
                    Program.oBot.embedTitle = $"InGame Request | Seen";
                    Program.oBot.sendText($"/seen {message.Split(' ')[1]}");
                    Program.oBot.userRequest = true;
                    Program.oBot.user = Discord.Bot.bot.GetUser(oPlayers.getPlayer(player).discordID);
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}strikes")) {
                    Program.oBot.chatData = "";
                    Program.oBot.embedTitle = $"InGame Request | FStrikes";
                    Program.oBot.sendText($"/f strikes {message.Split(' ')[1]}");
                    Program.oBot.userRequest = true;
                    Program.oBot.user = Discord.Bot.bot.GetUser(oPlayers.getPlayer(player).discordID);
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}wallcounter")) {
                    int radius = int.Parse(message.Split(' ')[3]) * 2;
                    string lastest = null;
                    int wallCount = 0;
                    Location from = GetCurrentLocation();
                    World world = GetWorld();
                    Location minPoint = new Location(from.X, from.Y - 1, from.Z - radius);
                    Location maxPoint = new Location(from.X, from.Y - 1, from.Z + radius);
                    for (double z = minPoint.Z; z <= maxPoint.Z; z++) {
                        if (z < from.Z) {
                            Location doneloc = new Location(minPoint.X, from.Y - 2, z);
                            Material blockType = world.GetBlock(doneloc).Type;
                            if (lastest == null) {
                                if (blockType.ToString().ToLower() == message.Split(' ')[1]) { }
                                if (blockType.ToString().ToLower() == message.Split(' ')[2]) { }
                                if (blockType.ToString().ToLower() == "sand") { }
                                else lastest = blockType.ToString().ToLower();
                            }
                            if (lastest == message.Split(' ')[2] && blockType.ToString().ToLower() == message.Split(' ')[1])
                                wallCount++;
                            if (lastest == "sand" && blockType.ToString().ToLower() == message.Split(' ')[1])
                                wallCount++;
                            lastest = blockType.ToString().ToLower();
                        }
                    }
                    if (wallCount <= 0) Program.oBot.sendText($"[WallCounter] Wallcount: No data");
                    else Program.oBot.sendText($"[WallCounter] Wallcount: {wallCount} | This doesn't include box wall.");
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}setfloat")) {
                    floatf = GetCurrentLocation();
                    sendText($"[FloatFinder] | Float bound to: {GetCurrentLocation()}");
                }
                if (message.ToLower().StartsWith($"{Program.config.mcPrefix}findfloat")) {
                    if (floatf.X == 0 && floatf.Z == 0 && floatf.Y == 0) {
                        sendText($"[FloatFinder] | Float isn't set");
                        return;
                    }
                    World world = GetWorld();
                    int radius = MinecraftClient.Settings.MCSettings_RenderDistance * 16;
                    Location minPoint = new Location(floatf.X, floatf.Y - radius, floatf.Z);
                    Location maxPoint = new Location(floatf.X, floatf.Y + radius, floatf.Z);
                    double yFloat = 0;
                    double zFloat = 0;
                    for (double y = minPoint.Y; y <= maxPoint.Y; y++) {
                        Location doneloc = new Location(minPoint.X, y, minPoint.Z);
                        Material blockType = world.GetBlock(doneloc).Type;
                        if (Program.floatFinder.cantPass(blockType) && y > 0 && y >= floatf.Y) {
                            yFloat = double.Parse(doneloc.Y.ToString().Replace(".5", ""));
                            break;
                        }
                    }
                    minPoint = new Location(floatf.X, floatf.Y - radius, floatf.Z - radius);
                    maxPoint = new Location(floatf.X, floatf.Y - radius, floatf.Z + radius);
                    for (double z = minPoint.Z; z <= maxPoint.Z; z++) {
                        Location doneloc = new Location(minPoint.X, yFloat, z);
                        Material blockType = world.GetBlock(doneloc).Type;
                        if (Program.floatFinder.cantPass(blockType)) {
                            zFloat = doneloc.Z;
                            break;
                        }
                    }
                    yFloat -= 1;
                    minPoint = new Location(floatf.X - radius, floatf.Y - radius, floatf.Z - radius);
                    maxPoint = new Location(floatf.X + radius, floatf.Y - radius, floatf.Z + radius);
                    List<int> coords = new List<int>();
                    for (double x = minPoint.X; x <= maxPoint.X; x++) {
                        Location doneloc = new Location(x, yFloat, zFloat);
                        Material blockType = world.GetBlock(doneloc).Type;
                        if (Program.floatFinder.cantPass(blockType))
                            coords.Add(int.Parse(x.ToString().Split('.')[0]));
                    }
                    zFloat = double.Parse(zFloat.ToString().Split('.')[0]);
                    sendText($"[FloatFinder] | (HeadHeight) Y: {yFloat - 1} | Z: {zFloat} | X: {1}");
                }
            }
        }
        public async void vanishTracker(string type, ulong chanID) {
            string tosend = "";
            foreach (string str in Program.serverConfig.staff) {
                await Task.Delay(1500);
                McClient.h.AutoComplete($"/f who {str}");
            }
            var list = Protocol18Handler.vanish.Except(Program.client.GetOnlinePlayers());
            foreach (string x in list) {
                tosend = tosend + x + " ";
            }
            if (type == "ig")
                sendText($"[VanishBot] [{tosend.Split(' ').Count() - 1}] | {tosend}");
            else if (type == "dc")
                await (Discord.Bot.bot.GetChannel(chanID) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"VanishBot | [{tosend.Split(' ').Count() - 1}]", $"`{tosend}`"));
            Protocol18Handler.vanish.Clear();

        }
        public async void ArchonClaimer(string player) {
            while (true) {
                if (lText.Equals("Teleportation commencing...")) {
                    sendText($"/f claim {Program.config.archonClaimAmount}");
                    sendText($"[ArchonClaimBot] Bot Has Claimed At: {GetCurrentLocation().X.ToString().Split('.')[0]} {GetCurrentLocation().Z.ToString().Split('.')[0]} | {player}");
                    break;
                }
                await Task.Delay(100);
            }
        }
        public override void AfterGameJoined() {
            Program.oBot.SendText(Program.config.hubCommand);
        }
        public void sendText(string text) {
            if (Program.config.autoFF && !text.StartsWith("/") && !Program.oBot.sendChatData) {
                if (text.ToLower().Contains("baltop")) Program.ftop.baltop = true;
                SendText($"/ff {text}");
            }
            else SendText($"{text}");
        }
        public override void OnEntitySpawn(Entity entity) {
            if (!string.IsNullOrEmpty(entity.Type.ToString()) || !string.IsNullOrWhiteSpace(entity.Type.ToString())) {
                if (entity.Type.ToString().ToLower() == "creeper") {
                    if (!Program.config.creeperDetectBot || Program.config.creeperDetectChannelID == 0)
                        return;
                    if (Program.config.creeperAutoReplay && Program.config.creeperAutoClipTime > 0)
                        Program.oBot.replayClipper(Program.config.creeperAutoClipTime);
                    Program.oBot.sendText($"[AntiFreecam] | Creeper Spawned @ [{entity.Location.ToString().Replace(".", ",")}]");
                }
                if (entity.Type.ToString().ToLower() == "snowball") {
                    if (!Program.config.creeperDetectBot || Program.config.creeperDetectChannelID == 0)
                        return;
                    if (Program.config.creeperAutoReplay && Program.config.creeperAutoClipTime > 0)
                        Program.oBot.replayClipper(Program.config.creeperAutoClipTime);
                    Program.oBot.sendText($"[AntiFreecam] | Possible Throwable Creeper Spawned @ [{entity.Location.ToString().Replace(".", ",")}]");
                }
                if (entity.Type.ToString().ToLower() == "egg") {
                    if (!Program.config.creeperDetectBot || Program.config.creeperDetectChannelID == 0)
                        return;
                    if (Program.config.creeperAutoReplay && Program.config.creeperAutoClipTime > 0)
                        Program.oBot.replayClipper(Program.config.creeperAutoClipTime);
                    Program.oBot.sendText($"[AntiFreecam] | Possible Throwable Creeper Spawned @ [{entity.Location.ToString().Replace(".", ",")}]");
                }
            }
        }
        public async void replayClipper(int awaitTime) {
           // SetNetworkPacketEventEnabled(true);
            //Program.oBot.replayClip = new ReplayHandler(GetProtocolVersion());
            //Program.oBot.replayClip.MetaData.serverName = GetServerHost() + GetServerPort();
            //await Task.Delay(TimeSpan.FromSeconds(awaitTime));
            //Program.oBot.replayClip.OnShutDown();
            //sendText($"[AntiFreecam] | Bot has clipped the last {awaitTime} seconds");
           // Program.oBot.replayClip = null;
        }
    }
}
