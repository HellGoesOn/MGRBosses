using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Effects;
using MGRBosses.Core;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent;
using MGRBosses.Content.Projectiles;

namespace MGRBosses
{
    public class BladeModeSystem : ModSystem
    {
        internal GraphicsDevice device => Terraria.Main.graphics.GraphicsDevice;

        internal BasicEffect basicEffect;
        internal BasicEffect lineEffect;
        internal bool initialized;
        internal static bool hackyTargetNeedsUpdate;

        public static RenderTarget2D screenReplicationTarget;
        public static RenderTarget2D cuttedPositionTarget;

        internal static bool shouldUpdate;

        internal static Rectangle HackerRectangle;

        public override void Load()
        {
            Quadrilaterals = new List<Quadrilateral>();
            On.Terraria.Main.DoDraw += Main_DoDraw;
        }

        private void Main_DoDraw(On.Terraria.Main.orig_DoDraw orig, Terraria.Main self, GameTime gameTime)
        {
            if (!initialized)
            {
                fakeTex = new Texture2D(device, BladeModeProjectile.BladeModeSize, BladeModeProjectile.BladeModeSize);
                lineEffect = new BasicEffect(device);
                initialized = true;
                basicEffect = new BasicEffect(device);
                screenReplicationTarget = new RenderTarget2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight);
                cuttedPositionTarget = new RenderTarget2D(device, 600, 600);
            }
                orig.Invoke(self, gameTime);

            if(hackyTargetNeedsUpdate)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null);
                hackyTargetNeedsUpdate = false;
                device.SetRenderTarget(screenReplicationTarget);
                device.Clear(Color.Transparent);
                foreach (NPC npc in Main.npc.Where(x => x.active))
                {
                        self.DrawNPCDirect(Main.spriteBatch, npc, false, Main.screenPosition);
                }

                device.SetRenderTarget(cuttedPositionTarget);
                device.Clear(Color.Transparent);

                Main.spriteBatch.Draw(screenReplicationTarget, Vector2.Zero, HackerRectangle, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);

