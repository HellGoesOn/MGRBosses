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

        private void BlockDamage()
        {
            NPC.damage = 0;
            SoundEngine.PlaySound(SoundID.Item37, NPC.position);
            Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Shockwave>(), 0, 0);
        }

        private void DoParry(/*Player player*/)
        {
            BlockDamage();

            if (Attack_AttemptCount > 0)
                Attack_AimTime = 38;// + player.itemAnimation;
            else
                Attack_AimTime = 2;

            //player.velocity = new Vector2(-player.direction * 3.5f, 0);
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
    }
}
