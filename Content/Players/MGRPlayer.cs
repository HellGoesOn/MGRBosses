using MGRBosses.Common;
using MGRBosses.Content.Buffs;
using MGRBosses.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace MGRBosses.Content.Players
{
    public class MGRPlayer : ModPlayer
    {
        private bool usedParry;

        public List<int> MissedHitsQueue;

        public CameraOverride CameraOverride { get; set; }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if(InputSystem.BladeMode.JustPressed && Player.ownedProjectileCounts[ModContent.ProjectileType<BladeModeProjectile>()] <= 0)
            {
                Projectile.NewProjectile(Player.GetSource_Misc("BladeMode"),
                    Player.Center + (Player.Center- Main.MouseWorld).SafeNormalize(-Vector2.UnitY) * 180, 
                    Vector2.Zero,
                    ModContent.ProjectileType<BladeModeProjectile>(), 
                    0, 0, Player.whoAmI);
            }
        }

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

        public override void Initialize()
        {
            CameraOverride = new CameraOverride(Main.screenPosition, Vector2.Zero);
            MissedHitsQueue = new List<int>();
        }

        public override void UpdateDead()
        {
            ResetCameraOverride();
        }

        public override void ResetEffects()
        {
            if(CameraOverride.Source != null && !CameraOverride.Source.active)
            {
                ResetCameraOverride();
            }

            CameraOverride.FailSafe = Player.Center - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;

            foreach(int i in MissedHitsQueue)
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
    }
}
