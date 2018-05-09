using System.Collections.Generic;

namespace Sandbox
{
    public class GlslBufferInitializer
    {
        public List<float> Positions { get; set; }
        public List<float> Normals { get; set; }
        public List<float> Uvs { get; set; }
        public ushort[] SphereElements { get; }

        public GlslBufferInitializer(List<float> positions, List<float> normals, List<float> uvs, ushort[] sphereElements)
        {
            Positions = positions;
            Normals = normals;
            Uvs = uvs;
            SphereElements = sphereElements;
        }
    }
}