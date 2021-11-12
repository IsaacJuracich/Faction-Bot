namespace orbitFactionBot.Utils {
    public class oFtop {
        public ulong currentValue;
        public string faction;
        public oFtop(ulong currentValue, string faction) {
            this.currentValue = currentValue;
            this.faction = faction;
        }
        public static bool factionExist(string fac) {
            foreach (oFtop ftop in Program.ftops) {
                if (ftop.faction == fac)
                    return true;
            }
            return false;
        }
        public static oFtop getFtop(string fac) {
            foreach (oFtop ftop in Program.ftops) {
                if (ftop.faction == fac)
                    return ftop;
            }
            return new oFtop(0, "");
        }
    }
}
