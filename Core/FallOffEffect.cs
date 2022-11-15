using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace MGRBosses.Core
{
    public class FallOffEffect
    {
        public static List<FallOffEffect> FallOffEffects { get; } = new List<FallOffEffect>();

        private float thickness;

        private Vector2 startPos;

        private Vector2 destination;

        private Vector2 endPos;

        public FallOffEffect(Vector2 s, Vector2 d)
        {
            startPos = s;
            endPos = s;
            destination = d;
            thickness = 4.24f;
        }

        public void Update()
        {
            thickness -= 0.12f;
            endPos = Vector2.SmoothStep(endPos, destination, 0.4f);
            startPos = Vector2.SmoothStep(startPos, destination, 0.24f);
        }

        public void Draw()
        {
            var off = (startPos - endPos).SafeNormalize(-Vector2.UnitY) * 8f;
            MGRBosses.DrawLine(startPos + off, endPos, thickness, Color.OrangeRed * 0.8f);
            MGRBosses.DrawLine(startPos, endPos, thickness * 0.4f, Color.LightYellow * 1f);
        }

        public static void UpdateAll()
        {
            foreach (var foe in FallOffEffects)
                foe.Update();

            FallOffEffects.RemoveAll(x => x.thickness <= 0);
        }

        public static void DrawEffects()
        {
            foreach (var foe in FallOffEffects)
                foe.Draw();
        }

        public static void Add(Vector2 startPoint, Vector2 endPoint)
        {
            FallOffEffects.Add(new FallOffEffect(startPoint, endPoint));
        }
    }
}
