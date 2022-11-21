using MGRBosses.Common;
using MGRBosses.Content.Buffs;
using MGRBosses.Content.Projectiles;
using MGRBosses.Content.Systems.Arenas;
using MGRBosses.Content.Systems.Cinematic;
using MGRBosses.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using static Terraria.Player;
using Terraria.ID;
using Terraria.Audio;
using MGRBosses.Content.NPCs;

namespace MGRBosses.Content.Players
{
    public class MGRPlayer : ModPlayer
    {
        public int parryCooldown;
        public int parryTime;
        public float visualDecay;

        private bool usedParry;

        public List<int> MissedHitsQueue;

        public bool HasActiveBladeMode => Player.ownedProjectileCounts[ModContent.ProjectileType<BladeModeProjectile>()] > 0;

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
            if (parryTime >= 40 && hitDirection != Player.direction) {
                return false;
            }
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }

        public override void OnHitByNPC(NPC npc, int damage, bool crit)
        {
            if (parryTime >= 40 && Player.direction != npc.direction) {
                MGRGlobalNPC.DoOnParry(npc);
                ResetParry();
            }
            base.OnHitByNPC(npc, damage, crit);
        }

        public override void OnHitByProjectile(Projectile proj, int damage, bool crit)
        {
            if (parryTime >= 40) {
                ResetParry();
                proj.velocity *= -1;
                proj.friendly = true;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (CinematicScene.IsActor(Player, out var res)) {

                if (res.SequenceBlocksInput) {
                    Player.BlockInputs();
                }
            }

            if (parryTime > 0)
                Player.BlockInputs();

            if(InputSystem.Parry.JustPressed && parryCooldown <= 0 && Player.HeldItem != null
                && Player.HeldItem.CountsAsClass<MeleeDamageClass>()) {
                parryCooldown = 60;
                parryTime = 60;
            }

            if(InputSystem.BladeMode.JustPressed && !HasActiveBladeMode)
            {
                Projectile.NewProjectile(Player.GetSource_Misc("BladeMode"),
                    Player.Center + (Player.Center- Main.MouseWorld).SafeNormalize(-Vector2.UnitY) * 180, 
                    Vector2.Zero,
                    ModContent.ProjectileType<BladeModeProjectile>(), 
                    0, 0, Main.myPlayer);
            }
        }
        /*
        public void SetCameraTarget(Vector2 targetLocation, float speed = 0.15f, Entity source = null)
        {
            if(!CameraOverride.Active)
            {
                CameraOverride.Active = true;
                CameraOverride.CurrentPosition = Main.screenPosition;
            }
            CameraOverride.Source = source;
            CameraOverride.TargetLocation = targetLocation;
            CameraOverride.Speed = speed;
        }
        public void ResetCameraOverride()
        {
            CameraOverride.Source = null;
            CameraOverride.Active = false;
            CameraOverride.Speed = 0.15f;
            CameraOverride.HasBeenReset = true;
        }

        public override void ModifyScreenPosition()
        {
            if(!CameraOverride.IsDefault(CameraOverride) && !CameraOverride.HasBeenReset)
            {
                Main.screenPosition = CameraOverride.CurrentPosition;
                CameraOverride.CurrentPosition = Vector2.SmoothStep
                    (CameraOverride.CurrentPosition,
                    CameraOverride.TargetLocation - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f,
                    CameraOverride.Speed);
            }

            Vector2 target = Player.dead ? CameraOverride.FailSafe : Player.Center - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            
            if (CameraOverride.HasBeenReset)
            {
                if (Vector2.Distance(CameraOverride.CurrentPosition, target) > 32)
                {
                    Main.screenPosition = CameraOverride.CurrentPosition;

                    CameraOverride.CurrentPosition = Vector2.SmoothStep
                        (CameraOverride.CurrentPosition,
                        target,
                        CameraOverride.Speed);
                }
                else
                {
                    if (!Player.dead)
                    {
                        CameraOverride.HasBeenReset = false;
                        CameraOverride.Speed = -1.0f;
                        CameraOverride.CurrentPosition = Vector2.Zero;
                    }
                }
            }
        }
        */
        public override void Initialize()
        {
            //CameraOverride = new CameraOverride(Main.screenPosition, Vector2.Zero);
            MissedHitsQueue = new List<int>();
        }

        public override void UpdateDead()
        {
            //ResetCameraOverride();
        }

        public override void PreUpdateMovement()
        {
            // use pattern matching for cheating && writing less code?
            if (BossArenaSystem.GetArenaForMe(Player) is BossArena arena) {
                if (Player.Bottom.Y >= arena.position.Y + arena.size.Y && Player.velocity.Y > 0)
                    Player.velocity.Y *= 0;
            }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (parryTime > 0) {
                int dir_m = Player.direction; // Direction multiplier
                var vec = new Vector2(-10 * dir_m, -24);

                var vec2 = new Vector2(10 * dir_m, 6);
                float angle = (Player.Center + vec - Player.Center).ToRotation();
                float angle2 = (Player.Center + vec2 - Player.Center).ToRotation();

                Player.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, angle + visualDecay * dir_m - MathHelper.PiOver2); 
                Player.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, angle2 - visualDecay * dir_m - MathHelper.PiOver2); 
            }
        }

        public override void ResetEffects()
        {/*
            if (CameraOverride.Source != null && !CameraOverride.Source.active)
            {
                ResetCameraOverride();
            }

            CameraOverride.FailSafe = Player.Center - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;*/

            if (visualDecay > 0)
                visualDecay -= 0.02f;

            if(parryTime > 0) {
                parryTime--;
            }

            if(parryCooldown > 0) {
                Player.velocity *= 0f;
                parryCooldown--;
            }

            foreach (int i in MissedHitsQueue)
            {
                Main.combatText[i].text = "MISS";
                Main.combatText[i].color = Color.Red;
                Main.combatText[i].crit = false;
            }

            if (usedParry && Player.itemAnimation <= 1)
                Player.ClearBuff(ModContent.BuffType<ParryBuff>());

            if (Player.HasBuff<ParryBuff>())
            {
                if (Player.itemAnimation > 0 && !usedParry)
                {
                    usedParry = true;
                }
            }
            else if(!Player.controlUseItem)
                usedParry = false;

            MissedHitsQueue.Clear();
        }

        public void ResetParry()
        {
            var immuneTime = 20;
            if(parryTime >= 40) {
                //immuneTime = 160;
            }

            Player.SetImmuneTimeForAllTypes(immuneTime);
            Player.immuneNoBlink = true;
            Main.NewText("Parried!");
            Player.velocity = new Vector2(-Player.direction * 3.5f, 0);
            parryCooldown = 0;
            parryTime = immuneTime;
            SoundEngine.PlaySound(SoundID.Item37, Player.position);
            visualDecay = 0.42f;
        }
    }
}
