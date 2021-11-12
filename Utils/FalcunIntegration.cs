using MinecraftClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace orbitFactionBot.Utils {
    public class FalcunIntegration : ChatBot {
        public bool toSend = false;
        List<string> buffer = new List<string>();
        public override void Initialize() {
            if (new WebClient().DownloadString($"https://orbitdev.tech/integration.php?hwid={Program.getHWID()}").Contains("true")) {
                FCNoti();
                enable();
            }
        }
        public async void FCNoti() {
            while (true) {
                foreach (string str in new WebClient().DownloadString("https://orbitdev.tech/integration/test%20falcun.txt").Split('\n')) {
                    if (buffer.Contains(str)) { }
                    else if (!string.IsNullOrEmpty(str) || !string.IsNullOrWhiteSpace(str)) {
                        buffer.Add(str);
                        string line = str;
                        if (line.Contains("[CHAT]") && line.Contains("Falcun")) {
                            if (toSend && line.Contains("pinged location")) {
                                string u = GetVerbatim(line.Split(']')[3].Replace("ï¿½", "§").Replace("(", "").Replace(")", "").Replace(",", "").Replace(".", ""));
                                Program.oBot.sendText($"[FalcunClientIN] | Ping from: {u.Split(' ')[3]} | Location: X:{u.Split(' ')[11]} Y:{u.Split(' ')[12]} Z:{u.Split(' ')[13]}");
                            }
                            if (toSend && line.Contains("recieved a patchcrumb")) {
                                string u = GetVerbatim(line.Split(']')[3].Replace("ï¿½", "§").Replace("(", "").Replace(")", "").Replace(",", "").Replace(".", ""));
                                Program.oBot.sendText($"[FalcunClientIN] | Callout from: {u.Split(' ')[3]} | Location: X:{u.Split(' ')[12]} Y:{u.Split(' ')[13]} Z:{u.Split(' ')[14]}");
                            }
                        }
                    }
                }
                await Task.Delay(100);
            }
        }
        public async void enable() {
            await Task.Delay(5000);
            toSend = true;
        }
    }
}
