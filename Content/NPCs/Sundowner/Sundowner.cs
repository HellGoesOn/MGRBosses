using MGRBosses.Content.Systems.Arenas;
using MGRBosses.Content.Systems.Cinematic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Assets;

namespace MGRBosses.Content.NPCs.Sundowner
{
    public  class Sundowner : ModNPC
    {
        private Player PlayerTarget => Main.player[NPC.target];
        private bool initialized;

        public override string Texture => "MGRBosses/Content/Textures/ph";

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.npcSlots = 200f;
            NPC.noGravity = false;
            NPC.friendly = false;
            NPC.width = 26;
            NPC.height = 44;
            NPC.life = NPC.lifeMax = 10000;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.ai[0] = 0;
            NPC.boss = true;
            NPC.chaseable = false;

        }

        public override bool PreAI()
        {
            if(!initialized && NPC.HasPlayerTarget) {
                initialized = true;
                var targetData = NPC.GetTargetData();
                BossArenaSystem.CreateArena(new(targetData.Position + targetData.Size - new Vector2(900, 1600), new Vector2(1800, 800), NPC, Main.player[NPC.target]) { Alias = "SundownerArena" });

                var myArena = BossArenaSystem.GetArenaByAlias("SundownerArena");
                var scene = CinematicSystem.AddCinematicScene();
                scene.AddSequence(1, () =>
                {
                    NPC.position = myArena.position + new Vector2(0, myArena.size.Y) - new Vector2(-NPC.width, NPC.height);
                    Main.NewText("You can be crueler than that, Jack!");
                    scene.screenPosition = NPC.Center;
                });
                scene.AddSequence(60, () =>
                {
                });
                scene.AddSequence(60, () =>
                {
                    scene.screenPosition += (Main.LocalPlayer.Center - scene.screenPosition) * 0.1f;
                }, false);
            }

            return base.PreAI();
        }

        public override void AI()
        {
            if (!Main.dedServ) {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/RedSun");
            }

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
                NPC.TargetClosest(true);
            }

            if (PlayerTarget.dead) {
                NPC.Center -= new Vector2(0, 40);
                NPC.EncourageDespawn(10);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            spriteBatch.Draw(ModContent.Request<Texture2D>("MGRBosses/Content/Textures/ph").Value, new Rectangle((int)NPC.position.X - (int)Main.screenPosition.X, (int)NPC.position.Y - (int)Main.screenPosition.Y, NPC.width, NPC.height), Color.White); 
        }
    }
}
