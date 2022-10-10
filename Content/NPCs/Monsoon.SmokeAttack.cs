using MGRBosses.Content.Projectiles.Monsoon;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public partial class MonsoonBoss : ModNPC
    {
        private float fogDensity = 0f;
        private bool jumpedAway;

        public List<MonsoonFog> monsoonFog = new List<MonsoonFog>();

        private float targetOpacity = 0.8f;

        private void SmokeAttack()
        {
            NPC.noTileCollide = true;

            foreach (MonsoonFog fog in monsoonFog)
                fog.Update();

            if (Attack_AttemptCount <= 0)
            {
				NPC.velocity *= 0;
                ForceRetreat(40);
            }

            if (fogDensity < 0.35f)
            {
                monsoonOpacity = 0f;
                fogDensity += 0.05f;
            }
            int val = (int)(CURRENT_AIM_TIME_MAX * 0.6f);

            if (Attack_AimTime == val)
                MGRBosses.TriggerParry(NPC.Center);

            if (Attack_AimTime > CURRENT_AIM_TIME_MAX * 0.5)
            {
                monsoonOpacity = Math.Clamp(monsoonOpacity + 0.5f, 0, targetOpacity);


                jumpedAway = false;

                float heightMultiplier = (AttacksFrom)Attack_Direction == AttacksFrom.TopRight
                    || (AttacksFrom)Attack_Direction == AttacksFrom.TopLeft ? 1 : 0.025f;

                float directionMultiplier = (AttacksFrom)Attack_Direction == AttacksFrom.TopRight
                    || (AttacksFrom)Attack_Direction == AttacksFrom.Right ? 1 : -1;

                NPC.Center = PlayerTarget.Center + new Vector2(250 * directionMultiplier, -200 * heightMultiplier);
                Attack_AimTime--;
            }
            else if (Attack_AimTime > CURRENT_AIM_TIME_MAX * 0.25f)
            {
                monsoonOpacity = 1f;

                if (DistanceFromTarget <= 2400)
                    Attack_AimTime--;
                SetDamage(0.2f*DifficultyScale);
                NPC.velocity = (PlayerTarget.Center - NPC.Center).SafeNormalize(-Vector2.UnitY) * 12f;

            }
            else if (Attack_AimTime > 0)
            {
                NPC.damage = 0;
                if (monsoonOpacity > 0)
                    monsoonOpacity -= 0.075f;

                Attack_AimTime--;

                if (!jumpedAway)
                {
                    jumpedAway = true;
                    NPC.velocity = -(PlayerTarget.Center - NPC.Center).SafeNormalize(-Vector2.UnitY) * 4f - new Vector2(0, 8);
                }
            }
            else if (Attack_AttemptCount > 0)
            {
                Attack_Direction = Main.rand.Next((int)AttacksFrom.Right + 1);
                Attack_AttemptCount--;
                Attack_AimTime = CURRENT_AIM_TIME_MAX;
            }
        }

        private void SmokePrepare()
        {
            if (Attack_AimTime >= 90)
            {
                monsoonFog.Clear();
                NPC.velocity *= 0f;
                Say(smokeQuotes[Main.rand.Next(smokeQuotes.Length)]);
            }
            if (Attack_AimTime > 0)
                Attack_AimTime--;

            if (Attack_AimTime == 80)
            {
                for(int i = 0; i < 600; i++)
                {
                    MonsoonFog fog = new MonsoonFog();
                    float halfWidth = Main.screenWidth * 0.5f;
                    float halfHeight = Main.screenHeight * 0.5f;
                    fog.position = new Vector2(halfWidth + Main.rand.Next(-(int)(halfWidth * 0.85f), (int)(halfWidth * 0.85f)), halfHeight + Main.rand.Next(-(int)(halfHeight * 0.9f), (int)(halfHeight * 0.85f)));
                    fog.velocity = new Vector2(Main.rand.Next(-36, 36) * 0.01f, 0);
                    fog.variation = Main.rand.Next(2);
                    monsoonFog.Add(fog);
                }

                Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center, new Vector2(2.9f, -6f), ModContent.ProjectileType<RedPhosphorDoodad>(), 0, 0);
                Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center, new Vector2(-2.9f, -6f), ModContent.ProjectileType<RedPhosphorDoodad>(), 0, 0);
            }

            if (Attack_AimTime < 40 && fogDensity < 0.65f)
                fogDensity += 0.05f;

            if (Attack_AimTime == 50)
            {
                NPC.velocity = -(PlayerTarget.Center - NPC.Center).SafeNormalize(-Vector2.UnitY) * 12f - new Vector2(0, 16);
            }

            if (Attack_AimTime <= 0)
            {
                monsoonOpacity = 0f;
                state = AIState.SmokeAttack;

                Attack_AttemptCount = 5;

                Attack_AimTime = CURRENT_AIM_TIME_MAX;

                Attack_Direction = Main.rand.Next((int)AttacksFrom.Right + 1);
            }
        }
    }
}
