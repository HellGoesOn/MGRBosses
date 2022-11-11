using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.Systems.Arenas
{
    public class BossArenaSystem : ModSystem
    {
        public List<BossArena> Arenas;

        public override void Load()
        {
            Arenas = new List<BossArena>();
        }

        public override void PostUpdateEverything()
        {
            foreach (var arena in Arenas) {

                if (!arena.initialized) {
                    foreach (var participants in arena.Participants)
                        participants.Center = arena.Center;
                }

                foreach (var participant in arena.Participants) {
                    if (!arena.initialized) {
                        participant.Center = arena.Center;
                    }
                    participant.position = Vector2.Clamp(participant.position, arena.position, arena.position + arena.size - new Vector2(participant.width, participant.height));

                    if (participant is Player plr && participant.Bottom.Y >= arena.position.Y + arena.size.Y) {
                        participant.velocity.Y = 0;
                    }
                }

                arena.initialized = true;

                arena.Participants.RemoveAll(x => !x.active);
            }

            Arenas.RemoveAll(x => !x.Boss.active || !x.Participants.Any(x => x is Player plr && !plr.dead));
        }

        public override void PostDrawTiles()
        {
            Main.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive);
            foreach (var arena in Arenas) {
                MGRBosses.DrawBorderedRectangle(arena.position - new Vector2(8, 0) - Main.screenPosition, 8, (int)arena.size.Y + 8, Color.Orange * 0.5f, Color.OrangeRed * 0.75f);
                MGRBosses.DrawBorderedRectangle(arena.position - new Vector2(8, 0) - Main.screenPosition, (int)arena.size.X + 16, 8, Color.Orange * 0.5f, Color.OrangeRed * 0.75f);
                MGRBosses.DrawBorderedRectangle(arena.position - new Vector2(0, 0) + new Vector2(arena.size.X, 0) - Main.screenPosition, 8, (int)arena.size.Y + 8, Color.Orange * 0.5f, Color.OrangeRed * 0.75f);
                MGRBosses.DrawBorderedRectangle(arena.position - new Vector2(8, 0) + new Vector2(0, arena.size.Y) - Main.screenPosition, (int)arena.size.X + 16, 8, Color.Orange * 0.5f, Color.OrangeRed * 0.75f);
            }
            Main.spriteBatch.End();
        }

        public static BossArena GetArenaByAlias(string alias)
        {
            ref var arenas = ref ModContent.GetInstance<BossArenaSystem>().Arenas;

            return arenas.Find(x => x.Alias == alias);
        }

        public static int CreateArena(BossArena arena)
        {
            ref var arenas = ref ModContent.GetInstance<BossArenaSystem>().Arenas;
            arenas.Add(arena);
            return arenas.Count - 1;
        }
    }
}
