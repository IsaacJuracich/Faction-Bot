using MinecraftClient;
using orbitFactionBot.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orbitFactionBot.Bots
{
    public class ReplayCapture : ChatBot
    {
        private Replay replay;
        private int backupInterval = 3000; // Unit: second * 10
        private int backupCounter = -1;

        public ReplayCapture(int backupInterval)
        {
            if (backupInterval != -1)
                this.backupInterval = backupInterval * 10;
            else this.backupInterval = -1;
        }

        public override void Initialize()
        {
            SetNetworkPacketEventEnabled(true);
            replay = new Replay(GetProtocolVersion());
            replay.MetaData.serverName = GetServerHost() + GetServerPort();
            backupCounter = backupInterval;
        }

        public override void OnNetworkPacket(int packetID, List<byte> packetData, bool isLogin, bool isInbound)
        {
            replay.AddPacket(packetID, packetData, isLogin, isInbound);
        }

        public override void Update()
        {
            if (backupInterval > 0 && replay.RecordRunning)
            {
                if (backupCounter <= 0)
                {
                    replay.CreateBackupReplay(@"recording_cache\REPLAY_BACKUP.mcpr");
                    backupCounter = backupInterval;
                }
                else backupCounter--;
            }
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            replay.OnShutDown();
            return base.OnDisconnect(reason, message);
        }
    }
}
