using MGRBosses.Content.Buffs;
using MGRBosses.Content.Projectiles.Monsoon;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public partial class MonsoonBoss : ModNPC
    {
        /*
        private bool switchSide;
        private int currentSide;
        */

        private bool noGrav;

        private void AttackChain()
        {
            if (noGrav)
                NPC.velocity.Y *= 0f;

            Attack_Direction = PlayerTarget.Center.X < NPC.Center.X ? -1 : 1;

            /*if (Attack_AimTime == 60)
            {
                Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Shockwave>(), 0, 0);
                NPC.velocity = new Vector2(-8 * Attack_Direction, -10.5f);

                switchSide = Main.rand.NextBool();

                Attack_AttemptCount--;
            }

            if (switchSide && Attack_AimTime == 58)
            {
                switchSide = false;
                NPC.velocity = new Vector2(NPC.direction * 12, -10.5f);
            }*/

            if(Attack_AimTime > 30)
            {
                NPC.velocity.X *= 0.25f;
            }

            if (DistanceFromTarget > 10000 && Attack_AimTime <= 30)
            {
                Run();
                MoveTowardsPlayer();
                    return;
            }

            if (DistanceFromTarget <= 10000 && Attack_AimTime > 20 && Attack_AimTime < 30)
                NPC.velocity.X *= 0f;

            if (Attack_AimTime == 24)
            {
                Attack_AttemptCount--;
                if(Attack_AttemptCount > 0)
                MGRBosses.TriggerParry(NPC.Center);
            }

            if (Attack_AimTime == 12)
            {
                SetDamage(0.08f * DifficultyScale);
                if (instaKill)
                    SetDamage(100f);

                float ySpeed = instaKill ? 0 : 16f;
                float velY = ((PlayerTarget.Center - NPC.Center).SafeNormalize(-Vector2.UnitY) * ySpeed).Y;
                float vel = instaKill ? 46f : 12f;
                NPC.velocity.X = vel * Attack_Direction;
                NPC.velocity.Y = velY;
            }

            if (Attack_AimTime == 2)
            {
                NPC.velocity.X *= 0f;
                NPC.damage = 0;
            }

            if (Attack_AimTime > 0)
                Attack_AimTime--;
            else
            {
                Attack_AimTime = 30;
            }

            if (Attack_AttemptCount <= 0)
            {
                ForceRetreat(40);
            }
        }

    }
}
