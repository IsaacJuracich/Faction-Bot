using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MinecraftClient;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace orbitFactionBot.Discord {
    internal class Bot {
        private static CommandService _commands = new CommandService();
        private static IServiceProvider _services;
        public static DiscordSocketClient bot;

        public static async Task start() {
            DiscordSocketConfig config = new DiscordSocketConfig {
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true
            };
            bot = new DiscordSocketClient(config);
            await RegisterCommands();
            await bot.LoginAsync(TokenType.Bot, Program.config.discordToken, true);
            await bot.StartAsync();
            if (bot.GetApplicationInfoAsync().Result.IsBotPublic) {
                publicBotNotify();
            }
            await bot.SetGameAsync(Settings.ServerIP, null, ActivityType.Playing);
            await Task.Delay(-1);
        }
        private static async Task RegisterCommands() {
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(bot)
                .AddSingleton(_commands)
                .BuildServiceProvider();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            bot.MessageReceived += handler;
        }
        private static async Task handler(SocketMessage arg) {
            var message = arg as SocketUserMessage;
            int argPos = 0;
            var context = new SocketCommandContext(bot, message);
            if (message.HasStringPrefix(Program.config.discordPrefix, ref argPos) || message.HasMentionPrefix(bot.CurrentUser, ref argPos)) {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
            }
        }
        public static async void publicBotNotify() {
            await Task.Delay(3000);
            while (true) {
                if (bot.GetApplicationInfoAsync().Result.IsBotPublic) {
                    await (bot.GetChannel(Program.config.botPublicChannelID) as ITextChannel).SendMessageAsync(embed: new EmbedBuilder().WithTitle("BotPublic | True").WithDescription($"**Notification**\nThis Bot is currently open to anyone, through discordappdeveloper. To **Fix** this issue, follow the **directions** below. After you've updated the settings, restart the bot to apply it.").WithColor(Program.embedColor).WithFooter("OrbitDevelopment").WithCurrentTimestamp().Build());
                    new WebClient().DownloadFile("https://orbitdevelopment.xyz/7261S.png", "!utils\\7261S.png");
                    await (bot.GetChannel(Program.config.botPublicChannelID) as ITextChannel).SendFileAsync("!utils\\7261S.png");
                    File.Delete("!utils\\7261S.png");
                }
                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }
    }
}
