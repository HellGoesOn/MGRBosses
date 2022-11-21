using MGRBosses.Core;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace MGRBosses.Common
{
    public class TriBallJoint
    {
        public float maxDistance;
        public float minDistance;
        public Vector2 origin;
        public Vector2 destination;

        public float Angle => (destination - origin).ToRotation();

        public float AdditionalAngle(float baseAngle, float targetAngle)
        {
            if (Vector2.Distance(origin, EndPoint) < maxDistance - 0.01f)
                return baseAngle;

            var dist = (destination - EndPoint).Length();
            dist = MathHelper.Clamp(dist, 0, maxDistance);

            return MathHelper.Lerp(baseAngle, targetAngle, (float)(dist / maxDistance));
        }

        public Vector2 EndPoint {
            get {
                var dir = (destination-origin).SafeNormalize(-Vector2.UnitY);
                var dist = (destination - origin).Length();
                var pos = origin + dir * MathHelper.Clamp(dist, minDistance, maxDistance);
                return pos;
            }
        }
    }
}
