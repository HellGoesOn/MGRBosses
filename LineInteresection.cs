using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGRBosses
{
    public class LineInteresection
    {
        public struct Line
        {
            public float x1;
            public float y1;

            public float x2;
            public float y2;

            public Line(float startX, float startY, float endX, float endY)
            {
                x1 = startX;
                y1 = startY;
                x2 = endX;
                y2 = endY;
            }

            public Line(Vector2 start, Vector2 end)
            {
                x1 = start.X;
                y1 = start.Y;
                x2 = end.X;
                y2 = end.Y;
            }
        }

        public static Line GetLine(Vector2 v1, Vector2 v2) => new Line(v1.X, v1.Y, v2.X, v2.Y);

        public static bool Intersects(Line left, Line right, out Vector2 intersectionPoint)
        {
            float A1 = left.y2 - left.y1;
            float B1 = left.x1 - left.x2;
            float C1 = A1 * left.x1 + B1 * left.y1;

            float A2 = right.y2 - right.y1;
            float B2 = right.x1 - right.x2;
            float C2 = A2 * right.x1 + B2 * right.y1;

            float det = A1 * B2 - A2 * B1;

            if (det == 0)
            {
                intersectionPoint = default;
                return false;
            }
            else
            {

                float x = (B2 * C1 - B1 * C2) / det;
                float y = (A1 * C2 - A2 * C1) / det;


                if (x >= Math.Min(left.x1, right.x1) && x <= Math.Max(left.x2, right.x2)
                    && y >= Math.Min(left.y1, right.y1) && y <= Math.Max(left.y2, right.y2))
                {
                    intersectionPoint = new Vector2(x, y);

                    return true;
                }

                intersectionPoint = default;
                return false;
            }
        }
    }
}
