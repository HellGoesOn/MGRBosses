using MGRBosses.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace MGRBosses
{
    public class MGRBosses : Mod
    {
        public override void Load()
        {
            base.Load();

            if (!Main.dedServ) {
                Ref<Effect> screenRef = new(Assets.Request<Effect>("Content/Effects/ShockwaveEffect", AssetRequestMode.ImmediateLoad).Value);
                Filters.Scene["Shockwave"] = new Filter(new ScreenShaderData(screenRef, "Shockwave"), EffectPriority.VeryHigh);
                Filters.Scene["Shockwave"].Load();
            }
        }


        public static void TriggerParry(Vector2 visualEffectPosition)
        {
            foreach (Player plr in Main.player.Where(x => x.active && x.whoAmI != 255)) {
                plr.AddBuff(ModContent.BuffType<ParryBuff>(), 10);
            }

            for (int i = -160; i < 160; i++) {
                if (i > -10 && i < 10)
                    continue;

                int dust = Dust.NewDust(visualEffectPosition - new Vector2(i, 20), 1, 1, DustID.RedTorch);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0f;

                int dust2 = Dust.NewDust(visualEffectPosition - new Vector2(0, 20 + i), 1, 1, DustID.RedTorch);
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].velocity *= 0f;
            }
        }

        public static void Line(Vector2 a, Vector2 b, float thickness, Color cl, float extraLength = 0f)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Laser").Value;
            Vector2 tan = (b - a);
            float rot = (float)Math.Atan2(tan.Y, tan.X);
            Vector2 scale = new(tan.Length() + extraLength, thickness);
            Vector2 middleOrigin = new(0, tex.Height / 2f);

            SpriteEffects sprfx = SpriteEffects.None;

            Main.EntitySpriteDraw(tex, a - Main.screenPosition, null, cl, rot, middleOrigin, scale, sprfx, 0);
        }

        public static void DrawBorderedRectangle(Vector2 position, int width, int height, Color color, Color borderColor, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw
                 (
                     TextureAssets.MagicPixel.Value,
                     position,
                     new Rectangle(0, 0, width, height),
                     color,
                     0f,
                     Vector2.Zero,
                     1f,
                     SpriteEffects.None,
                     1
                 );

            #region Draw Borders
            spriteBatch.Draw
                     (
                         TextureAssets.MagicPixel.Value,
                         position,
                         new Rectangle(0, 0, 2, height),
                         borderColor,
                         0f,
                         Vector2.Zero,
                         1f,
                         SpriteEffects.None,
                         1
                     );
            spriteBatch.Draw
                     (
                         TextureAssets.MagicPixel.Value,
                         position,
                         new Rectangle(0, 0, width, 2),
                         borderColor,
                         0f,
                         Vector2.Zero,
                         1f,
                         SpriteEffects.None,
                         1
                     );
            spriteBatch.Draw
                     (
                         TextureAssets.MagicPixel.Value,
                         position + new Vector2(width - 2, 0),
                         new Rectangle(0, 0, 2, height),
                         borderColor,
                         0f,
                         Vector2.Zero,
                         1f,
                         SpriteEffects.None,
                         1
                     );
            spriteBatch.Draw
                     (
                         TextureAssets.MagicPixel.Value,
                         position + new Vector2(0, height - 2),
                         new Rectangle(0, 0, width, 2),
                         borderColor,
                         0f,
                         Vector2.Zero,
                         1f,
                         SpriteEffects.None,
                         1
                     );
            #endregion
        }
    }
}