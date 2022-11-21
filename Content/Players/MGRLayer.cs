using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MGRBosses.Content.Players
{
    public class MGRLayer : PlayerDrawLayer
    {
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return true;
        }

        public override Position GetDefaultPosition()
        {
            return new BeforeParent(PlayerDrawLayers.ArmOverItem);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var plr = drawInfo.drawPlayer.GetModPlayer<MGRPlayer>();
            if (plr.parryTime <= 0 || drawInfo.drawPlayer.HeldItem.type <= 0)
                return;

            Player drawPlayer = drawInfo.drawPlayer;
            int dir_m = drawPlayer.direction; // Direction multiplier

            var texture = TextureAssets.Item[drawPlayer.HeldItem.type].Value;
            var vec = new Vector2((int)drawPlayer.Center.X, (int)drawPlayer.Center.Y);

            DrawData drawData = new(
                texture,
                vec + new Vector2(-18 * dir_m, -24)- Main.screenPosition,
                null,
                drawInfo.itemColor,
                -(MathHelper.PiOver2 + MathHelper.PiOver4 + 0.24f + plr.visualDecay) * -dir_m,
                new Vector2((dir_m == -1 ? 0 : texture.Width), texture.Height) ,
                1,
                dir_m == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                1
                );

            drawInfo.DrawDataCache.Add(drawData);
        }
    }
}
