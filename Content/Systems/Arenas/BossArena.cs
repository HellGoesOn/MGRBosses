using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace MGRBosses.Content.Systems.Arenas
{
    public class BossArena
    {
        public readonly List<Entity> Participants = new();

        public NPC Boss;

        public Vector2 position;

        public Vector2 size;

        public Vector2 Center => position + size * 0.5f;

        public bool initialized;

        public string Alias { get; set; }

        public int Id;

        public BossArena(Vector2 position, Vector2 size, NPC boss, List<Player> participants)
        {
            this.position = position;
            this.size = size;
            this.Participants.Add(boss);
            Boss = boss;
            participants.ForEach(x => Participants.Add(x));
        }

        public BossArena(Vector2 position, Vector2 size, NPC boss, Player soleParticipant)
        {
            this.position = position;
            this.size = size;
            this.Participants.Add(boss);
            Boss = boss;
            Participants.Add(soleParticipant);
        }
    }
}
