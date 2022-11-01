using MGRBosses.Common.Collision;
using MGRBosses.Core;
using MGRBosses.Core.BladeMode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MGRBosses
{
    public class Quadrilateral
    {
        public Color[] Colors { get; set; }

        public Vector2 position;

        public Vector2 velocity;

        private readonly Vector2[] _vertices;

        public Quadrilateral(Vector2 pos, Vector2[] verts)
        {
            position = pos;
            _vertices = verts;
        }

        public VertexPositionColorTexture[] GetTextureColorVertices(Texture2D sampledTexture)
        {
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[_vertices.Length];

            var texWidth = sampledTexture.Width;
            var texHeight = sampledTexture.Height;

            for (int i = 0; i < _vertices.Length; i++) {
                var vertexPosition = position + _vertices[i];

                float normalizedX = (_vertices[i].X) / (float)texWidth;
                normalizedX = Math.Clamp(normalizedX, 0.0f, 1.0f);

                float normalizedY = (_vertices[i].Y) / (float)texHeight;
                normalizedY = Math.Clamp(normalizedY, 0.0f, 1.0f);

                var textureMapping = new Vector2(normalizedX, normalizedY);
                vertices[i] = new VertexPositionColorTexture(vertexPosition.ToVector3(), Color.White, textureMapping);
            }

            return vertices;
        }

        public VertexPositionTexture[] GetTextureVertices(Texture2D sampledTexture)
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[_vertices.Length];

            var texWidth = sampledTexture.Width;
            var texHeight = sampledTexture.Height;

            for (int i = 0; i < _vertices.Length; i++) {
                var vertexPosition = position + _vertices[i];

                float normalizedX = (_vertices[i].X) / (float)texWidth;
                normalizedX = Math.Clamp(normalizedX, 0.0f, 1.0f);

                float normalizedY = (_vertices[i].Y) / (float)texHeight;
                normalizedY = Math.Clamp(normalizedY, 0.0f, 1.0f);

                var textureMapping = new Vector2(normalizedX, normalizedY);
                vertices[i] = new VertexPositionTexture(vertexPosition.ToVector3(), textureMapping);
            }

            return vertices;
        }

        public VertexPositionColor[] GetColorVertices(Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[_vertices.Length];

            for (int i = 0; i < _vertices.Length; i++) {
                var vertexPosition = position + _vertices[i];

                var clr = Colors == null || Colors.Length <= 0 ? color : Colors[i / 3];

                vertices[i] = new VertexPositionColor(vertexPosition.ToVector3(), clr);
            }

            return vertices;
        }

        public List<Quadrilateral> Breakdown(Vector2 cuttingLineStart, Vector2 cuttingLineEnd)
        {
            List<Quadrilateral> result = new() {
                this
            };

            List<Vector2> intersectionPoints = new();
            List<Line> lines = new()
            {
                new Line(position + _vertices[0], position + _vertices[1]),
                new Line(position + _vertices[5], position + _vertices[4]),
                new Line(position + _vertices[0], position + _vertices[2]),
                new Line(position + _vertices[1], position + _vertices[4])
            };

            bool[] intersectedSides = new bool[lines.Count];

            for (int i = 0; i < lines.Count; i++) {
                var checkedLine = lines[i];
                Line cuttingLine = new(cuttingLineEnd, cuttingLineStart);

                if (LineCollision.Intersection(cuttingLine, checkedLine, out var hector)) {
                    intersectionPoints.Add(hector);
                    intersectedSides[i] = true;
                }
            }

            bool intersectedParallelHorizontal = intersectedSides[2] && intersectedSides[3];
            bool intersectedParallelVertical = intersectedSides[0] && intersectedSides[1];

            if (intersectionPoints.Count != 2)
                return result;

            if (intersectedParallelHorizontal || intersectedParallelVertical) {
                float speed = 0.05f;
                if (intersectedParallelHorizontal) {
                    var newQuad = new Quadrilateral(position, new Vector2[]
                    {
                    _vertices[0],
                    _vertices[1],
                    intersectionPoints[0]-position,
                    _vertices[3],
                    intersectionPoints[1]-position,
                    intersectionPoints[0]-position
                    }) {
                        Colors = new Color[]
                        {
                            Color.Red, Color.Blue
                        }
                    };

                    newQuad.velocity += new Vector2(0, -speed);

                    var newQuad2 = new Quadrilateral(position, new Vector2[]
                    {
                    intersectionPoints[0]-position,
                    intersectionPoints[1]-position,
                    _vertices[5],
                    intersectionPoints[1]-position,
                    _vertices[4],
                    _vertices[5]
                    }) {
                        Colors = new Color[]
                        {
                            Color.Green, Color.Yellow
                        }
                    };

                    newQuad2.velocity += new Vector2(0, speed);

                    result.Clear();
                    result.Add(newQuad);
                    result.Add(newQuad2);
                }

                if (intersectedParallelVertical) {
                    var newQuad = new Quadrilateral(position, new Vector2[]
                    {
                    _vertices[0],
                    intersectionPoints[0]-position,
                    _vertices[2],
                    intersectionPoints[0]-position,
                    intersectionPoints[1]-position,
                    _vertices[5]
                    }) {
                        Colors = new Color[]
                        {
                            Color.Red, Color.Blue
                        }
                    };

                    newQuad.velocity += new Vector2(-speed, 0);

                    var newQuad2 = new Quadrilateral(position, new Vector2[]
                    {
                    intersectionPoints[0]-position,
                    _vertices[1],
                    intersectionPoints[1]-position,
                    _vertices[3],
                    _vertices[4],
                    intersectionPoints[1]-position
                    }) {
                        Colors = new Color[]
                        {
                            Color.Green, Color.Yellow
                        }
                    };

                    newQuad2.velocity += new Vector2(speed, 0);

                    result.Clear();
                    result.Add(newQuad);
                    result.Add(newQuad2);
                }
            }

            return result;
        }
    }
}
