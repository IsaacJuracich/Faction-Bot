using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MinecraftClient;
using MinecraftClient.Protocol.Handlers;
using Newtonsoft.Json;
using orbitFactionBot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace orbitFactionBot.Discord {
    public class DiscordCommands : ModuleBase<SocketCommandContext> {
        [Group("Whitelist")]
        public class whitelistCommands : ModuleBase<SocketCommandContext> {
            [Command("Add")]
            public async Task whitelistAddAsync() {
                if (!Program.config.whitelistBot) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"**Whitelist Bot** Is Currently Disabled"));
                    return;
                }
                if (Discord.Bot.bot.GetChannel(Program.config.whitelistChannel) == null) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"**Whitelist Channel** Isn't Set"));
                    return;
                }
                if (oPlayers.playerExist(Context.User.Id)) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You're already **whitelisted** to `{oPlayers.getPlayerName(Context.User.Id)}`"));
                    return;
                }
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed(null, $"{Context.User.Mention} Check your **DMS**"));
                string code = $"ob-{orbitFactionBot.Utils.Discord.randomCode(5)}";
                Program.players.Add(new Utils.oPlayers("", Context.User.Id, 0, 0, 0, 0, 0, new System.Collections.Generic.List<string>(), 0, code));
                Utils.JSON.savePlayers(Program.players);
                await Context.User.SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Process", $"You will have **60** seconds to send this code in `factionChat`\n```{code}```\n**IMPORTANT**\nAfter **60** this whitelistAttempt will be terminated, and a **Log** will be sent."));
                oPlayers.whitelistProcess(Context.User.Id);
            }
            [Command("Remove")]
            public async Task whitelistRemoveAsync(SocketGuildUser user) {
                var c = Context.User as SocketGuildUser;
                if (!Program.config.adminUsers.Contains(Context.User.Id) || !c.GuildPermissions.Administrator) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Missing Permissions", $"{Context.User.Mention} You're not an **Administrator**, or you're not in the **adminList**")); return;
                }
                if (!oPlayers.playerExist(Context.User.Id)) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{user.Mention} Isn't **whitelisted**"));
                    return;
                }
                foreach (oPlayers players in Program.players) {
                    if (players.discordID == user.Id) {
                        Program.players.Remove(players);
                        JSON.savePlayers(Program.players);
                        await (orbitFactionBot.Discord.Bot.bot.GetChannel(Program.config.whitelistChannel) as ITextChannel).SendMessageAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Player Removed", $"{user.Mention} Has been `removed` from the **whitelist**"));
                    }
                }
            }
            [Command("List")]
            public async Task whitelistListAsync() {
                if (!oPlayers.playerExist(Context.User.Id)) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                    return;
                }
                string toSend = "";
                int count = 0;
                int page = 1;
                foreach (oPlayers players in Program.players) {
                    if (count == 10) {
                        await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"whiteList[{count}] | Page: [{page}]", toSend));
                        count = 0;
                        toSend = "";
                        page++;
                    }
                    toSend = toSend + $"`{players.playerName}` - {Discord.Bot.bot.GetUser(players.discordID).Mention}" + "\n";
                    count++;
                }
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"whiteList[{count}] | Page: [{page++}]", toSend));
            }
            [Command("Force")]
            public async Task whitelistForceAsync(SocketGuildUser user, string playerName) {
                if (!Program.config.whitelistBot) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"**Whitelist Bot** Is Currently Disabled"));
                    return;
                }
                if (Discord.Bot.bot.GetChannel(Program.config.whitelistChannel) == null) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"**Whitelist Channel** Isn't Set"));
                    return;
                }
                var c = Context.User as SocketGuildUser;
                if (!Program.config.adminUsers.Contains(Context.User.Id) || !c.GuildPermissions.Administrator) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Missing Permissions", $"{Context.User.Mention} You're not an **Administrator**, or you're not in the **adminList**")); return;
                }
                if (oPlayers.playerExist(user.Id)) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You're already **whitelisted** to `{oPlayers.getPlayerName(Context.User.Id)}`"));
                    return;
                }
                string code = $"ob-{orbitFactionBot.Utils.Discord.randomCode(5)}";
                Program.players.Add(new Utils.oPlayers(playerName, user.Id, 0, 0, 0, 0, 0, new System.Collections.Generic.List<string>(), 0, code));
                Utils.JSON.savePlayers(Program.players);
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Player Whitelisted", $"{user.Mention} Has been **forceWhitelisted** to `{playerName}`"));
            }
            [Command("Clear")]
            public async Task whitelistClearAsync() {
                var c = Context.User as SocketGuildUser;
                if (!Program.config.adminUsers.Contains(Context.User.Id) || !c.GuildPermissions.Administrator) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Missing Permissions", $"{Context.User.Mention} You're not an **Administrator**, or you're not in the **adminList**")); return;
                }
                int players = Program.players.Count;
                Program.players.Clear();
                JSON.savePlayers(Program.players);
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Cleared", $"**{players}** player(s) have been removed from the **whitelist**"));
            }
        }
        [Group("Admin")]
        public class adminCommands : ModuleBase<SocketCommandContext> {
            [Command("Add")]
            public async Task adminAddAsync(SocketGuildUser user) {
                var c = Context.User as SocketGuildUser;
                if (!Program.config.adminUsers.Contains(Context.User.Id) || !c.GuildPermissions.Administrator) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Missing Permissions", $"{Context.User.Mention} You're not an **Administrator**, or you're not in the **adminList**")); return;
                }
                Program.config.adminUsers.Add(user.Id);
                JSON.saveSettings(Program.config);
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Admin Added", $"{user.Mention} Has been added to the **adminList**"));
            }
            [Command("Remove")]
            public async Task adminRemoveAsync(SocketGuildUser user) {
                var c = Context.User as SocketGuildUser;
                if (!Program.config.adminUsers.Contains(Context.User.Id) || !c.GuildPermissions.Administrator) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Missing Permissions", $"{Context.User.Mention} You're not an **Administrator**, or you're not in the **adminList**")); return;
                }
                if (!Program.config.adminUsers.Contains(user.Id)) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Admin Error", $"The **adminList** doesn't contain: {user.Mention}"));
                    return;
                }
                Program.config.adminUsers.Remove(user.Id);
                JSON.saveSettings(Program.config);
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Admin Remove", $"{user.Mention} Has been remove from the **adminList**"));
            }

            [Command("Clear")]
            public async Task adminClearAsync() {
                var c = Context.User as SocketGuildUser;
                if (!Program.config.adminUsers.Contains(Context.User.Id) || !c.GuildPermissions.Administrator) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Missing Permissions", $"{Context.User.Mention} You're not an **Administrator**, or you're not in the **adminList**")); return;
                }
                int players = Program.config.adminUsers.Count;
                Program.config.adminUsers.Clear();
                JSON.saveSettings(Program.config);
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Admin Cleared", $"**{players}** player(s) have been removed from the **adminList**"));
            }
        }
        [Command("Fwho")]
        [Alias("FShow")]
        public async Task fShowAsync(string faction = null) {
            if (!oPlayers.playerExist(Context.User.Id)) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                return;
            }
            string fac = faction;
            if (faction == null)
                fac = "";
            Program.oBot.chatData = "";
            Program.oBot.embedTitle = $"factionShow | {fac}";
            Program.oBot.sendText("/f who " + fac);
            await Task.Delay(200);
            Program.oBot.sendChatData = true;
            Program.oBot.chatDataID = Context.Channel.Id;
            Program.oBot.fwhoFaction = fac;
        }
        [Command("FList")]
        public async Task fListAsync(int page) {
            if (!oPlayers.playerExist(Context.User.Id)) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                return;
            }
            Program.oBot.chatData = "";
            Program.oBot.embedTitle = $"factionList | page: {page}";
            Program.oBot.sendText("/f list " + page.ToString());
            await Task.Delay(200);
            Program.oBot.sendChatData = true;
            Program.oBot.chatDataID = Context.Channel.Id;
        }
        [Command("Sudo")]
        public async Task sudoTextAsync([Remainder] string text) {
            var c = Context.User as SocketGuildUser;
            if (!Program.config.adminUsers.Contains(Context.User.Id) || !c.GuildPermissions.Administrator) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Missing Permissions", $"{Context.User.Mention} You're not an **Administrator**, or you're not in the **adminList**")); return;
            }
            if (!oPlayers.playerExist(Context.User.Id)) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                return;
            }
            Program.oBot.chatData = "";
            Program.oBot.embedTitle = $"sudoInformation | {Context.User.Id}";
            Program.oBot.sendText(Program.config.sudoCommandFormat.Replace("[discord.fullname]", Context.User.ToString()).Replace("discord.name]", Context.User.Username).Replace("[text]", text));
            await Task.Delay(200);
            Program.oBot.sendChatData = true;
            Program.oBot.chatDataID = Context.Channel.Id;
        }
        [Command("SendCmd")]
        public async Task sendCmdAsync([Remainder] string text) {
            var c = Context.User as SocketGuildUser;
            if (!Program.config.adminUsers.Contains(Context.User.Id) || !c.GuildPermissions.Administrator) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Missing Permissions", $"{Context.User.Mention} You're not an **Administrator**, or you're not in the **adminList**")); return;
            }
            if (!oPlayers.playerExist(Context.User.Id)) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                return;
            }
            Program.oBot.chatData = "";
            Program.oBot.embedTitle = $"sendCMD | {Context.User.Id}";
            Program.oBot.sendText(text);
            await Task.Delay(200);
            Program.oBot.sendChatData = true;
            Program.oBot.chatDataID = Context.Channel.Id;
        }
        [Command("Ftop")]
        public async Task fTopAsync() {
            if (!oPlayers.playerExist(Context.User.Id)) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                return;
            }
            Program.ftop.pos = null;
            Program.ftop.faction = null;
            Program.ftop.value = null;
            Program.ftop.change = null;
            Program.oBot.sendText($"{Program.config.ftopCommand}");
            Program.ftop.ftopCompiler(Context.Channel.Id);
        }
        [Command("ShieldCheck")]
        public async Task checkShieldsAsync([Remainder] string factionList) {
            if (!oPlayers.playerExist(Context.User.Id)) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                return;
            }
            foreach (string str in factionList.Split(' ')) {
                Program.oBot.checkShields = true;
                Program.oBot.sendText("/f who " + str);
            }
            await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"ShieldChecker | FactionCount ({factionList.Split(' ').Count()})", $"Data will be returned in **{factionList.Split(' ').Count() * 2.5}** seconds"));
            await Task.Delay(TimeSpan.FromSeconds(factionList.Split(' ').Count() * 2.5));
            List<string> factions = factionList.Replace(" ", "\n").Split('\n').ToList();
            List<string> data = Program.oBot.shieldCheck.Split('\n').ToList();
            string tosend = null;
            var zip = factions.Zip(data, (f, d) => new { factions = f, data = d });
            foreach (var vara in zip) {
                tosend = tosend + vara.factions + ": **" + vara.data + "**" + "\n";
            }
            await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"ShieldChecker | FactionCount ({factionList.Split(' ').Count()})", tosend));
            Program.oBot.checkShields = false;
            Program.oBot.shieldCheck = null;
        }
        [Group("LB")]
        public class lbCommands : ModuleBase<SocketCommandContext> {
            [Command("Walls")]
            public async Task lbWallsAsync() {
                if (!oPlayers.playerExist(Context.User.Id)) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                    return;
                }
                string toSend = "";
                int count = 0;
                int page = 1;
                List<oPlayers> sorted = Program.players.OrderBy(x => x.wallChecks).ToList();
                sorted.Reverse();
                foreach (oPlayers players in sorted) {
                    if (count == 10) {
                        await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"wallTop[{count}] | Page: [{page}]", toSend));
                        count = 0;
                        toSend = "";
                        page++;
                    }
                    toSend = toSend + $"`{players.wallChecks}` - {Discord.Bot.bot.GetUser(players.discordID).Mention} - **{players.playerName}**" + "\n";
                    count++;
                }
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"wallTop[{count}] | Page: [{page++}]", toSend));
            }
            [Command("Buffers")]
            public async Task lbBuffersAsync() {
                if (!oPlayers.playerExist(Context.User.Id)) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                    return;
                }
                string toSend = "";
                int count = 0;
                int page = 1;
                List<oPlayers> sorted = Program.players.OrderBy(x => x.bufferChecks).ToList();
                sorted.Reverse();
                foreach (oPlayers players in sorted) {
                    if (count == 10) {
                        await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"bufferTop[{count}] | Page: [{page}]", toSend));
                        count = 0;
                        toSend = "";
                        page++;
                    }
                    toSend = toSend + $"`{players.bufferChecks}` - {Discord.Bot.bot.GetUser(players.discordID).Mention} - **{players.playerName}**" + "\n";
                    count++;
                }
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"bufferTop[{count}] | Page: [{page++}]", toSend));
            }
            [Command("Rpost")]
            public async Task lbRpostAsync() {
                if (!oPlayers.playerExist(Context.User.Id)) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                    return;
                }
                string toSend = "";
                int count = 0;
                int page = 1;
                List<oPlayers> sorted = Program.players.OrderBy(x => x.rpostChecks).ToList();
                sorted.Reverse();
                foreach (oPlayers players in sorted) {
                    if (count == 10) {
                        await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"rpostTop[{count}] | Page: [{page}]", toSend));
                        count = 0;
                        toSend = "";
                        page++;
                    }
                    toSend = toSend + $"`{players.rpostChecks}` - {Discord.Bot.bot.GetUser(players.discordID).Mention} - **{players.playerName}**" + "\n";
                    count++;
                }
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"rpostTop[{count}] | Page: [{page++}]", toSend));
            }
        }
        [Command("BaseRegionChecker")]
        public async Task BaseRegionCheckerAsync() {
            if (!oPlayers.playerExist(Context.User.Id)) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                return;
            }
            Program.oBot.baseRegion = true;
            Program.oBot.sendText("/f map");
            Program.oBot.baseRegionID = Context.Channel.Id;
            await Task.Delay(2500);
            await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Baseregion Checker", $"```{Program.oBot.tosend}```"));
            Program.oBot.tosend = null;
            Program.oBot.baseRegion = false;
            Program.oBot.baseRegionID = 0;
        }
        [Command("vanish")]
        public async Task vanishAsync() {
            if (!oPlayers.playerExist(Context.User.Id)) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                return;
            }
            try {
                Program.oBot.vanishTracker("dc", Context.Channel.Id);
            }
            catch (Exception e) {
            }
        }
        [Command("OrbitCapes")]
        public async Task orbitCapesAsync() {
            if (!oPlayers.playerExist(Context.User.Id)) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed("Whitelist Error", $"{Context.User.Mention} You need to be **whitelisted** to execute this `command`"));
                return;
            }
            string toSend = "";
            int count = 0;
            int page = 1;
            var parser = JsonConvert.DeserializeObject<OrbitClientCapes[]>(new WebClient().DownloadString($"http://public.orbitfac.com/getCapesNew"));
            foreach (var parse in parser) {
                if (count == 10) {
                    await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"OrbitClient Capes [{count}] | Page: [{page}]", toSend));
                    count = 0;
                    toSend = "";
                    page++;
                }
                toSend = toSend + $"**[CapeLink]({parse.name})** - **{parse.uuids.Count()}** Users" + "\n";
                count++;
            }
            await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"OrbitClient Capes [{count}] | Page: [{page}]", toSend));
        }
        [Command("Set")]
        public async Task setAsync(string setting = null, [Remainder] string content = null) {
            var c = Context.User as SocketGuildUser;
            if (!Program.config.adminUsers.Contains(Context.User.Id) || !c.GuildPermissions.Administrator) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Missing Permissions", $"{Context.User.Mention} You're not an **Administrator**, or you're not in the **adminList**")); return;
            }
            if (setting == null)
            {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Settings | Page: [1]", $"```discordprefix\nmcprefix\nhubcommand\nbotpublichannelid\nadminusers\n" +
                    $"autoff\n" +
                    $"whitelistbot\n" +
                    $"whitelistchannel\n" +
                    $"whitelistmessagedc\n" +
                    $"whitelistmessageig\n" +
                    $"embedcss\n" +
                    $"sudocommandformat\n" +
                    $"ftopbot\n" +
                    $"ftopchannelid\n" +
                    $"ftopcooldown\n" +
                    $"ftopcommand\n" +
                    $"musicbot\n" +
                    $"musicchannelid\n" +
                    $"musicsetvolume\n" +
                    $"musicmaxvolume\n" +
                    $"musiccmdcooldown```"));
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Settings | Page: [2]", $"```autofwhobot\n" +
                    $"autofwhochannelid\n" +
                    $"autofwhocooldown\n" +
                    $"autofwhofaction\n" +
                    $"autoflistbot\n" +
                    $"autoflistchannelid\n" +
                    $"autoflistcooldown\n" +
                    $"captchalogchannelid\n" +
                    $"openfactionflagbot\n" +
                    $"bankbot\n" +
                    $"banklogchannelid\n" +
                    $"banklogmessageig\n" +
                    $"banklogmessagedc\n" +
                    $"creeperdetectbot\n" +
                    $"creeperdetectchannelid\n" +
                    $"creeperautoreplay\n" +
                    $"layertrackerbot\n" +
                    $"layertrackerchannelid\n" +
                    $"layertrackercooldown\n" +
                    $"layertrackerformat\n" +
                    $"factionsplitchar```"));
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Settings | Page: [3]", $"```relogtrackerbot\n" +
                    $"relogtrackerchannelid\n" +
                    $"archonclaimbot\n" +
                    $"archonclaimamount\n" +
                    $"claimtrackerbot\n" +
                    $"claimtrackerchannelid\n" +
                    $"killtrackerbot\n" +
                    $"killtrackerchannelid\n" +
                    $"killtrackerfactions\n" +
                    $"killtrackercooldown\n" +
                    $"leaderboardupdates\n" +
                    $"leaderboadsToUpdate\n" +
                    $"wallbot\n" +
                    $"wallchannelid\n" +
                    $"wallalertcooldown\n" +
                    $"bufferbot\n" +
                    $"bufferchannelid\n" +
                    $"buffercooldown\n" +
                    $"bufferalertcooldown\n" +
                    $"buffersides\n" +
                    $"rpostbot```"));
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Settings | Page: [4]", $"```rpostchannelid\n" +
                    $"rpostcooldown\n" +
                    $"rpostalertcooldown\n" +
                    $"musthaverpost\n" +
                    $"rpostbuffersides```"));
                return;
            }
            if (content == null) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Settings | {Context.User.Id}", $"Please provide your **content**"));
                return;
            }
            content = content.ToLower();
            if (setting == "discordprefix") {
                Program.config.discordPrefix = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "mcprefix") {
                Program.config.mcPrefix = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "hubcommand") {
                Program.config.hubCommand = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "botpublicchannelid") {
                Program.config.botPublicChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "autoff") {
                Program.config.autoFF = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "whitelistbot") {
                Program.config.whitelistBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "whitelistchannel") {
                Program.config.whitelistChannel = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "whitelistmessagedc") {
                Program.config.whitelistMessageDC = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "whitelistmessageig") {
                Program.config.whitelistMessageIG = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "embedcss") {
                Program.config.embedCSS = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "sudocommandformat") {
                Program.config.sudoCommandFormat = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "ftopbot") {
                Program.config.ftopBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "ftopchannelid") {
                Program.config.ftopChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "ftopcooldown") {
                Program.config.ftopCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "ftopCommand") {
                Program.config.ftopCommand = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "musicbot") {
                Program.config.musicBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "musicchannelid") {
                Program.config.musicChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "musicsetvolume") {
                Program.config.musicSetVolume = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "musicnaxvolume") {
                Program.config.musicMaxVolume = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "musiccmdcooldown") {
                Program.config.musicCMDCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "autofwhobot") {
                Program.config.autoFWhoBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "autofwhochannelid") {
                Program.config.autoFWhoChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "autofwhocooldown") {
                Program.config.autoFWhoCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "autofwhofaction") {
                Program.config.autoFWhoFaction = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "autoflistbot")
            {
                Program.config.autoFListBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "autoflistchannelid")
            {
                Program.config.autoFListChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "autoflistcooldown")
            {
                Program.config.autoFListCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "captchalogchannelid")
            {
                Program.config.captchaLogChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "openfactionflagbot")
            {
                Program.config.openFactionFlagBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "openfactionflagchannelid")
            {
                Program.config.openFactionFlagChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "openfactionautojoin")
            {
                Program.config.openFactionAutoJoin = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "bankbot")
            {
                Program.config.bankBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "banklogchannelid")
            {
                Program.config.bankLogChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "banklogmessageig")
            {
                Program.config.bankLogMessageIG = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "banklogmessagedc") 
            {
                Program.config.bankLogMessageDC = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "creeperdetectbot")
            {
                Program.config.creeperDetectBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "creeperdetectchannelid")
            {
                Program.config.creeperDetectChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "creeperautoreplay")
            {
                Program.config.creeperAutoReplay = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "creeperautocliptime")
            {
                Program.config.creeperAutoClipTime = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "layertrackerbot")
            {
                Program.config.layerTrackerBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "layertrackerchannelid")
            {
                Program.config.layerTrackerChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "layertrackercooldown")
            {
                Program.config.layerTrackerCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "layertrackerformat")
            {
                Program.config.layerTrackerFormat = content;
                JSON.saveSettings(Program.config);
            }
            if (setting == "factionsplitchar")
            {
                Program.config.factionSplitChar = char.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "relogtrackerbot")
            {
                Program.config.relogTrackerBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "relogtrackerchannelid")
            {
                Program.config.relogTrackerChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "archonclaimbot")
            {
                Program.config.archonClaimBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "archonclaimamount")
            {
                Program.config.archonClaimAmount = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "killtrackerbot")
            {
                Program.config.killTrackerBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "killtrackerchannelid")
            {
                Program.config.killTrackerChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "killtrackercooldown")
            {
                Program.config.killTrackerCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "leaderboardupdates")
            {
                Program.config.leaderBoardUpdates = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "leaderboardtoupdate")
            {
                Program.config.leaderBoardsToUpdate.Add(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "wallbot")
            {
                Program.config.wallBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "wallchannelid")
            {
                Program.config.wallChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "wallcooldown")
            {
                Program.config.wallCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "wallalertcooldown")
            {
                Program.config.wallAlertCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "bufferbot")
            {
                Program.config.bufferBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "bufferchannelid")
            {
                Program.config.bufferChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "buffercooldown")
            {
                Program.config.bufferCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "bufferalertcooldown")
            {
                Program.config.bufferAlertCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "buffersides")
            {
                Program.config.bufferSides.Add(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "rpostbot")
            {
                Program.config.rpostBot = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "rpostchannelid")
            {
                Program.config.rpostChannelID = ulong.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "rpostcooldown")
            {
                Program.config.rpostCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "rpostalertcooldown")
            {
                Program.config.rpostAlertCooldown = int.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "musthaverpost")
            {
                Program.config.mustHaveRpost = bool.Parse(content);
                JSON.saveSettings(Program.config);
            }
            if (setting == "rpostbuffersides")
            {
                Program.config.rpostBufferSides.Add(content);
                JSON.saveSettings(Program.config);
            }
            await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Settings | {Context.User.Id}", $"**{setting}** has been changed to: **{content}**"));
        }
        [Command("Help")]
        public async Task helpAsync(string type = null) {
            if (type == null) {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Help | Info", $"**__InGame__** **__Moderation__** **__Confirgurable__** **__Discord__**"));
                return;
            }
            if (type.ToLower() == "ingame") {
                await ReplyAsync(embed: orbitFactionBot.Utils.Discord.oEmbed($"Help | Ingame", $"```{Program.config.mcPrefix}rposttop\n" +
                    $"{Program.config.mcPrefix}walltop\n" +
                    $"{Program.config.mcPrefix}buffertop\n" +
                    $"{Program.config.mcPrefix}bankdtop\n" +
                    $"{Program.config.mcPrefix}bankwtop\n" +
                    $"{Program.config.mcPrefix}flist\n" +
                    $"{Program.config.mcPrefix}baltop\n" +
                    $"{Program.config.mcPrefix}claim\n" +
                    $"{Program.config.mcPrefix}ping\n" +
                    $"{Program.config.mcPrefix}yawandpitch\n" +
                    $"{Program.config.mcPrefix}vanish\n" +
                    $"{Program.config.mcPrefix}walls\n" +
                    $"{Program.config.mcPrefix}buffers\n" +
                    $"{Program.config.mcPrefix}rpost\n" +
                    $"{Program.config.mcPrefix}sneak <true/false>\n" +
                    $"{Program.config.mcPrefix}move <direction>\n" +
                    $"{Program.config.mcPrefix}look <direction>\n" +
                    $"{Program.config.mcPrefix}dig <x y z>\n" +
                    $"{Program.config.mcPrefix}changeslot <slot>\n" +
                    $"{Program.config.mcPrefix}seen <player>\n" +
                    $"{Program.config.mcPrefix}fwho <faction>\n" +
                    $"{Program.config.mcPrefix}strikes <faction>\n" +
                    $"{Program.config.mcPrefix}wallcounter <block1> <block2> <wall>\n" +
                    $"{Program.config.mcPrefix}setfloat here\n" +
                    $"{Program.config.mcPrefix}findfloat find```"));

            }
            if (type.ToLower() == "moderation") {

            }
            if (type.ToLower() == "confirgurable") {

            }
            if (type.ToLower() == "discord") {

            }
        }
    }
}

