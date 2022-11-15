using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace MGRBosses.Core
{
    public static class PlayerExtensions
    {
        public static void BlockInputs(this Player plr)
        {
            plr.controlDown = false;
            plr.controlUp = false;
            plr.controlLeft = false;
            plr.controlRight = false;
            plr.controlHook = false;
            plr.controlUseItem = false;
            plr.controlUseTile = false;
            plr.controlTorch = false;
            plr.controlSmart = false;
            plr.controlMount = false;
            plr.controlJump = false;
            plr.controlThrow = false;
        }
    }
}
