using MinecraftClient;
using MinecraftClient.Inventory;
using MinecraftClient.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace orbitFactionBot.Commands { 
    public class FloatFinder : ChatBot {
        public bool cantPass(Material block) {  
            if (block == Material.Air)
                return false;
            if (block == Material.Water)
                return false;
            if (block == Material.Lava)
                return false;
            if (block == Material.Cobweb)
                return false;
            if (block == Material.Ladder)
                return false;
            if (block == Material.OakTrapdoor)
                return false;
            return true;
        }
        public double getClosestX(List<int> coords, double currentx) {
            return coords.Min(i => (Math.Abs(currentx - i), i)).i;
        }
    }
}
