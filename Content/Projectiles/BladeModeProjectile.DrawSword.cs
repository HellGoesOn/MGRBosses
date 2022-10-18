using MGRBosses.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.Projectiles
{
    public partial class BladeModeProjectile : ModProjectile
    {
        public void InitializeSword()
        {
            endAngle = MathHelper.PiOver2-MathHelper.PiOver4-0.01f;
            startAngle = -MathHelper.PiOver2+ MathHelper.PiOver4 + 0.01f;
        }

        public void SetSwordAngle()
        {
            Utils.Swap(ref startAngle, ref endAngle);
        }

        public void GetSwordAngle()
        {
            float angleToMouse = (Main.MouseWorld - Owner.Center).SafeNormalize(-Vector2.UnitY).ToRotation();
            swordAngle = Utils.AngleLerp(startAngle + angleToMouse, endAngle + angleToMouse, 1.0f -cutProgress);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (-MathHelper.PiOver2 + swordAngle));
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, (-MathHelper.PiOver2 + swordAngle));
        }

        private float swordAngle;

        private float endAngle;

        private float startAngle;

        public void DrawSword()
        {
            Texture2D sword = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Items/Murasama").Value;

            bool flip = directionFixer == -1;
            Vector2 origin = !flip ? new Vector2(0, 0) : new Vector2(0, sword.Height);

            SpriteEffects effects = !flip ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(sword, Owner.Center.FloatToInt() + new Vector2(10, 0).FloatToInt().RotatedBy(swordAngle) - Main.screenPosition, null, Color.White, swordAngle, origin, 1, effects, 1);
            
        }
    }
}
