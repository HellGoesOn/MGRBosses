﻿using MGRBosses.Common;
using MGRBosses.Content.Systems.BladeMode;
using MGRBosses.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.Projectiles
{
    public partial class BladeModeProjectile : ModProjectile
    {
        public const int BladeModeSize = 300;

        public float testTimer;
        public Rectangle worldRect;
        public static List<Weakspot> Weakspots => BladeModeSystem.Weakspots;

        internal float cutProgress;

        internal Vector2 cutStartPos;
        internal Vector2 cutProgressPos;
        internal Vector2 cutDestination;

        private int directionFixer;
        private bool initialized;
        private bool oldControlUseItem;
        private float cutAngle;
        private float angleChangeSpeed;
        private Vector2 mousePos;

       // private Vector2 positionOffset;

        public override string Texture => "MGRBosses/Content/Textures/Items/Murasama";

        private Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = BladeModeSize;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.damage = 100;
            Projectile.timeLeft = 13;
        }

        public override void PostAI()
        {
            foreach (Weakspot spot in Weakspots) {
                spot.Exposed = false;
                if (Collision.CheckAABBvLineCollision(spot.Owner.Center + spot.PositionOffset, spot.Size, cutStartPos, cutDestination)) {
                    spot.Exposed = true;
                }
            }

            oldControlUseItem = Owner.controlUseItem;
        }

        public override bool PreAI()
        {
            if (!initialized) {
                InitializeSword();
                Projectile.damage = 10;
                Projectile.ai[0] = 1;
                initialized = true;
                cutAngle = MathHelper.Pi;
            }

            if (Main.myPlayer == Projectile.owner)
                mousePos = Main.MouseWorld;


            Projectile.Center = mousePos; // Owner.Center + positionOffset + new Vector2(0, Owner.gfxOffY);

            var distance = Vector2.Distance(Projectile.Center, Owner.Center);
            if (distance > 400) {
                var newPos = Projectile.Center - Owner.Center;
                newPos *= 400 / distance;
                Projectile.Center = Owner.Center + newPos;
            }

            Owner.direction = Projectile.Center.X < Owner.Center.X ? -1 : 1;


            cutStartPos = Projectile.Center + new Vector2(240 * -directionFixer, -20).RotatedBy(cutAngle);
            cutDestination = cutStartPos + (Projectile.Center - cutStartPos).SafeNormalize(-Vector2.UnitY) * 480f;

            cutStartPos = Vector2.Clamp(cutStartPos, Projectile.position, Projectile.position + Projectile.Size);
            cutDestination = Vector2.Clamp(cutDestination, Projectile.position, Projectile.position + Projectile.Size);

            cutAngle = (float)Math.Clamp(cutAngle, -MathHelper.TwoPi, MathHelper.TwoPi);
            return base.PreAI();
        }

        public override bool? CanHitNPC(NPC target)
        {
            List<Weakspot> targetWeakspot = Weakspots.Where(x => x.Owner == target).ToList();

            if (cutProgress <= 0 || targetWeakspot == null)
                return false;

            bool counter = targetWeakspot.All(x => x.Exposed) && targetWeakspot.Count > 0;

            if (counter) {

                int x = (int)(target.Center.X - BladeModeSize / 2 - Main.screenPosition.X);
                int y = (int)(target.Center.Y - BladeModeSize / 2 - Main.screenPosition.Y);
                var rect = worldRect;
                var newRect = new Rectangle(x, y, BladeModeSize, BladeModeSize);
                
                if (target.life <= (int)(target.lifeMax * 0.1f)) {
                    BladeModeSystem.HackerRectangle = newRect;
                    BladeModeSystem.hackyTargetNeedsUpdate = true;
                    BladeModeSystem.CacheGore(new(BladeModeSystem.cuttedPositionTarget, Projectile.position, BladeModeSize));
                    target.life = 0;
                }

                foreach (var weakspot in targetWeakspot) {
                    Weakspot.Remove(weakspot);
                }
                return true;//base.CanHitNPC(target);
            }

            return false;
        }

        public override void AI()
        {
            Projectile.netUpdate = true;
            testTimer += 0.06f;

            if (Main.myPlayer == Projectile.owner) {
                if (InputSystem.BladeMode.JustPressed && Projectile.timeLeft < 13)
                    Projectile.Kill();

                if (Owner.controlUseItem && !oldControlUseItem && cutProgress <= 0) {
                    int x = (int)(Projectile.position.X - Main.screenPosition.X);
                    int y = (int)(Projectile.position.Y - Main.screenPosition.Y);

                    worldRect = new Rectangle(x, y, Projectile.width, Projectile.height);
                    SetSwordAngle();
                    BladeModeSystem.cuttingLineStart = Projectile.Center + (cutStartPos - Projectile.Center).SafeNormalize(-Vector2.UnitY) * 200f;
                    BladeModeSystem.cuttingLineEnd = cutDestination + (cutDestination - Projectile.Center).SafeNormalize(-Vector2.UnitY) * 100f;
                    BladeModeSystem.shouldUpdate = true;
                    cutProgress = 1;
                    Projectile.ai[0] *= -1;
                }

                if (Owner.controlUp) {
                    if (angleChangeSpeed <= 0.16f)
                        angleChangeSpeed += 0.002f;
                    cutAngle += angleChangeSpeed;

                    if (cutAngle >= MathHelper.TwoPi)
                        cutAngle = -MathHelper.TwoPi;
                }

                if (cutProgress <= 0) {
                    if (Owner.controlDown) {
                        if (angleChangeSpeed >= -0.16f)
                            angleChangeSpeed -= 0.002f;
                        cutAngle += angleChangeSpeed;

                        if (cutAngle <= -MathHelper.TwoPi)
                            cutAngle = MathHelper.TwoPi;
                    }

                    if (!Owner.controlDown && !Owner.controlUp)
                        angleChangeSpeed = 0f;
                }
                if (Owner.controlUseTile) {
                    Projectile.timeLeft = 14;
                }
            }
            if (cutProgress > 0) {
                cutProgressPos = Vector2.Lerp(cutStartPos, cutDestination, 1 - cutProgress);
                cutProgress -= 0.12f;
            } else {
                directionFixer = (int)Projectile.ai[0];
                cutProgress = 0;
            }

            //cutAngle = (Projectile.Center - Main.MouseWorld).SafeNormalize(-Vector2.UnitY).ToRotation();

            Owner.heldProj = Projectile.whoAmI;

            if (Projectile.timeLeft <= 10)
                Projectile.timeLeft = 10;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            if (cutProgress > 0) {
                MGRBosses.DrawLine(cutProgressPos.FloatToInt(), cutDestination.FloatToInt(), 6f, Color.Crimson);
            } else {
                MGRBosses.DrawLine(cutStartPos.FloatToInt(), cutDestination.FloatToInt(), 1f, Color.LightCyan);
            }

            if (Projectile.timeLeft > 11)
                return;

            MGRBosses.DrawBorderedRectangle(Projectile.position - Main.screenPosition, Projectile.width, Projectile.height, Color.Cyan * 0.05f, Color.LightCyan * 0.5f);

            MGRBosses.DrawBorderedRectangle(cutStartPos.FloatToInt() - new Vector2(4) - Main.screenPosition, 8, 8, Color.Cyan, Color.Violet);
            MGRBosses.DrawBorderedRectangle(cutDestination.FloatToInt() - new Vector2(4) - Main.screenPosition, 8, 8, Color.Cyan, Color.Orange);

            foreach (Weakspot weakspot in Weakspots) {
                Color weakSpotColor = weakspot.Exposed ? Color.Lime * 0.5f : Color.Red * 0.2f;
                MGRBosses.DrawBorderedRectangle(weakspot.Owner.Center + weakspot.PositionOffset - Main.screenPosition, (int)weakspot.Size.X, (int)weakspot.Size.Y, weakSpotColor, Color.White);
            }

            GetSwordAngle();
            DrawSword();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(mousePos.X);
            writer.Write(mousePos.Y);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            mousePos.X = reader.ReadSingle();
            mousePos.Y = reader.ReadSingle();
        }
    }
}
