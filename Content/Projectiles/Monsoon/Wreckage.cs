using BladeMode.Content.Items;
using MGRBosses.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MGRBosses.Content.Projectiles.Monsoon
{
    public class Wreckage : ModProjectile
    {
        public override string Texture => "MGRBosses/Content/Textures/Monsoon/Wreckage";

        public float scale;
        private bool launched;

        public override void SetDefaults()
        {
            scale = 0f;
            Projectile.width =
            Projectile.height = 70;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 240;
            Projectile.aiStyle = -1;
            launched = false;
        }

        private NPC monsoonNPC;

        private Vector2 positionOffset;

        private float soundTimer;

        private bool triggeredParry;

        public override void AI()
        {
            if (!launched)
            {
                triggeredParry = false;
                Projectile.timeLeft = (int)Projectile.ai[1];
                launched = true;
                Projectile.ai[1] = 0;
                monsoonNPC = Main.npc[(int)Projectile.knockBack];
                Projectile.knockBack = 0f;
                positionOffset = monsoonNPC.Center - Projectile.Center;
            }

            if (Projectile.ai[0] < 0 || Projectile.ai[0] >= 255)
            {
                Projectile.Kill();
                return;
            }

            if (scale < 1f)
                scale += 0.012f;

            if (Projectile.timeLeft > 120)
            {
                if ((soundTimer += Projectile.ai[1] * 0.75f) > (float)Math.PI)
                {
                    soundTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item169, Projectile.position);
                }
                Projectile.rotation += Projectile.ai[1];

                Projectile.Center = monsoonNPC.Center - positionOffset;

                if (Projectile.ai[1] < 1.8f)
                    Projectile.ai[1] += 0.002f;
            }

            Player target = Main.player[(int)Projectile.ai[0]];
            float targetRotation = (target.Center + target.velocity - Projectile.Center).SafeNormalize(-Vector2.UnitY).ToRotation();

            if (Projectile.timeLeft >= 100 && Projectile.timeLeft <= 120)
            {
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, targetRotation, 0.225f);
            }

            if (Projectile.timeLeft == 100)
            {
                Projectile.velocity = (target.Center + target.velocity - Projectile.Center).SafeNormalize(-Vector2.UnitY) * 16f;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            if ((Projectile.Center + Projectile.velocity).Distance(target.Center) <= 80 && !triggeredParry)
            {
                triggeredParry = true;
                MGRBosses.TriggerParry(Projectile.Center + Projectile.velocity * 2);
            }
            if (Projectile.timeLeft <= 100)
            {
                List<Projectile> bladeModes = Main.projectile.Where(x => x.active && x.ModProjectile is BladeModeProjectile).ToList();

                foreach (Projectile blade in bladeModes)
                {
                    BladeModeProjectile bl = blade.ModProjectile as BladeModeProjectile;
                    if (bl != null && bl.cutProgress > 0 && Collision.CheckAABBvLineCollision(Projectile.position, Projectile.Size, bl.cutProgressPos, bl.cutDestination))
                    {
                        Projectile.Kill();

                        int dropItemType = ModContent.ItemType<EMGrenade>();
                        int newItem = Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.Hitbox, dropItemType);
                        Main.item[newItem].noGrabDelay = 0;

                        if (Main.netMode == NetmodeID.MultiplayerClient && newItem >= 0)
                        {
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItem, 1f);
                        }
                    }
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            GarbageExplosionPlaceholderMethod();
        }


        private void GarbageExplosionPlaceholderMethod()
        {
            // Play explosion sound
            SoundEngine.PlaySound(SoundID.Item62, Projectile.position);
            // Smoke Dust spawn
            for (int i = 0; i < 50; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }
            // Fire Dust spawn
            for (int i = 0; i < 80; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= 5f;
                dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                Main.dust[dustIndex].velocity *= 3f;
            }
            // Large Smoke Gore spawn
            for (int g = 0; g < 2; g++)
            {
                int goreIndex = Gore.NewGore(Projectile.GetSource_None(), new Vector2(Projectile.position.X + (float)(Projectile.width / 2) - 24f, Projectile.position.Y + (float)(Projectile.height / 2) - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
                goreIndex = Gore.NewGore(Projectile.GetSource_None(), new Vector2(Projectile.position.X + (float)(Projectile.width / 2) - 24f, Projectile.position.Y + (float)(Projectile.height / 2) - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
                goreIndex = Gore.NewGore(Projectile.GetSource_None(), new Vector2(Projectile.position.X + (float)(Projectile.width / 2) - 24f, Projectile.position.Y + (float)(Projectile.height / 2) - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
                goreIndex = Gore.NewGore(Projectile.GetSource_None(), new Vector2(Projectile.position.X + (float)(Projectile.width / 2) - 24f, Projectile.position.Y + (float)(Projectile.height / 2) - 24f), default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            SpriteEffects effects = Projectile.velocity.X > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(67, 32), scale, effects, 1);
            if(Projectile.timeLeft > 100)
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.Purple * scale * 0.75f, Projectile.rotation, new Vector2(67, 32), scale + 0.12f, effects, 1);
        }
    }
}
