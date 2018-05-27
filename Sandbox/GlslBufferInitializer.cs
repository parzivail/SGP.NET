using System.Collections.Generic;

namespace Sandbox
{
    public class GlslBufferInitializer
    {
        public GlslBufferInitializer(List<float> positions, List<float> normals, List<float> uvs,
            ushort[] sphereElements)
        {
            Positions = positions.ToArray();
            Normals = normals.ToArray();
            Uvs = uvs.ToArray();
            SphereElements = sphereElements;
        }

        public float[] Positions { get; set; }
        public float[] Normals { get; set; }
        public float[] Uvs { get; set; }
        public ushort[] SphereElements { get; }
    }
}