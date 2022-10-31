using Microsoft.Xna.Framework;
using Terraria;

namespace MGRBosses.Core.BladeMode
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
}
