
namespace orbitFactionBot.Utils {
    public class oKillPlayers {
        public int kills;
        public int deaths;
        public string playername;
        public string faction;

        public oKillPlayers(int kills, int deaths, string playername, string faction) {
            this.kills = kills;
            this.deaths = deaths;
            this.playername = playername;
            this.faction = faction;
        }
        public static bool killExist(string player) {
            foreach (oKillPlayers players in Program.kplayers) {
                if (players.playername == player)
                    return true;
            }
            return false;
        }
        public static oKillPlayers getPlayer(string player) {
            foreach (oKillPlayers players in Program.kplayers) {
                if (players.playername == player)
                    return players;
            }
            return new oKillPlayers(0, 0, "", "");
        }
    }
}
