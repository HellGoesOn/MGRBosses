using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MGRBosses.Content.Projectiles.Monsoon
{
    public class RedPhosphorDoodad : ModProjectile
    {
        public override string Texture => "MGRBosses/Content/Textures/Monsoon/RedPhosphor";

        private float rotation;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
            Projectile.damage = 0;
        }

        public override void AI()
        {
            base.AI();

            Projectile.velocity.X *= 0.99f;
            Projectile.velocity.Y += 0.45f;
            int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.GemEmerald, 0, 0);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0f;

            rotation += 0.17f * (Projectile.velocity.X < 0 ? -1 : 1);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glow = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Monsoon/RedPhosphor_Glow").Value;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, rotation, new Vector2(8), 1f, SpriteEffects.None, 1);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.White, rotation, new Vector2(8), 1f, SpriteEffects.None, 1);
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            for (int g = 0; g < 2; g++) {
                int goreIndex = Gore.NewGore(NPC.GetSource_None(), Projectile.Center, default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity *= 1.5f;
                goreIndex = Gore.NewGore(NPC.GetSource_None(), Projectile.Center, default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity *= 1.5f;
                goreIndex = Gore.NewGore(NPC.GetSource_None(), Projectile.Center, default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity *= 1.5f;
                goreIndex = Gore.NewGore(NPC.GetSource_None(), Projectile.Center, default, Main.rand.Next(61, 64), 1f);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity *= 1.5f;
            }
        }
    }
}
