using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;

namespace MGRBosses.Content.Systems.BladeMode
{
    public class Weakspot
    {
        public Entity Owner;

        public Vector2 PositionOffset;

        public Vector2 Size;

        public bool Exposed;

        public int Id;

        public WeakspotEventHandler OnWeakspotCut;

        public Weakspot(Entity owner, Vector2 offset, Vector2 size)
        {
            Owner = owner;
            PositionOffset = offset;
            Size = size;
            Exposed = false;
        }

        public static Weakspot Create(Entity owner, Vector2 offset, Vector2 size)
        {
            Weakspot result = new(owner, offset, size);
            BladeModeSystem.Weakspots.Add(result);
            result.Id = BladeModeSystem.Weakspots.Count - 1;
            return result;
        }

        public static bool ExistsFor(Entity owner) => BladeModeSystem.Weakspots.Any(x => x.Owner == owner);

        public static void Remove(Weakspot weakspot)
        {
            weakspot.OnWeakspotCut?.Invoke(weakspot.Owner);
            BladeModeSystem.Weakspots.Remove(weakspot);
        }

        public static void Remove(int id)
        {
            Weakspot spot = BladeModeSystem.Weakspots[id];
            spot.OnWeakspotCut?.Invoke(spot.Owner);
            BladeModeSystem.Weakspots.RemoveAt(id);
        }
    }
}
