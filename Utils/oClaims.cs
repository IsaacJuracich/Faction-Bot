namespace orbitFactionBot.Utils {
    public class oClaims {
        public ulong currentValue;
        public string faction;
        public oClaims(ulong currentValue, string faction) {
            this.currentValue = currentValue;
            this.faction = faction;
        }
        public static bool factionExist(string fac) {
            foreach (oClaims ftop in Program.claims) {
                if (ftop.faction == fac)
                    return true;
            }
            return false;
        }
        public static oClaims getClaims(string fac) {
            foreach (oClaims ftop in Program.claims) {
                if (ftop.faction == fac)
                    return ftop;
            }
            return new oClaims(0, "");
        }
    }
}
