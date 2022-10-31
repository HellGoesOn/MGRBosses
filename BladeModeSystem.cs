using MGRBosses.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses
{
    public class BladeModeSystem : ModSystem
    {
        public int currentSelection;
        public bool drawPolygons;
        public List<Quadrilateral> Quadrilaterals;
        public KeyboardState old;

        internal static bool shouldUpdate;
        internal static bool hackyTargetNeedsUpdate;

        internal static Vector2 cuttingLineStart;
        internal static Vector2 cuttingLineEnd;

        internal static Rectangle HackerRectangle;
        internal static RenderTarget2D screenReplicationTarget;
        internal static RenderTarget2D cuttedPositionTarget;
        internal static List<Vector2> points = new();
        internal static GraphicsDevice Device => Main.graphics.GraphicsDevice;

        internal bool initialized;

        internal BasicEffect basicEffect;
        internal BasicEffect lineEffect;
        internal Texture2D fakeTex;

        public override void Load()
        {
            Quadrilaterals = new List<Quadrilateral>();

            if (!Main.dedServ) {
                On.Terraria.Main.DoDraw += Main_DoDraw;
            }
        }

        public override void Unload()
        {
            On.Terraria.Main.DoDraw -= Main_DoDraw;
            basicEffect.Dispose();
            lineEffect.Dispose();
        }

        public override void PostUpdateGores()
        {
            if(!Main.dedServ) {
                foreach (Quadrilateral quad in Quadrilaterals)
                    quad.position += quad.velocity;
            }
        }

        private void Main_DoDraw(On.Terraria.Main.orig_DoDraw orig, Terraria.Main self, GameTime gameTime)
        {
            if (!initialized) {
                fakeTex = new Texture2D(Device, BladeModeProjectile.BladeModeSize, BladeModeProjectile.BladeModeSize);
                lineEffect = new BasicEffect(Device);
                initialized = true;
                basicEffect = new BasicEffect(Device);
                screenReplicationTarget = new RenderTarget2D(Device, Device.PresentationParameters.BackBufferWidth, Device.PresentationParameters.BackBufferHeight);
                cuttedPositionTarget = new RenderTarget2D(Device, 600, 600);
            }

            orig.Invoke(self, gameTime);

            if (hackyTargetNeedsUpdate) {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null);
                hackyTargetNeedsUpdate = false;
                Device.SetRenderTarget(screenReplicationTarget);
                Device.Clear(Color.Transparent);
                foreach (NPC npc in Main.npc.Where(x => x.active)) {
                    self.DrawNPCDirect(Main.spriteBatch, npc, false, Main.screenPosition);
                }

                Device.SetRenderTarget(cuttedPositionTarget);
                Device.Clear(Color.Transparent);

                Main.spriteBatch.Draw(screenReplicationTarget, Vector2.Zero, HackerRectangle, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);

                Device.SetRenderTarget(Main.screenTarget);
                Main.spriteBatch.End();
            }

            if (initialized)
                DrawQuads();

            old = Keyboard.GetState();
        }

        private void DrawQuads()
        {
            KeyboardState ks = Keyboard.GetState();

            Texture2D sword = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Monsoon/PH").Value;

            if (ks.IsKeyDown(Keys.OemCloseBrackets))
                drawPolygons = true;
            if (ks.IsKeyDown(Keys.OemOpenBrackets))
                drawPolygons = false;

            if ((ks.IsKeyDown(Keys.NumPad1) && old.IsKeyUp(Keys.NumPad1)) || shouldUpdate) {
                shouldUpdate = false;
                List<Quadrilateral> newQuads = new();
                points.Clear();
                foreach (Quadrilateral q in Quadrilaterals) {
                    q.Breakdown(cuttingLineStart, cuttingLineEnd).ForEach(x => newQuads.Add(x));
                }
                Quadrilaterals.Clear();
                foreach (Quadrilateral nq in newQuads)
                    Quadrilaterals.Add(nq);
            }

            if (ks.IsKeyDown(Keys.NumPad0) && old.IsKeyUp(Keys.NumPad0)) {
                Main.NewText(currentSelection);

                if (++currentSelection >= Quadrilaterals.Count)
                    currentSelection = 0;
            }

            int size = BladeModeProjectile.BladeModeSize;

            if (ks.IsKeyDown(Keys.V)) {
                CreateBladeModeQuadrilateral(size);
            }

            if (Quadrilaterals.Count > 0) {
                Quadrilateral quad = Quadrilaterals[currentSelection];
                if (ks.IsKeyDown(Keys.Up))
                    quad.position.Y -= 1.5f;
                if (ks.IsKeyDown(Keys.Down))
                    quad.position.Y += 1.5f;
                if (ks.IsKeyDown(Keys.Left))
                    quad.position.X -= 1.5f;
                if (ks.IsKeyDown(Keys.Right))
                    quad.position.X += 1.5f;
            }

            VertexBuffer vertexBuffer;

            VertexPositionTexture[] texturedVerts = new VertexPositionTexture[Quadrilaterals.Count * 6];

            for (int i = 0; i < Quadrilaterals.Count; i++) {
                var _verts = Quadrilaterals[i].GetTextureVertices(screenReplicationTarget);
                for (int j = 0; j < _verts.Length; j++) {
                    texturedVerts[j + i * 6] = _verts[j];
                }
            }

            if (texturedVerts == null || texturedVerts.Length <= 0)
                return;

            vertexBuffer = new VertexBuffer(Device, typeof(VertexPositionTexture), Quadrilaterals.Count * 6, BufferUsage.WriteOnly);
            vertexBuffer.SetData(texturedVerts);


            var screenpos = Main.screenPosition;
            Matrix world = Matrix.CreateTranslation(-new Vector3(screenpos.X, screenpos.Y, 0));
            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            var viewport = Device.Viewport;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, -1);

            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.Texture = cuttedPositionTarget;
            basicEffect.TextureEnabled = true;

            lineEffect.World = world;
            lineEffect.View = view;
            lineEffect.Projection = projection;
            lineEffect.VertexColorEnabled = true;

            Device.SetVertexBuffer(vertexBuffer);

            RasterizerState rasterizerState = new() {
                CullMode = CullMode.None
            };
            Device.RasterizerState = rasterizerState;
            Device.SamplerStates[0] = SamplerState.PointClamp;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Device.DrawPrimitives(PrimitiveType.TriangleList, 0, Quadrilaterals.Count * 2);
            }

            if (!drawPolygons)
                return;

            VertexPositionColor[] coloredVerts = new VertexPositionColor[Quadrilaterals.Count * 6];
            for (int i = 0; i < Quadrilaterals.Count; i++) {
                var _verts = Quadrilaterals[i].GetColorVertices(Color.Red);
                for (int j = 0; j < _verts.Length; j++) {
                    coloredVerts[j + i * 6] = _verts[j];
                }
            }

            vertexBuffer = new VertexBuffer(Device, typeof(VertexPositionColor), Quadrilaterals.Count * 6, BufferUsage.WriteOnly);
            vertexBuffer.SetData(coloredVerts);

            Device.SetVertexBuffer(vertexBuffer);

            foreach (EffectPass pass in lineEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Device.DrawPrimitives(PrimitiveType.LineStrip, 0, Quadrilaterals.Count * 5);
            }

        }

        private void CreateBladeModeQuadrilateral(int size)
        {
            Quadrilaterals.Clear();
            currentSelection = 0;
            Quadrilaterals.Add(new Quadrilateral(Main.LocalPlayer.position + new Vector2(0, -150), new Vector2[6]
            {
                    new Vector2(0, 0),
                    new Vector2(size, 0),
                    new Vector2(0, size),
                    new Vector2(size, 0),
                    new Vector2(size, size),
                    new Vector2(0, size),
            }));
        }

        /*
        public void UpdateTex(Rectangle rect)
        {
            Color[] data = new Color[BladeModeProjectile.BladeModeSize * BladeModeProjectile.BladeModeSize];
            int offX = rect.X;
        }*/

    }
}