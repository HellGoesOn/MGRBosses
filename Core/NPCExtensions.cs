using MGRBosses.Content.NPCs;
using System;
using Terraria;

namespace MGRBosses.Core
{
    public static class NPCExtensions
    {
        public static void AddOnParryAction(this NPC n, Action action) => n.GetGlobalNPC<MGRGlobalNPC>().OnParry += action;
        public static void ClearOnParryAction(this NPC n) => MGRGlobalNPC.ClearOnParry(n);
    }
}
