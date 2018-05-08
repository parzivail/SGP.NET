using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Sandbox
{
    public class Sphere
    {
        private readonly Vertex[] _sphereVertices;
        private readonly ushort[] _sphereElements;

        public Sphere(float radius, float height, byte segments, byte rings)
        {
            _sphereVertices = CalculateVertices2(radius, height, segments, rings);
            _sphereElements = CalculateElements(radius, height, segments, rings);
        }

        public void Draw()
        {
            GL.Begin(PrimitiveType.Triangles);
            foreach (var element in _sphereElements)
            {
                GL.Color4(1.5f, 0.0f, 1.0f, 0.50f);
                var vertex = _sphereVertices[element];
                GL.TexCoord2(vertex.TexCoord);
                GL.Normal3(vertex.Normal);
                GL.Vertex3(vertex.Position);
            }

            GL.End();
        }

        public static Vertex[] CalculateVertices2(float radius, float height, byte segments, byte rings)
        {
            var data = new Vertex[segments * rings];

            var i = 0;

            for (double y = 0; y < rings; y++)
            {
                var phi = y / (rings - 1) * Math.PI; //was /2 
                for (double x = 0; x < segments; x++)
                {
                    var theta = x / (segments - 1) * 2 * Math.PI;

                    var v = new Vector3
                    {
                        X = (float)(radius * Math.Sin(phi) * Math.Cos(theta)),
                        Y = (float)(height * Math.Cos(phi)),
                        Z = (float)(radius * Math.Sin(phi) * Math.Sin(theta))
                    };
                    var n = Vector3.Normalize(v);
                    var uv = new Vector2
                    {
                        X = 1 - (float)(x / (segments - 1)),
                        Y = (float)(y / (rings - 1))
                    };
                    // Using data[i++] causes i to be incremented multiple times in Mono 2.2 (bug #479506).
                    data[i] = new Vertex { Position = v, Normal = n, TexCoord = uv };
                    i++;
                }

            }

            return data;
        }

        public static ushort[] CalculateElements(float radius, float height, byte segments, byte rings)
        {
            var numVertices = segments * rings;
            var data = new ushort[numVertices * 6];

            ushort i = 0;

            for (byte y = 0; y < rings - 1; y++)
            {
                for (byte x = 0; x < segments - 1; x++)
                {
                    data[i++] = (ushort)((y + 0) * segments + x);
                    data[i++] = (ushort)((y + 1) * segments + x);
                    data[i++] = (ushort)((y + 1) * segments + x + 1);

                    data[i++] = (ushort)((y + 1) * segments + x + 1);
                    data[i++] = (ushort)((y + 0) * segments + x + 1);
                    data[i++] = (ushort)((y + 0) * segments + x);
                }
            }

            // Verify that we don't access any vertices out of bounds:
            if (data.Any(index => index >= segments * rings))
                throw new IndexOutOfRangeException();

            return data;
        }


        public struct Vertex
        { // mimic InterleavedArrayFormat.T2fN3fV3f
            public Vector2 TexCoord;
            public Vector3 Normal;
            public Vector3 Position;
        }
    }
}
