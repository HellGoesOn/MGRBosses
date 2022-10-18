using Microsoft.Xna.Framework;
using Terraria;

namespace MGRBosses.Core
{
    public static class Extensions
    {
        public static Vector3 ToVector3(this Vector2 v) => new Vector3(v.X, v.Y, 0);

		public static Vector2 FloatToInt(this Vector2 v) => new Vector2((int)v.X, (int)v.Y); 
    }
}
