﻿using MGRBosses.Content.Players;
using MGRBosses.Content.Projectiles.Monsoon;
using MGRBosses.Content.Systems.Cinematic;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public partial class MonsoonBoss : ModNPC
    {
        public bool beganTenPercentAttack;

        public void TenPercentRule()
        {
            if (!beganTenPercentAttack) {
                Attack_AimTime = 240;
                intendedPosition = PlayerTarget.Center + new Vector2(NPC.width * 0.5f + 400 * PlayerTarget.direction, -400);

                    var scene =
                    CinematicSystem.AddCinematicScene();
                    scene.AddSequence(240, () =>
                    {
                        scene.screenPosition += (intendedPosition + new Vector2(0, 120) - scene.screenPosition) * 0.025f;
                    });

                    beganTenPercentAttack = true;
            }

            if (Attack_AimTime == 160) {
                Main.LocalPlayer.Center = NPC.Center + new Vector2((16 * 32) * NPC.direction, 360 - Main.LocalPlayer.height);
                Projectile.NewProjectile(NPC.GetBossSpawnSource(NPC.target), NPC.Center + new Vector2(-600 * NPC.direction, 240), Vector2.Zero, ModContent.ProjectileType<ChekhovRifle>(), 0, NPC.whoAmI);
            }

            float horizontalDist = (float)Math.Abs(NPC.Center.X - Main.LocalPlayer.Center.X);

            if (Attack_AimTime > 0) {
                Attack_AimTime--;
            } else if (horizontalDist < 100) {
                noGrav = true;
                instaKill = true;
                ResetAI(30, 2, 24);
                state = AIState.AttackChain;
            }

            NPC.position = Vector2.Lerp(NPC.position, intendedPosition, 0.25f);
        }
    }
}
