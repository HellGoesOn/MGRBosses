using MGRBosses.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MGRBosses.Content.Projectiles
{
    public class Weakspot
    {
        public Entity Owner;

        public Vector2 PositionOffset;

        public Vector2 Size;

        public bool Exposed;

        public Weakspot(Entity owner, Vector2 offset, Vector2 size)
        {
            Owner = owner;
            PositionOffset = offset;
            Size = size;
            Exposed = false;
        }
    }

    public class BladeModeProjectile : ModProjectile
    {
        public override string Texture => "MGRBosses/Content/Textures/Items/Murasama";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.damage = 100;
            Weakspots = new List<Weakspot>();
            Projectile.timeLeft = 13;
        }

        public List<Weakspot> Weakspots;

        private bool initialized;

        private bool initializedList;

        private Vector2 positionOffset;

        private Player Owner => Main.player[Projectile.owner];

        private bool oldControlUseItem;

        private float cutAngle;
        private float angleChangeSpeed;

        private Vector2 cutStartPos;
        private Vector2 cutProgressPos;
        private Vector2 cutDestination;

        private float cutProgress;

        private int directionFixer;

        public override void PostAI()
        {
            if (!initializedList && Projectile.timeLeft == 12)
            {
                Weakspots.Clear();
                initializedList = true;
                List<NPC> qualifiedToBeDismembered = Main.npc.Where(x => x.active && x.Hitbox.Intersects(Projectile.Hitbox)).ToList();

                foreach (NPC npc in qualifiedToBeDismembered)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int halfWidth = (int)(npc.width * 0.5);
                        int halfHeight = (int)(npc.height * 0.5);
                        Weakspot newWeakspot = new Weakspot(npc, new Vector2(-20 + 20 * i, 0), new Vector2(20));
                        Weakspots.Add(newWeakspot);
                    }
                }
            }

            foreach(Weakspot spot in Weakspots)
            {
                spot.Exposed = false;
                if(Collision.CheckAABBvLineCollision(spot.Owner.Center + spot.PositionOffset, spot.Size, cutStartPos, cutDestination))
                {
                    spot.Exposed = true;
                }
            }
        }

        public override bool PreAI()
        {
            if (!initialized)
            {
                Projectile.damage = 1000;
                Projectile.ai[0] = 1;
                positionOffset = Owner.Center - Projectile.Center;
                initialized = true; 
                cutAngle = MathHelper.Pi;
            }

            return base.PreAI();
        }

        public override bool? CanHitNPC(NPC target)
        {
            List<Weakspot> targetWeakspot = Weakspots.Where(x => x.Owner == target).ToList();

            if (cutProgress <= 0 || targetWeakspot == null)
                return false;

            bool counter = targetWeakspot.All(x => x.Exposed);
            if(counter)
                return base.CanHitNPC(target);

            return false;
        }

        public override void AI()
        {
            Projectile.netUpdate = true;

            if (Main.myPlayer == Projectile.owner)
            {
                if (InputSystem.BladeMode.JustPressed && initializedList)
                    Projectile.Kill();

                if(Owner.controlUseItem && !oldControlUseItem && cutProgress <= 0)
                {
                    cutProgress = 1;
                    Projectile.ai[0] *= -1;
                }

                if (Owner.controlUp)
                {
                    if (angleChangeSpeed <= 0.16f)
                        angleChangeSpeed += 0.002f;
                    cutAngle += angleChangeSpeed;

                    if (cutAngle >= MathHelper.TwoPi)
                        cutAngle = -MathHelper.TwoPi;
                }

                if (cutProgress <= 0)
                {
                    if (Owner.controlDown)
                    {
                        if (angleChangeSpeed >= -0.16f)
                            angleChangeSpeed -= 0.002f;
                        cutAngle += angleChangeSpeed;

                        if (cutAngle <= -MathHelper.TwoPi)
                            cutAngle = MathHelper.TwoPi;
                    }

                    if (!Owner.controlDown && !Owner.controlUp)
                        angleChangeSpeed = 0f;
                }
                if(Owner.controlUseTile)
                {
                    Projectile.timeLeft = 14;
                    initializedList = false;
                }
            }

            positionOffset = (Main.MouseWorld - Owner.Center).SafeNormalize(-Vector2.UnitY) * 180;
            if (cutProgress >= 0)
            {
                cutProgressPos = Vector2.Lerp(cutStartPos, cutDestination, 1-cutProgress);
                cutProgress -= 0.12f;
            }
            else
            if(cutProgress  != -100)
            {
                directionFixer = (int)Projectile.ai[0];
                cutProgress = -100;
            }

            //cutAngle = (Projectile.Center - Main.MouseWorld).SafeNormalize(-Vector2.UnitY).ToRotation();

            Owner.heldProj = Projectile.whoAmI;

            Projectile.Center = Owner.Center + positionOffset + new Vector2(0, Owner.gfxOffY);

            if(Projectile.timeLeft <= 10)
                Projectile.timeLeft = 10;

            oldControlUseItem = Owner.controlUseItem;

            cutStartPos = Projectile.Center + new Vector2(240 * -directionFixer, -20).RotatedBy(cutAngle);
            cutDestination = cutStartPos + (Projectile.Center - cutStartPos).SafeNormalize(-Vector2.UnitY) * 480f;

            cutStartPos = Vector2.Clamp(cutStartPos, Projectile.position, Projectile.position + Projectile.Size);
            cutDestination = Vector2.Clamp(cutDestination, Projectile.position, Projectile.position + Projectile.Size);

            Owner.direction = Projectile.Center.X < Owner.Center.X ? -1 : 1;

            cutAngle = (float)Math.Clamp(cutAngle, -MathHelper.TwoPi, MathHelper.TwoPi);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D sword = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Items/Murasama").Value;


            bool flip = Projectile.ai[0] == -1;
            Vector2 origin = !flip ? new Vector2(sword.Width, sword.Height) : new Vector2(0, sword.Height);

            SpriteEffects effects = !flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float swordAngle = (Owner.Center - Main.MouseWorld).SafeNormalize(-Vector2.UnitY).ToRotation();

            MGRBosses.DrawBorderedRectangle(Projectile.position - Main.screenPosition, Projectile.width, Projectile.height, Color.Cyan * 0.12f, Color.LightCyan * 0.5f, Main.spriteBatch);
            Main.EntitySpriteDraw(sword, Owner.Center - Main.screenPosition, null, Color.White, swordAngle + MathHelper.PiOver2- MathHelper.PiOver4 * Projectile.ai[0], origin, 1, effects, 1);


            if(cutProgress >= 0)
                MGRBosses.Line(cutProgressPos, cutDestination, 6f, Color.Crimson);
            else
                MGRBosses.Line(cutStartPos, cutDestination, 1f, Color.LightCyan);

            MGRBosses.DrawBorderedRectangle(cutStartPos - new Vector2(4) - Main.screenPosition, 8, 8, Color.Cyan, Color.Orange, Main.spriteBatch) ;
            MGRBosses.DrawBorderedRectangle(cutDestination - new Vector2(4) - Main.screenPosition, 8, 8, Color.Cyan, Color.Orange, Main.spriteBatch) ;

            foreach(Weakspot weakspot in Weakspots)
            {
                Color weakSpotColor = weakspot.Exposed ? Color.Lime * 0.5f : Color.Red * 0.2f;
                MGRBosses.DrawBorderedRectangle(weakspot.Owner.Center + weakspot.PositionOffset - Main.screenPosition, (int)weakspot.Size.X, (int)weakspot.Size.Y, weakSpotColor, Color.White, Main.spriteBatch);
                //Main.EntitySpriteDraw(pixel, weakspot.Owner.Center + weakspot.PositionOffset - Main.screenPosition, new Rectangle(0, 0, (int)weakspot.Size.X, (int)weakspot.Size.Y), Color.Red * 0.15f, 0f, weakspot.Size * 0.5f, 1f, SpriteEffects.None, 1);
            }
        }
    }
}
