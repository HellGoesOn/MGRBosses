using Microsoft.Xna.Framework;
using System;

namespace MGRBosses.Common.Collision
{
    public class LineCollision
    {
        public static Line GetLine(Vector2 v1, Vector2 v2) => new(v1.X, v1.Y, v2.X, v2.Y);

        public static bool Intersection(Line a, Line b, out Vector2 intersection)
        {
            intersection = new Vector2();

            float div = ((b.endY - b.startY) * (a.endX - a.startX) - (b.endX - b.startX) * (a.endY - a.startY));

            float uA = ((b.endX - b.startX) * (a.startY - b.startY) - (b.endY - b.startY) * (a.startX - b.startX)) / div;
            float uB = ((a.endX - a.startX) * (a.startY - b.startY) - (a.endY - a.startY) * (a.startX - b.startX)) / div;

            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1) {
                float x = a.startX + (uA * (a.endX - a.startX));
                float y = a.startY + (uA * (a.endY - a.startY));
                intersection = new Vector2(x, y);

                return true;
            }

            return false; // collision failed, we'll get 'em next frame
        }

        public static bool Intersects(Line left, Line right, out Vector2 intersectionPoint)
        {
            float A1 = left.endY - left.startY;
            float B1 = left.startX - left.endX;
            float C1 = A1 * left.startX + B1 * left.startY;

            float A2 = right.endY - right.startY;
            float B2 = right.startX - right.endX;
            float C2 = A2 * right.startX + B2 * right.startY;

            float det = A1 * B2 - A2 * B1;

            if (det == 0) {
                intersectionPoint = default;
                return false;
            } else {

                float x = (B2 * C1 - B1 * C2) / det;
                float y = (A1 * C2 - A2 * C1) / det;


                if (x >= Math.Min(left.startX, right.startX) && x <= Math.Max(left.endX, right.endX)
                    && y >= Math.Min(left.startY, right.startY) && y <= Math.Max(left.endY, right.endY)) {
                    intersectionPoint = new Vector2(x, y);

                    return true;
                }

                intersectionPoint = default;
                return false;
            }
        }
    }
}
