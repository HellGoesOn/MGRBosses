using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public class MonsoonFog
    {
        public Vector2 position;

        public Vector2 velocity;

        public int variation;

        public bool flipped;

        public MonsoonFog()
        {
            flipped = Main.rand.NextBool();
        }

        public void Update()
        {
            position += velocity;
        }

        public void Draw(SpriteBatch spriteBatch, float opacity)
        {
            Texture2D mistTexture = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Monsoon/Mist").Value;
            Texture2D mistTexture2 = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Monsoon/Mist2").Value;
            SpriteEffects effects = flipped ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.EntitySpriteDraw(variation == 1 ? mistTexture2 : mistTexture, Main.LocalPlayer.Center - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f + position - Main.screenPosition, null, Color.Gray * opacity * 0.55f, 0f, Vector2.Zero, 2f, effects, 1);

        }
    }
}
