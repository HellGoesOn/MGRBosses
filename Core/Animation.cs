using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Core
{
    public enum FlipDirection
    {
        Horizontal,
        Vertical
    }
    public class SpriteAnimation
    {
        public string TexturePath { get; set; }
        public int FrameCount { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public int FrameCurrent { get; set; }
        public float FrameSpeed { get; set; } = 0.2f;
        public Vector2 Origin { get; set; }
        public Color Tint { get; set; } = Color.White;
        public Vector2 Scale { get; set; } = Vector2.One;

        private float _frameTicks;

        public SpriteAnimation(string texturePath)
        {
            TexturePath = texturePath;
        }

        public void Update()
        {
            if((_frameTicks += FrameSpeed) > 1.0f) {
                if (++FrameCurrent >= FrameCount) {
                    FrameCurrent = 0;
                }
                _frameTicks = 0.0f;
            }
        }

        public void Draw(Vector2 position, float rotation = 0f, SpriteEffects sfx = SpriteEffects.None)
        {
            Texture2D tex = ModContent.Request<Texture2D>(TexturePath).Value;
            Rectangle frame = new(0, FrameHeight * FrameCurrent, FrameWidth, FrameHeight);

            if (Tint == default)
                Tint = Color.White;

            if (tex == null)
                return;

            Main.spriteBatch.Draw(tex, position - Main.screenPosition, frame, Tint, rotation, Origin, Scale, sfx, 1f);
        }
    }
}
