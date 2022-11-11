using MGRBosses.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.Systems.BladeMode
{
    public class BladeModeSystem : ModSystem
    {
        public readonly static List<Weakspot> Weakspots = new();
        public readonly static List<BladeModeGore> Gores = new();

        public int currentSelection;
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
        internal static BasicEffect BasicEffect;
        internal static BasicEffect LineBasicEffect;
        internal static bool drawPolygons;

        internal bool initialized;
        internal Texture2D fakeTex;

        private readonly List<BladeModeGore> _cachedGore = new();

        public override void Load()
        {
            if (!Main.dedServ) {
                On.Terraria.Main.DoDraw += Main_DoDraw;
                Main.OnResolutionChanged += Main_OnResolutionChanged;
            }
        }

        public override void Unload()
        {
            On.Terraria.Main.DoDraw -= Main_DoDraw;
            Main.OnResolutionChanged -= Main_OnResolutionChanged;
            Main.QueueMainThreadAction(() =>
            {

                BasicEffect.Dispose();
                LineBasicEffect.Dispose();
                fakeTex.Dispose();
            });
        }

        public override void PostUpdateGores()
        {
            if(!Main.dedServ) {
                foreach (BladeModeGore gore in Gores)
                    gore.Update();

                Gores.RemoveAll(x => x.timeSinceLastCut <= 0);
            }
        }

        public static void CacheGore(BladeModeGore goreToSpawn)
        {
            ModContent.GetInstance<BladeModeSystem>()._cachedGore.Add(goreToSpawn);
        }

        private void Main_DoDraw(On.Terraria.Main.orig_DoDraw orig, Terraria.Main self, GameTime gameTime)
        {
            if (!initialized) {
                initialized = true;
                fakeTex = new Texture2D(Device, BladeModeProjectile.BladeModeSize, BladeModeProjectile.BladeModeSize);
                LineBasicEffect = new BasicEffect(Device);
                BasicEffect = new BasicEffect(Device);
                screenReplicationTarget = new RenderTarget2D(Device, Device.PresentationParameters.BackBufferWidth, Device.PresentationParameters.BackBufferHeight);
                cuttedPositionTarget = new RenderTarget2D(Device, 600, 600);
            }

            orig.Invoke(self, gameTime);

            if (hackyTargetNeedsUpdate) {
                hackyTargetNeedsUpdate = false;

                foreach (BladeModeGore gore in _cachedGore) {

                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null);
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
                    gore.ReassignTexture(cuttedPositionTarget);
                    Gores.Add(gore);
                }

                _cachedGore.Clear();
            }

            if (initialized)
                DrawQuads();

            old = Keyboard.GetState();
        }

        private void DrawQuads()
        {
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.OemCloseBrackets))
                drawPolygons = true;
            if (ks.IsKeyDown(Keys.OemOpenBrackets))
                drawPolygons = false;

            if ((ks.IsKeyDown(Keys.NumPad1) && old.IsKeyUp(Keys.NumPad1)) || shouldUpdate) {
                shouldUpdate = false;

                foreach (BladeModeGore q in Gores) {
                    q.DoCut(cuttingLineStart, cuttingLineEnd);
                }
            }

            if (ks.IsKeyDown(Keys.NumPad0) && old.IsKeyUp(Keys.NumPad0)) {
                Main.NewText(currentSelection);

                if (++currentSelection >= Gores.Count)
                    currentSelection = 0;
            }

            foreach(BladeModeGore gore in Gores) {
                gore.Draw();
            }

            /*
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
            */
        }


        private void Main_OnResolutionChanged(Vector2 obj)
        {
            initialized = false;
        }
        /*
        public void UpdateTex(Rectangle rect)
        {
            Color[] data = new Color[BladeModeProjectile.BladeModeSize * BladeModeProjectile.BladeModeSize];
            int offX = rect.X;
        }*/

    }
}