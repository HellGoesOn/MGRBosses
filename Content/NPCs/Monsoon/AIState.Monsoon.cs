using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGRBosses.Content.NPCs
{
    public enum AIState
    {
        Spawn,
        Idle,
        Retreat,
        Run,
        AttackChain,
        SmokePrepare,
        SmokeAttack,
        MagneticWreckageThrowPrepare,
        MagneticWreckageThrowAttack,
        MagneticSpin,
        PantsAttack,
        TenPercentLeft
    }
}
