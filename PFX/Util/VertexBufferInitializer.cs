using System;
using System.Collections.Generic;
using OpenTK;

namespace PFX.Util
{
    public class VertexBufferInitializer
    {
        public List<Vector3> Vertices { get; }
        public List<Vector3> Normals { get; }
        public List<Vector2> TexCoords { get; }
        public List<int> Indices { get; }

        public VertexBufferInitializer(List<Vector3> vertices, List<Vector3> normals, List<Vector2> texCoords,
            List<int> indices)
        {
            Vertices = vertices;
            Normals = normals;
            TexCoords = texCoords;
            Indices = indices;
        }

        public VertexBufferInitializer()
        {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            TexCoords = new List<Vector2>();
            Indices = new List<int>();
        }

        public void AddVertex(Vector3 pos)
        {
            AddVertex(pos, Vector3.Zero);
        }

        public void AddVertex(Vector3 pos, Vector3 normal)
        {
            AddVertex(pos, normal, Vector2.Zero);
        }

        public void AddVertex(Vector3 pos, Vector3 normal, Vector2 texCoord)
        {
            Vertices.Add(pos);
            Normals.Add(normal);
            TexCoords.Add(texCoord);
            Indices.Add(Indices.Count);
        }
    }
}