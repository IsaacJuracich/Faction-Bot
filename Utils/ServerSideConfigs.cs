namespace orbitFactionBot.Utils {
    public class ServerSideConfigs {
        public string factionChatFormat;
        public string ftopFormat;
        public string checkShieldsFormat;
        public string captchaFormat;
        public string openFactionFlagFormat;
        public string bankFormat;
        public bool entityHandling;
        public bool inventoryHandling;
        public string version;
        public string claimFormat;
        public System.Collections.Generic.List<string> staff;
        public string killFormat;

        public ServerSideConfigs() {
            factionChatFormat = "";
            ftopFormat = "";
            checkShieldsFormat = "";
            captchaFormat = "";
            openFactionFlagFormat = "";
            bankFormat = "";
            version = "";
            claimFormat = "";
            staff = new System.Collections.Generic.List<string>();
            killFormat = "";
        }
    }
}
