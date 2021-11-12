using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace orbitFactionBot.Utils {
    class JSON {
        public static void savePlayers(List<oPlayers> settings) {
            JsonSerializerSettings settings1 = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
            string contents = JsonConvert.SerializeObject(settings, Formatting.Indented, settings1);
            File.WriteAllText("!data\\players.json", contents);
        }
        public static void saveFtop(List<oFtop> settings) {
            JsonSerializerSettings settings1 = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
            string contents = JsonConvert.SerializeObject(settings, Formatting.Indented, settings1);
            File.WriteAllText("!data\\ftop.json", contents);
        }
        public static void saveSettings(oSettings settings) {
            JsonSerializerSettings settings1 = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
            string contents = JsonConvert.SerializeObject(settings, Formatting.Indented, settings1);
            File.WriteAllText("config.json", contents);
        }
        public static void saveClaims(List<oClaims> settings) {
            JsonSerializerSettings settings1 = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
            string contents = JsonConvert.SerializeObject(settings, Formatting.Indented, settings1);
            File.WriteAllText("!data\\claims.json", contents);
        }
        public static void saveKillPlayers(List<oKillPlayers> settings) {
            JsonSerializerSettings settings1 = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
            string contents = JsonConvert.SerializeObject(settings, Formatting.Indented, settings1);
            File.WriteAllText("!data\\kill.json", contents);
        }
    }
    class Converter {
        public string name;
        public long? changedToAt;
        public static string getIGN(string uuid) {
            var parse = JsonConvert.DeserializeObject<Converter[]>(new WebClient().DownloadString($"https://api.mojang.com/user/profiles/{uuid}/names"));
            return parse.Last().name.ToString();
        }
    }
}
