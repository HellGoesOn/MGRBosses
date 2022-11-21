using MGRBosses.Common;
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
        private Vector2 frontMachetePosition;
        private Vector2 backMachetePosition;

        private float frontMacheteAngle;
        private float backMacheteAngle;
        private float dist;

        private TriBallJoint FrontArm;

        private float macheteAttackAngle;

        private int phase;

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
            backMacheteAngle = 0.12f;
            frontMacheteAngle = 0.52f;

            FrontArm = new TriBallJoint();
            FrontArm.minDistance = 16;
            FrontArm.maxDistance = 32;
            FrontArm.origin = NPC.Center + new Vector2(-24 * NPC.direction, -4);
            FrontArm.destination = FrontArm.origin + new Vector2(0, 16);
        }

        public override bool PreAI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
                NPC.TargetClosest(true);
            }

            if (PlayerTarget.dead) {
                NPC.Center -= new Vector2(0, 40);
                NPC.EncourageDespawn(10);
            }
            if (!initialized && NPC.HasPlayerTarget) {
                initialized = true;
                var targetData = NPC.GetTargetData();
                BossArenaSystem.CreateArena(new(targetData.Position + targetData.Size - new Vector2(900, 1200), new Vector2(1800, 800), NPC, Main.player[NPC.target]) { Alias = "SundownerArena" });

                var myArena = BossArenaSystem.GetArenaByAlias("SundownerArena");
                var scene = CinematicSystem.AddCinematicScene();
                scene.Actors.Add(NPC);
                scene.AddSequence(1, () =>
                {
                    NPC.position = myArena.position + new Vector2(0, myArena.size.Y) - new Vector2(-NPC.width-30, NPC.height);
                    FrontArm.origin = NPC.Center + new Vector2(0, -4);
                    FrontArm.destination = NPC.Center + new Vector2(-28, 16);
                    Main.NewText("You can be crueler than that, Jack!");
                    scene.screenPosition = NPC.Center;
                });
                scene.AddSequence(30, null, false);
                scene.AddSequence(90, () =>
                {
                    FrontArm.destination = Vector2.Lerp(FrontArm.destination, NPC.Center + new Vector2(0, -4) + new Vector2(90, 0), 0.1f);
                    scene.screenPosition = NPC.Center;
                }, false);
                scene.AddSequence(60, () =>
                {
                    FrontArm.destination = Vector2.Lerp(FrontArm.destination, NPC.Center + new Vector2(0, -40), 0.1f);
                    scene.screenPosition += (Main.LocalPlayer.Center - scene.screenPosition) * 0.1f;
                }, false);
            }
            NPC.direction = NPC.Center.X > PlayerTarget.Center.X ? -1 : 1;

            return base.PreAI();
        }

        public override void AI()
        {
            if (!Main.dedServ) {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/RedSun");
            }

            if (macheteAttackAngle > -MathHelper.PiOver2 - 0.5f)
                macheteAttackAngle -= 0.18f;
        }

        public override void PostAI()
        {
            backMachetePosition = NPC.Center + new Vector2(4 * NPC.direction, 0).RotatedBy(macheteAttackAngle * NPC.direction) - new Vector2(0, 4 - 10 * macheteAttackAngle);
            //frontMachetePosition

            if (Main.LocalPlayer.controlUseItem)
                dist += 0.5f;
            if (Main.LocalPlayer.controlUseTile)
                dist -= 0.5f;

            frontMachetePosition = FrontArm.EndPoint;
            frontMacheteAngle = FrontArm.AdditionalAngle(FrontArm.Angle - MathHelper.PiOver4 * NPC.direction, FrontArm.Angle + MathHelper.PiOver4 );
            backMacheteAngle += 0.02f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var macheteTex = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Pincer").Value;
            var backPos = new Vector2((int)(backMachetePosition.X) - Main.screenPosition.X, (int)(backMachetePosition.Y) - Main.screenPosition.Y);
            var frontPos = new Vector2((int)(frontMachetePosition.X) - Main.screenPosition.X, (int)(frontMachetePosition.Y) - Main.screenPosition.Y);
            var origin = new Vector2(NPC.direction == -1 ? macheteTex.Width : 0, macheteTex.Height);
            var fx = NPC.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (phase <= 0) {
                //spriteBatch.Draw(macheteTex, backPos, null, Color.White, (MathHelper.PiOver4 + backMacheteAngle + macheteAttackAngle) * NPC.direction, origin, 1f, fx, 1f);
                spriteBatch.Draw(ModContent.Request<Texture2D>("MGRBosses/Content/Textures/ph").Value, new Rectangle((int)NPC.position.X - (int)Main.screenPosition.X, (int)NPC.position.Y - (int)Main.screenPosition.Y, NPC.width, NPC.height), Color.White);
                spriteBatch.Draw(macheteTex, frontPos, null, Color.White, frontMacheteAngle, origin, 1f, fx, 1f);
                MGRBosses.DrawBorderedRectangle(FrontArm.origin - new Vector2(1) - Main.screenPosition, 4, 4, Color.Aqua, Color.Blue);
                MGRBosses.DrawBorderedRectangle(FrontArm.destination - new Vector2(1) - Main.screenPosition, 4, 4, Color.Red, Color.Violet);
                MGRBosses.DrawBorderedRectangle(FrontArm.EndPoint - new Vector2(1) - Main.screenPosition, 4, 4, Color.Lime, Color.Green);
            }
        }
    }
}