                device.SetRenderTarget(null);
                Main.spriteBatch.End();
            }


            /*Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.EffectMatrix);
            foreach(var v in points)
            {
                MGRBosses.Line(cuttingLineStart, cuttingLineEnd, 2f, Color.Red);
                MGRBosses.DrawBorderedRectangle(v - new Vector2(2) - Main.screenPosition, 6, 6, Color.Red * 0.5f, Color.White, Main.spriteBatch);
            }
            Main.spriteBatch.End();*/

            if (initialized)
                DrawQuads();

            old = Keyboard.GetState();
        }

        public bool drawPolygons;

        public int currentSelection;

        public KeyboardState old;

        internal static Vector2 cuttingLineStart;
        internal static Vector2 cuttingLineEnd;

        internal static List<Vector2> points = new List<Vector2>();

        private void DrawQuads()
        {
            foreach (Quadrilateral quad in Quadrilaterals)
                quad.position += quad.velocity;

            KeyboardState ks = Keyboard.GetState();

            Texture2D sword = ModContent.Request<Texture2D>("MGRBosses/Content/Textures/Monsoon/PH").Value;

            if (ks.IsKeyDown(Keys.OemCloseBrackets))
                drawPolygons = true;
            if (ks.IsKeyDown(Keys.OemOpenBrackets))
                drawPolygons = false;

            if((ks.IsKeyDown(Keys.NumPad1) && old.IsKeyUp(Keys.NumPad1)) || shouldUpdate)
            {
                shouldUpdate = false;
                List<Quadrilateral> newQuads = new List<Quadrilateral>();
                points.Clear();
                foreach(Quadrilateral q in Quadrilaterals)
                {
                    q.Breakdown(cuttingLineStart,cuttingLineEnd, cuttedPositionTarget).ForEach(x => newQuads.Add(x));
                }
                Quadrilaterals.Clear();
                foreach (Quadrilateral nq in newQuads)
                    Quadrilaterals.Add(nq);
            }

            if(ks.IsKeyDown(Keys.NumPad0) && old.IsKeyUp(Keys.NumPad0))
            {
                Main.NewText(currentSelection);

                if (++currentSelection >= Quadrilaterals.Count)
                    currentSelection = 0;
            }

            int size = BladeModeProjectile.BladeModeSize;

            if (ks.IsKeyDown(Keys.V))
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

            if (Quadrilaterals.Count > 0)
            {
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
            for (int i = 0; i < Quadrilaterals.Count; i++)
            {
                var _verts = Quadrilaterals[i].GetTextureVertices(screenReplicationTarget);
                for (int j = 0; j < _verts.Length; j++)
                {
                    texturedVerts[j + i * 6] = _verts[j];
                }
            }

            if (texturedVerts == null || texturedVerts.Length <= 0)
                return;

            vertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), Quadrilaterals.Count*6, BufferUsage.WriteOnly);
            vertexBuffer.SetData(texturedVerts);


            var screenpos = Main.screenPosition;
            Matrix world = Matrix.CreateTranslation(-new Vector3(screenpos.X, screenpos.Y, 0));
            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            var viewport = device.Viewport;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, -1);

            basicEffect.World= world;
            basicEffect.View=view;
            basicEffect.Projection = projection;
            basicEffect.Texture = cuttedPositionTarget;
            basicEffect.TextureEnabled = true;

            lineEffect.World = world;
            lineEffect.View = view;
            lineEffect.Projection = projection;
            lineEffect.VertexColorEnabled = true;

            device.SetVertexBuffer(vertexBuffer);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            device.RasterizerState = rasterizerState;
            device.SamplerStates[0] = SamplerState.PointClamp;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, Quadrilaterals.Count * 2);
            }

            if (!drawPolygons)
                return;

            VertexPositionColor[] coloredVerts = new VertexPositionColor[Quadrilaterals.Count * 6];
            for (int i = 0; i < Quadrilaterals.Count; i++)
            {
                var _verts = Quadrilaterals[i].GetColorVertices(Color.Red);
                for (int j = 0; j < _verts.Length; j++)
                {
                    coloredVerts[j + i * 6] = _verts[j];
                }
            }

            vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), Quadrilaterals.Count * 6, BufferUsage.WriteOnly);
            vertexBuffer.SetData(coloredVerts);

            device.SetVertexBuffer(vertexBuffer);

            foreach (EffectPass pass in lineEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawPrimitives(PrimitiveType.LineStrip, 0, Quadrilaterals.Count * 5);
            }

        }

        internal Texture2D fakeTex;

        public override void Unload()
        {
            On.Terraria.Main.DoDraw -= Main_DoDraw;
            basicEffect.Dispose();
            lineEffect.Dispose();
        }

        public void UpdateTex(Rectangle rect)
        {
            Color[] data = new Color[BladeModeProjectile.BladeModeSize * BladeModeProjectile.BladeModeSize];
            int offX = rect.X;
        }

        public List<Quadrilateral> Quadrilaterals;
    }
}


/*
/*
Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
Main.EntitySpriteDraw(HackyTarget, new Vector2(0, 0), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
Main.spriteBatch.End();


if (BladeModeGore.oldCount != BladeModeGore.ActiveGore.Count)
{
    device.SetRenderTarget(HackyTarget);
    device.Clear(Color.Transparent);
    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    foreach (BladeModeGore gore in BladeModeGore.ActiveGore)
    {
        self.DrawNPCDirect(Main.spriteBatch, gore.npcCopy, false, Main.screenPosition);
    }
    Main.spriteBatch.End();
    BladeModeGore.oldCount = BladeModeGore.ActiveGore.Count;
    device.SetRenderTarget(null);
}

Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
foreach (BladeModeGore gore in BladeModeGore.ActiveGore)
{
    gore.Draw();
}

BladeModeGore.ActiveGore.RemoveAll(x => x.testTimer >= 64f);

Main.spriteBatch.End();
*/