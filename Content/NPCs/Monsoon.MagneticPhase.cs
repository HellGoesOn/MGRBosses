using MGRBosses.Common;
using MGRBosses.Content.Players;
using MGRBosses.Content.Projectiles.Monsoon;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public partial class MonsoonBoss : ModNPC
    {
        public bool IsMagnetized => magnetizedTime > 0;

        public int magnetizedTime;

        private int throwAttackLengthBase = 180;

        private int throwAttackLength = 60;

        private int currentThrownProjectileCount = 0;
        private int totalProjectileCount = 0;

        private bool prepedThrowAttack;

        private int targetThrowAttackLength;

        private bool secondWaveReached;

        private Vector2 intendedPosition;

        public void ThrowAttack()
        {
            if(!prepedThrowAttack)
            {
                intendedPosition = PlayerTarget.position + new Vector2(NPC.width * 0.5f, -400);

                if (!Main.dedServ)
                    Main.LocalPlayer.GetModPlayer<MGRPlayer>().SetCameraTarget(intendedPosition + new Vector2(0, 120), 0.14f, NPC);

                targetThrowAttackLength = throwAttackLength = throwAttackLengthBase * 2 + totalProjectileCount * 60;
                currentThrownProjectileCount = 0;
                prepedThrowAttack = true;
                totalProjectileCount++;
                secondWaveReached = false;
            }

            if(throwAttackLength >= throwAttackLengthBase && throwAttackLength <= targetThrowAttackLength * 0.8f && currentThrownProjectileCount < totalProjectileCount)
            {
                Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center + new Vector2(Main.rand.Next(-300, 301), -160), Vector2.Zero, ModContent.ProjectileType<Wreckage>(), (int)(200 * DifficultyScale), NPC.whoAmI, Main.myPlayer, PlayerTarget.whoAmI, (throwAttackLengthBase + totalProjectileCount * 60) - 60 * currentThrownProjectileCount);
                currentThrownProjectileCount++;
            }
            Player plr = Main.LocalPlayer;
            plr.position = new Vector2(Math.Clamp(plr.position.X, intendedPosition.X - 600, intendedPosition.X + 600 - plr.width), plr.position.Y);

            if (throwAttackLength > 0)
                throwAttackLength--;
            else if(!secondWaveReached)
            {
                targetThrowAttackLength = throwAttackLength = throwAttackLengthBase * 2 + totalProjectileCount * 60;
                totalProjectileCount++;
                currentThrownProjectileCount = 0;
                secondWaveReached = true;
            }
            else
            {

                currentAttack = 1;
                magnetizedTime = 1800;
                state = AIState.Idle;
                ResetAI(40);
                if (!Main.dedServ)
                    Main.LocalPlayer.GetModPlayer<MGRPlayer>().ResetCameraOverride();
            }

            NPC.position = Vector2.Lerp(NPC.position, intendedPosition, 0.25f);

        }

        private float rotationSpeed;

        public void MagneticSpin()
        {
            if(rotationSpeed < 1.6f)
                rotationSpeed += 0.006f * NPC.direction;

            rotation += rotationSpeed;
            NPC.velocity.Y = 0f;
            NPC.Center = new Vector2(NPC.Center.X, MathHelper.Lerp(NPC.Center.Y, PlayerTarget.Center.Y - 24, 0.25f));

            if(Attack_AimTime == 40)
            {
                MGRBosses.TriggerParry(NPC.Center);
                SetDamage(0.1f * DifficultyScale);
                NPC.velocity.X = NPC.direction * 16f;
            }

            if (!IsMagnetized)
                magnetizedTime = 30;

            if (--Attack_AimTime <= 0)
            {
                if (NPC.Center.Distance(PlayerTarget.Center) >= 320)
                {
                    NPC.velocity.X *= 0f;
                    if (Attack_AttemptCount > 0)
                    {
                        rotation = 0;
                        rotationSpeed = 0;
                        Attack_AimTime = 90f;
                        Attack_AttemptCount--;
                    }
                    else
                    {
                        ResetAI(30);
                        currentAttack = 1;
                        ForceRetreat(30);
                    }
                }
            }
        }

        public bool detachedLegs;

        public int pantsId;

        public void PantsAttack()
        {
            NPC.velocity.Y = 0f;
            NPC.Center = new Vector2(NPC.Center.X, MathHelper.Lerp(NPC.Center.Y, PlayerTarget.Center.Y - 24, 0.25f));

            if (Attack_AimTime == 240)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    pantsId = NPC.NewNPC(NPC.GetBossSpawnSource(NPC.target), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<MonsoonPants>(), 0, 40, 4, 0, 0, NPC.target);
            }

            if (pantsId == -1 && Attack_AimTime <= 239)
                Attack_AimTime = 0;

            if(--Attack_AimTime <= 0)
            {
                ResetAI(10);
                state = AIState.Idle;
                currentAttack = 1;
            }
        }
    }
}
