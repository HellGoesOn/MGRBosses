using MGRBosses.Common.Collision;
using MGRBosses.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

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

            HandleParallelLineCuts(result, intersectionPoints, intersectedParallelHorizontal, intersectedParallelVertical, cuttingLineStart, cuttingLineEnd);
            //HandlePerpendicularCuts(result, intersectionPoints, intersectedSides);

            return result;
        }

        
        private void HandlePerpendicularCuts(List<Quadrilateral> result, List<Vector2> intersectionPoints, bool[] intersectedSides)
        {
            bool intersectedTopAndLeft = intersectedSides[0] && intersectedSides[2];
            bool intersectedTopAndRight = intersectedSides[0] && intersectedSides[3];
            bool intersectedBottoAndLeft = intersectedSides[1] && intersectedSides[2];
            bool intersectedBottoAndRight = intersectedSides[1] && intersectedSides[3];

            if(intersectedSides.Any(x => x)) {

                if(intersectedTopAndLeft) {
                    float speed = 0.05f;

                    var newQuad = new Quadrilateral(position, new Vector2[]
                    {
                    _vertices[0],
                    intersectionPoints[1]-position,
                    intersectionPoints[0]-position,
                    _vertices[0],
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
                    _vertices[1],
                    intersectionPoints[1]-position,
                    _vertices[1],
                    new Vector2(_vertices[4].X, _vertices[4].Y / 2),
                    intersectionPoints[1]-position
                    }) {
                        Colors = new Color[]
                        {
                            Color.Green, Color.Yellow
                        }
                    };

                    newQuad2.velocity += new Vector2(0, speed);

                    var newQuad3 = new Quadrilateral(position, new Vector2[]
                    {
                    intersectionPoints[1]-position,
                    new Vector2(_vertices[1].X, _vertices[1].Y + _vertices[5].Y / 2),
                    _vertices[2],
                    new Vector2(_vertices[1].X, _vertices[1].Y + _vertices[5].Y / 2),
                    _vertices[4],
                    _vertices[5]
                    }) {
                        Colors = new Color[]
                        {
                            Color.Green, Color.Yellow
                        }
                    };

                    newQuad3.velocity += new Vector2(0, speed);

                    result.Clear();
                    result.Add(newQuad);
                    result.Add(newQuad2);
                    result.Add(newQuad3);
                }
            }
        }

        private void HandleParallelLineCuts(List<Quadrilateral> result, List<Vector2> intersectionPoints, bool intersectedParallelHorizontal, bool intersectedParallelVertical, Vector2 cutStart, Vector2 cutEnd)
        {
            var vel = (cutEnd - cutStart).SafeNormalize(-Vector2.UnitY);

            if (intersectedParallelHorizontal || intersectedParallelVertical) {
                float speed = 0.05f;

                for(int i = 0; i < 20; i++) {
                    var startPos = Vector2.Lerp(cutStart, cutEnd, (float)(i / 20.0f));
                    FallOffEffect.Add(startPos - vel * 80, (startPos + (vel * Main.rand.Next(40, 100)).RotatedByRandom(0.35f)));
                }

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

                    newQuad.velocity += vel * speed;

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

                    newQuad2.velocity += vel * -speed;

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

                    newQuad.velocity += vel * -speed;

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

                    newQuad2.velocity += vel * speed;

                    result.Clear();
                    result.Add(newQuad);
                    result.Add(newQuad2);
                }
            }
        }
    }
}
