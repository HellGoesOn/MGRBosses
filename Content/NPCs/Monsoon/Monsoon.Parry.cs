using MGRBosses.Content.Buffs;
using MGRBosses.Content.Players;
using MGRBosses.Content.Projectiles.Monsoon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public partial class MonsoonBoss : ModNPC
    {
        private readonly string[] taunts = new string[]
        {
            "ARE YOU EVEN AIMING?",
            "RIDICULUOUS",
            "ARE YOU STUPID?",
            "CATCH ME IF YOU CAN"
        };

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            if (!player.HasBuff<ParryBuff>())
                return;

            if (state == AIState.SmokeAttack && Attack_AimTime > CURRENT_AIM_TIME_MAX * 0.25 && Attack_AttemptCount > 0 && player.direction != NPC.direction) {
                SoundEngine.PlaySound(SoundID.Item37, NPC.position);
                Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Shockwave>(), 0, 0);
                Attack_AimTime = (float)(CURRENT_AIM_TIME_MAX * 0.25);
            }

            if (state == AIState.AttackChain && Attack_AimTime <= 12 && Attack_AimTime >= 4 && player.direction != NPC.direction) {
                DoParry(player);
            }

            if (state == AIState.MagneticSpin) {
                player.velocity.X = -4f * player.direction;
                BlockDamage();

            }
        }

        private void BlockDamage()
        {
            NPC.damage = 0;
            SoundEngine.PlaySound(SoundID.Item37, NPC.position);
            Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Shockwave>(), 0, 0);
        }

        private void DoParry(Player player)
        {
            BlockDamage();

            if (Attack_AttemptCount > 0)
                Attack_AimTime = 30 + player.itemAnimation;
            else
                Attack_AimTime = 2;

            player.velocity = new Vector2(-player.direction * 3.5f, 0);
        }

        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            if (NPC.life <= NPC.lifeMax * 0.1f) {
                if (player.HasBuff<ParryBuff>())
                    damage = (int)(NPC.lifeMax * 0.1f);
                else {
                    crit = false;
                    NPC.life++;
                    damage = 0;

                    for (int i = 0; i < Main.combatText.Length; i++) {
                        if (!Main.combatText[i].active) {
                            Main.LocalPlayer.GetModPlayer<MGRPlayer>().MissedHitsQueue.Add(i);
                            break;
                        }
                    }
                }
            }
            IgnoreDamageWhenMagnetized(ref damage, ref crit);
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (NPC.life <= NPC.lifeMax * 0.1f) {
                if (Main.player[projectile.owner].HasBuff<ParryBuff>())
                    damage = (int)(NPC.lifeMax * 0.1f);
                else {
                    crit = false;
                    NPC.life++;
                    damage = 0;

                    for (int i = 0; i < Main.combatText.Length; i++) {
                        if (!Main.combatText[i].active) {
                            Main.LocalPlayer.GetModPlayer<MGRPlayer>().MissedHitsQueue.Add(i);
                            break;
                        }
                    }
                }
            }

            if (projectile.type == ProjectileID.Electrosphere)
                magnetizedTime = 1;

            IgnoreDamageWhenMagnetized(ref damage, ref crit);
        }

        private void IgnoreDamageWhenMagnetized(ref int damage, ref bool crit)
        {
            if (IsMagnetized) {
                if (Main.rand.NextBool(6))
                    Say(taunts[Main.rand.Next(taunts.Length)]);

                crit = false;
                NPC.life++;
                damage = 0;

                for (int i = 0; i < Main.combatText.Length; i++) {
                    if (!Main.combatText[i].active) {
                        Main.LocalPlayer.GetModPlayer<MGRPlayer>().MissedHitsQueue.Add(i);
                        break;
                    }
                }
            }
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            if (!Main.player[projectile.owner].HasBuff<ParryBuff>())
                return;

            if (Attack_AimTime > CURRENT_AIM_TIME_MAX * 0.25 && Attack_AttemptCount > 0 && projectile.direction != NPC.direction) {
                SoundEngine.PlaySound(SoundID.Item37, NPC.position);
                Attack_AimTime = (float)(CURRENT_AIM_TIME_MAX * 0.25);
            }

            if (state == AIState.AttackChain && Attack_AimTime <= 24 && Attack_AimTime >= 14 && projectile.direction != NPC.direction) {
                DoParry(Main.player[projectile.owner]);
            }

            if (state == AIState.MagneticSpin) {
                BlockDamage();
            }
        }
    }
}
