using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace MGRBosses.Common
{
    public class CameraOverride
    {
        public Vector2 TargetLocation;

        public Vector2 CurrentPosition;

        public Vector2 FailSafe;

        public bool Active { get; set; }

        public Entity Source { get; set; }

        public bool HasBeenReset { get; set; }

        public float Speed { get; set; } = -1.0f;

        public static bool IsDefault(CameraOverride value) => value != null && (value.TargetLocation == Vector2.Zero || value.Speed < 0);

        public CameraOverride(Vector2 currentPosition, Vector2 targetLocation)
        {
            CurrentPosition = currentPosition;
            TargetLocation = targetLocation;
        }
    }
}
