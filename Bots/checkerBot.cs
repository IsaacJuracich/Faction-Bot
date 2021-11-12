using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinecraftClient;

namespace orbitFactionBot.Bots {
    public class checkerBot : ChatBot {
        public bool cooldownCheck(string file, int compareDelay) {
            var lastModify = (DateTime.Now - File.GetLastWriteTime(file)).TotalMinutes;
            if (lastModify < compareDelay)
                return true;
            if (lastModify > compareDelay)
                return false;
            return false;
        }
        public override void Initialize() {
            checkerAlert("walls");
            checkerAlert("buffers");
            checkerAlert("rpost");
        }
        public async void checkerAlert(string type) {
            await Task.Delay(10000);
            if (type == "walls") {
                if (!Program.config.wallBot || Program.config.wallChannelID == 0 || Program.config.wallCooldown == 0)
                    return;
                while (true) {
                    if (!cooldownCheck("!data/walls.txt", Program.config.wallCooldown)) {
                        var lastModify = (DateTime.Now - File.GetLastWriteTime("!data/walls.txt")).TotalMinutes;
                        TimeSpan remain = TimeSpan.FromMinutes(lastModify);
                        Program.oBot.sendText($"[WallBot] | Walls haven't been checked for: {remain.Minutes} minutes | Checker: {File.ReadAllText("!data/walls.txt")}");
                    }
                    await Task.Delay(TimeSpan.FromMinutes(Program.config.wallAlertCooldown));
                }
            }
            if (type == "buffers") {
                if (!Program.config.bufferBot || Program.config.bufferChannelID == 0 || Program.config.bufferCooldown == 0)
                    return;
                while (true) {
                    if (!cooldownCheck("!data/buffers.txt", Program.config.bufferCooldown)) {
                        var lastModify = (DateTime.Now - File.GetLastWriteTime("!data/buffers.txt")).TotalMinutes;
                        TimeSpan remain = TimeSpan.FromMinutes(lastModify);
                        Program.oBot.sendText($"[BufferBot] | Buffers haven't been checked for: {remain.Minutes} minutes | Checker: {File.ReadAllText("!data/buffers.txt")}");
                    }
                    await Task.Delay(TimeSpan.FromMinutes(Program.config.bufferAlertCooldown));
                }
            }
            if (type == "rpost") {
                if (!Program.config.rpostBot || Program.config.rpostChannelID == 0 || Program.config.rpostCooldown == 0)
                    return;
                while (true) {
                    if (!cooldownCheck("!data/rpost.txt", Program.config.rpostCooldown)) {
                        var lastModify = (DateTime.Now - File.GetLastWriteTime("!data/rpost.txt")).TotalMinutes;
                        TimeSpan remain = TimeSpan.FromMinutes(lastModify);
                        Program.oBot.sendText($"[RpostBot] | RpostWalls haven't been checked for: {remain.Minutes} minutes | Checker: {File.ReadAllText("!data/rpost.txt")}");
                    }
                    await Task.Delay(TimeSpan.FromMinutes(Program.config.rpostAlertCooldown));
                }
            }
        }
    }
}
