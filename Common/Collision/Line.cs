using Microsoft.Xna.Framework;

namespace MGRBosses.Common.Collision
{

    public struct Line
    {
        public float startX;
        public float startY;

        public float endX;
        public float endY;

        public Line(float startX, float startY, float endX, float endY)
        {
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
        }

        public Line(Vector2 start, Vector2 end)
        {
            startX = start.X;
            startY = start.Y;
            endX = end.X;
            endY = end.Y;
        }
    }
}
