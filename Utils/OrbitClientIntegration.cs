using MinecraftClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace orbitFactionBot.Utils {
    public class OrbitClientIntegration : ChatBot {
        public string name;
        public int blockid;
        public int pots;
        public string ign;
        public string uploadID;
        public string flipLog;
        public float x;
        public float y;
        public float z;
        public float posY;
        public float pos;
        public string direction;       
        public bool toSend = false;
        List<string> buffer = new List<string>();
        public override void Initialize() {
            if (new WebClient().DownloadString($"https://orbitdev.tech/integration.php?hwid={Program.getHWID()}").Contains("true")) {
                OCNoti();
                enable();
            }
        }
        public async void OCNoti() {
            while (true) {
                foreach (string str in new WebClient().DownloadString("https://orbitdev.tech/integration/test%20orbit.txt").Split('\n')) {
                    if (buffer.Contains(str)) {}
                    else if (!string.IsNullOrEmpty(str) || !string.IsNullOrWhiteSpace(str)) {
                        buffer.Add(str);
                        string line = str;
                        if (line.Contains("[com.orbitclient.imsoogood.ez.orbitclient.modules.SchemShare:onSocket:126]")) {
                            try {
                                if (toSend) {
                                    line = GetSubstringByString("{", "}", line.Split(']')[3]);
                                    var json = JsonConvert.DeserializeObject<OrbitClientIntegration>(line);
                                    string toStringify = $"{json.uploadID} {json.x} {json.y} {json.z}";
                                    Program.oBot.sendText($"[OrbitClientIN] | /schemshare {toStringify}");
                                }
                            }
                            catch (Exception e) {
                                ConsoleIO.WriteLine(e.StackTrace);
                            }
                        }
                        if (line.Contains("[com.orbitclient.imsoogood.ez.orbitclient.modules.PingLocation:onSocket:139]")) {
                            try {
                                if (toSend) {
                                    line = GetSubstringByString("{", "}", line.Split(']')[3]);
                                    var json = JsonConvert.DeserializeObject<OrbitClientIntegration>(line);
                                    Program.oBot.sendText($"[OrbitClientIN] | Ping from: {json.name} | Location: X: {json.x} Y: {json.y} Z: {json.z}");
                                }
                            }
                            catch (Exception e) {
                                ConsoleIO.WriteLine(e.StackTrace);
                            }
                        }
                        if (line.Contains("[CHAT]") && line.ToLower().Contains("orbitclient")) {
                            try {
                                if (toSend && GetVerbatim(line.Split(']')[3].Replace("ï¿½", "§")).Contains("(OrbitClient)")) {
                                    if (line.Split(']')[3].Replace("ï¿½", "§").Split(' ')[3].Contains(":")) {
                                        Program.oBot.sendText($"[OrbitClientIN] | Author: {line.Split(']')[3].Replace("�", "§").Split(' ')[3].Replace(":", "")} | Content: {line.Split(']')[3].Replace("�", "§").Split(' ')[4]}");
                                    }
                                    if (line.Contains("labeled chunk")) {
                                        Program.oBot.sendText($"[OrbitClientIN] | Chunk Marked by: {line.Split(' ')[4]} | Location: {line.Split(' ')[10]} {line.Split(' ')[11]}");
                                    }
                                    if (line.ToLower().Contains("current shot")) {
                                        string u = GetVerbatim(line.Split(']')[3].Replace("ï¿½", "§"));
                                        Program.oBot.sendText($"[OrbitClientIN] | Shot Recaller | X:{u.Split(' ')[6]} {u.Split(' ')[7].ToUpper()} Z:{u.Split(' ')[9]}");
                                    }
                                    if (line.ToLower().Contains("focused by")) {
                                        string u = GetVerbatim(line.Split(']')[3].Replace("ï¿½", "§"));
                                        Program.oBot.sendText($"[OrbitClientIN] | Focus: {u.Split(' ')[2]} | Focused By: {u.Split(' ')[7]}");
                                    }
                                }
                            }
                            catch (Exception e) {
                                ConsoleIO.WriteLine(e.StackTrace);
                            }
                        }
                        if (line.Contains("[com.orbitclient.imsoogood.ez.orbitclient.modules.PingBlock:onSocket:179]")) {
                            try {
                                if (toSend) {
                                    line = GetSubstringByString("{", "}", line.Split(']')[3]);
                                    var json = JsonConvert.DeserializeObject<OrbitClientIntegration>(line);
                                    Program.oBot.sendText($"[OrbitClientIN] | Block Ping from: {json.name} | Location: X:{json.x} Y:{json.y} Z:{json.x}");
                                }
                            }
                            catch (Exception e) {
                                ConsoleIO.WriteLine(e.StackTrace);
                            }
                        }
                        if (line.Contains("[com.orbitclient.imsoogood.ez.orbitclient.modules.PingAdjust:onSocket:259]")) {
                            try {
                                if (toSend) {
                                    line = GetSubstringByString("{", "}", line.Split(']')[3]);
                                    ConsoleIO.WriteLine(line);
                                    var json = JsonConvert.DeserializeObject<OrbitClientIntegration>(line);
                                    if (json.direction == "ns")
                                        Program.oBot.sendText($"[OrbitClientIN] | Adjust Ping from: {json.name} | Location: X:{json.pos} Y:{json.posY.ToString().Split('.')[0]}");
                                    if (json.direction == "ew")
                                        Program.oBot.sendText($"[OrbitClientIN] | Adjust Ping from: {json.name} | Location: Z:{json.pos} Y:{json.posY.ToString().Split('.')[0]}");
                                }
                            }
                            catch (Exception e) {
                                ConsoleIO.WriteLine(e.StackTrace);
                            }
                        }
                    }
                }
                await Task.Delay(500);
            }
        }
        public string GetSubstringByString(string a, string b, string c) {
            return a + c.Substring(c.IndexOf(a) + a.Length, c.IndexOf(b) - c.IndexOf(a) - a.Length) + b;
        }
        public async void enable() {
            await Task.Delay(5000);
            toSend = true;
        }
    }
    public class OrbitClientCapes {
        public string name;
        public List<string> uuids;
    }
}
