using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MGRBosses.Content.NPCs
{
    public partial class MonsoonBoss : ModNPC
    {
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = NPC.HasPlayerTarget ? (NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally) : SpriteEffects.None;

            Color clr = IsMagnetized ? Color.Purple : drawColor;

            switch (state)
            {
                case AIState.SmokeAttack:
                    if (monsoonOpacity <= targetOpacity + 0.01f)
                        DrawBody(effects, Color.White * monsoonOpacity);

                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Gray * fogDensity);

                    if (monsoonOpacity > targetOpacity + 0.01f)
                        DrawBody(effects, drawColor);
                    break;
            }

            if (state != AIState.SmokeAttack)
            {
                bool pantsActive = pantsId != -1;
                if (pantsActive)
                {
                    NPC pants = Main.npc[pantsId];

                    if (pantsActive)
                        DrawBody(effects, clr, false, true, 0f, pants.Center);
                }
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Gray * fogDensity);
                DrawBody(effects, clr, true, !pantsActive);
            }

            if (fogDensity <= 0)
                return;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < monsoonFog.Count; i++)
            {
                monsoonFog[i].Draw(spriteBatch, fogDensity);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public void DrawBody(SpriteEffects effects, Color clr, bool drawTorso = true, bool drawLegs = true, float overrideRotation = 0f, Vector2 changedPos = default)
        {
            Texture2D texBody = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Monsoon/PH_Torso").Value;
            Texture2D texLegs = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Monsoon/PH_Legs").Value;

            Vector2 drawPos = NPC.Center;
            if (changedPos != default)
                drawPos = changedPos;

            if(drawTorso)
                Main.EntitySpriteDraw(texBody, drawPos - Main.screenPosition, null, clr * monsoonOpacity, overrideRotation != 0f ? overrideRotation : rotation, new Vector2(13, 22), Vector2.One, effects, 1);

            if(drawLegs)
                Main.EntitySpriteDraw(texLegs, drawPos - Main.screenPosition, null, clr * monsoonOpacity, overrideRotation != 0f ? overrideRotation : rotation, new Vector2(13, 22), Vector2.One, effects, 1);
        }
    }
}
