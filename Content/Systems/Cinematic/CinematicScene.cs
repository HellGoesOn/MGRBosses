using MGRBosses.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.Systems.Cinematic
{
    public class CinematicScene
    {
        public Vector2 screenPosition;

        public readonly List<Entity> Actors;
        private readonly List<Sequence> sequences;

        public CinematicScene()
        {
            sequences = new();
            Actors = new();
        }

        public void Update()
        {
            if(sequences.Count > 0) {
                Sequence currentSequence = sequences[0];

                currentSequence.Update();

                if(--currentSequence.timeLeft <= 0) {
                    sequences.RemoveAt(0);
                }

                foreach(var e in Actors) {
                    if (!e.active)
                        continue;

                    if(e is Player plr) {
                        plr.controlDown = false;
                        plr.BlockInputs();
                    }
                }
            }
        }

        public void AddSequence(int time, Action action, bool blocksInput = true)
        {
            sequences.Add(new(time, action, blocksInput));
        }

        public static bool IsActor(Entity entity, out CinematicScene result)
        {
            result = ModContent.GetInstance<CinematicSystem>().Scenes.FirstOrDefault(x => x.Actors.Contains(entity));

            return result != null;
        }

        public static bool IsActor(Entity entity)
            => ModContent.GetInstance<CinematicSystem>().Scenes.Any(x => x.Actors.Contains(entity));

        public Vector2 ScreenPosition => screenPosition - new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);

        public bool HasEnded => sequences.Count <= 0;

        public bool SequenceBlocksInput => sequences[0].blocksInput;
    }
}
