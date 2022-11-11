using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace MGRBosses.Content.Systems.BladeMode
{
    public class BladeModeGore
    {
        public int timeSinceLastCut = 300;

        public Texture2D texture;

        private readonly List<Quadrilateral> quadrilaterals = new();

        public BladeModeGore(Texture2D texture, Vector2 position, float size)
        {
            this.texture = new(Main.graphics.GraphicsDevice, (int)size*2, (int)size*2);
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            this.texture.SetData(data);

            quadrilaterals.Add(new Quadrilateral(position, new Vector2[6]
            {
                    new Vector2(0, 0),
                    new Vector2(size, 0),
                    new Vector2(0, size),
                    new Vector2(size, 0),
                    new Vector2(size, size),
                    new Vector2(0, size),
            }));
        }

        public void ReassignTexture(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            this.texture.SetData(data);
        }

        public void Update()
        {
            foreach(Quadrilateral quad in quadrilaterals) {
                quad.position += quad.velocity;
            }

            timeSinceLastCut--;
        }

        public void DoCut(Vector2 lineStart, Vector2 lineEnd)
        {
            timeSinceLastCut = 300;
            List<Quadrilateral> newQuads = new();
            foreach (Quadrilateral q in quadrilaterals) {
                q.Breakdown(lineStart, lineEnd).ForEach(x => newQuads.Add(x));
            }
            quadrilaterals.Clear();
            foreach (Quadrilateral nq in newQuads)
                quadrilaterals.Add(nq);
        }

        public void Draw()
        {
            GraphicsDevice _device = Main.graphics.GraphicsDevice;
            BasicEffect basicEffect = BladeModeSystem.BasicEffect;
            BasicEffect lineEffect = BladeModeSystem.LineBasicEffect;
            VertexBuffer vertexBuffer;

            VertexPositionTexture[] texturedVerts = new VertexPositionTexture[quadrilaterals.Count * 6];

            for (int i = 0; i < quadrilaterals.Count; i++) {
                var _verts = quadrilaterals[i].GetTextureVertices(BladeModeSystem.screenReplicationTarget);
                for (int j = 0; j < _verts.Length; j++) {
                    texturedVerts[j + i * 6] = _verts[j];
                }
            }

            if (texturedVerts == null || texturedVerts.Length <= 0)
                return;

            vertexBuffer = new VertexBuffer(_device, typeof(VertexPositionTexture), quadrilaterals.Count * 6, BufferUsage.WriteOnly);
            vertexBuffer.SetData(texturedVerts);


            var screenpos = Main.screenPosition;
            Matrix world = Matrix.CreateTranslation(-new Vector3(screenpos.X, screenpos.Y, 0));
            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            var viewport = _device.Viewport;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, -1);

            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;

            lineEffect.World = world;
            lineEffect.View = view;
            lineEffect.Projection = projection;
            lineEffect.VertexColorEnabled = true;

            _device.SetVertexBuffer(vertexBuffer);

            RasterizerState rasterizerState = new() {
                CullMode = CullMode.None
            };
            _device.RasterizerState = rasterizerState;
            _device.SamplerStates[0] = SamplerState.PointClamp;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                _device.DrawPrimitives(PrimitiveType.TriangleList, 0, quadrilaterals.Count * 2);
            }

            if (!BladeModeSystem.drawPolygons)
                return;

            VertexPositionColor[] coloredVerts = new VertexPositionColor[quadrilaterals.Count * 6];
            for (int i = 0; i < quadrilaterals.Count; i++) {
                var _verts = quadrilaterals[i].GetColorVertices(Color.Red);
                for (int j = 0; j < _verts.Length; j++) {
                    coloredVerts[j + i * 6] = _verts[j];
                }
            }

            vertexBuffer = new VertexBuffer(_device, typeof(VertexPositionColor), quadrilaterals.Count * 6, BufferUsage.WriteOnly);
            vertexBuffer.SetData(coloredVerts);

            _device.SetVertexBuffer(vertexBuffer);

            foreach (EffectPass pass in lineEffect.CurrentTechnique.Passes) {
                pass.Apply();
                _device.DrawPrimitives(PrimitiveType.LineStrip, 0, quadrilaterals.Count * 5);
            }
        }
    }
}
