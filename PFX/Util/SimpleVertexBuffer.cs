using System;
using System.Collections.Generic;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PFX.Util
{
    public class SimpleVertexBuffer
    {
        private List<Vector2> _texcoords = new List<Vector2>();
        private List<int> _indices = new List<int>();
        private List<Vector3> _normals = new List<Vector3>();
        private List<Vector3> _vertices = new List<Vector3>();

        public int UvBufferId;
        public int ElementBufferId;
        public int NormalBufferId;
        public int TangentBufferId;
        public int BinormalBufferId;
        public int VertexBufferId;

        public int NumElements;

        public void AddVertex(Vector3 pos)
        {
            AddVertex(pos, Vector3.Zero);
        }

        public void AddVertex(Vector3 pos, Vector3 normal)
        {
            AddVertex(pos, normal, Vector2.Zero);
        }

        public void AddVertex(Vector3 pos, Vector3 normal, Vector2 texcoord)
        {
            _vertices.Add(pos);
            _normals.Add(normal);
            _texcoords.Add(texcoord);
            _indices.Add(_indices.Count);
        }

        public void InitializeVbo()
        {
            InitializeVbo(_vertices.ToArray(), _normals.ToArray(), _texcoords.ToArray(), _indices.ToArray());
            _vertices.Clear();
            _normals.Clear();
            _indices.Clear();
            _texcoords.Clear();
        }

        public void InitializeVbo(VertexBufferInitializer vbi)
        {
            _vertices = vbi.Vertices;
            _normals = vbi.Normals;
            _indices = vbi.Indices;
            _texcoords = vbi.TexCoords;
            InitializeVbo();
        }

        public void InitializeVbo(Vector3[] vertices, Vector3[] vertexNormals, Vector2[] vertexUVs, int[] indices)
        {
            if (vertices == null) return;
            if (indices == null) return;

            Vector3[] tangents = null;
            Vector3[] binormals = null;

            if (vertexNormals != null)
            {
                tangents = new Vector3[vertices.Length];
                binormals = new Vector3[vertices.Length];

                for (var a = 0; a < indices.Length / 6 - 2; a++)
                {
                    var i1 = indices[a * 6 + 0];
                    var i2 = indices[a * 6 + 1];
                    var i3 = indices[a * 6 + 2];

                    var v1 = vertices[i1];
                    var v2 = vertices[i2];
                    var v3 = vertices[i3];

                    var w1 = vertexUVs[i1];
                    var w2 = vertexUVs[i2];
                    var w3 = vertexUVs[i3];

                    var x1 = v2.X - v1.X;
                    var x2 = v3.X - v1.X;
                    var y1 = v2.Y - v1.Y;
                    var y2 = v3.Y - v1.Y;
                    var z1 = v2.Z - v1.Z;
                    var z2 = v3.Z - v1.Z;

                    var s1 = w2.X - w1.X;
                    var s2 = w3.X - w1.X;
                    var t1 = w2.Y - w1.Y;
                    var t2 = w3.Y - w1.Y;

                    float coef = 1 / (s1 * t1 - s2 * t2);
                    var tangent = new Vector3(coef * ((x1 * t2) + (x2 * -t1)), coef * ((y1 * t2) + (y2 * -t1)), coef * ((z1 * t2) + (z2 * -t1)));

                    tangents[a + 0] = tangent;
                    tangents[a + 1] = tangent;
                    tangents[a + 2] = tangent;

                    var normal = Vector3.Cross(new Vector3(x1, y1, z1), new Vector3(x2, y2, z2));
                    var binormal = Vector3.Cross(normal, tangent);

                    binormals[a + 0] = binormal;
                    tangents[a + 1] = binormal;
                    tangents[a + 2] = binormal;
                }
            }

            try
            {
                // UV Array Buffer
                if (vertexUVs != null)
                {
                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out UvBufferId);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, UvBufferId);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexUVs.Length * Vector2.SizeInBytes), vertexUVs,
                        BufferUsageHint.StaticDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out int bufferSize);
                    if (vertexUVs.Length * Vector2.SizeInBytes != bufferSize)
                        throw new ApplicationException("Vertex UV array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                // Normal Array Buffer
                if (vertexNormals != null)
                {
                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out NormalBufferId);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexNormals.Length * Vector3.SizeInBytes),
                        vertexNormals, BufferUsageHint.StaticDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out int bufferSize);
                    if (vertexNormals.Length * Vector3.SizeInBytes != bufferSize)
                        throw new ApplicationException("Normal array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                    /*
                     * Tangents
                     */

                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out TangentBufferId);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, TangentBufferId);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexNormals.Length * Vector3.SizeInBytes),
                        tangents, BufferUsageHint.StaticDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                    if (vertexNormals.Length * Vector3.SizeInBytes != bufferSize)
                        throw new ApplicationException("Tangent array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                    /*
                     * Binormals
                     */

                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out BinormalBufferId);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, BinormalBufferId);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexNormals.Length * Vector3.SizeInBytes),
                        tangents, BufferUsageHint.StaticDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                    if (vertexNormals.Length * Vector3.SizeInBytes != bufferSize)
                        throw new ApplicationException("Tangent array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                // Vertex Array Buffer
                {
                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out VertexBufferId);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferId);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Vector3.SizeInBytes), vertices,
                        BufferUsageHint.DynamicDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out int bufferSize);
                    if (vertices.Length * Vector3.SizeInBytes != bufferSize)
                        throw new ApplicationException("Vertex array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                // Element Array Buffer
                {
                    // Generate Array Buffer Id
                    GL.GenBuffers(1, out ElementBufferId);

                    // Bind current context to Array Buffer ID
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferId);

                    // Send data to buffer
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices,
                        BufferUsageHint.StreamDraw);

                    // Validate that the buffer is the correct size
                    GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize,
                        out int bufferSize);
                    if (indices.Length * sizeof(int) != bufferSize)
                        throw new ApplicationException("Element array not uploaded correctly");

                    // Clear the buffer Binding
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                }
            }
            catch (ApplicationException ex)
            {
                Lumberjack.Error($"{ex.Message}. Try re-rendering.");
            }

            // Store the number of elements for the DrawElements call
            NumElements = indices.Length;
        }

        public void Render(PrimitiveType type = PrimitiveType.Quads)
        {
            // Push current Array Buffer state so we can restore it later
            GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);

            if (VertexBufferId == 0) return;
            if (ElementBufferId == 0) return;

            // Normal Array Buffer
            if (NormalBufferId != 0)
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);

                // Set the Pointer to the current bound array describing how the data ia stored
                GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(ArrayCap.NormalArray);
            }

            // UV Array Buffer
            if (UvBufferId != 0)
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, UvBufferId);

                // Set the Pointer to the current bound array describing how the data ia stored
                GL.ColorPointer(4, ColorPointerType.Float, Vector2.SizeInBytes, IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(ArrayCap.TextureCoordArray);
            }

            // Vertex Array Buffer
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferId);

                // Set the Pointer to the current bound array describing how the data ia stored
                GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(ArrayCap.VertexArray);
            }

            // Element Array Buffer
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferId);

                // Draw the elements in the element array buffer
                // Draws up items in the Color, Vertex, TexCoordinate, and Normal Buffers using indices in the ElementArrayBuffer
                GL.DrawElements(type, NumElements, DrawElementsType.UnsignedInt, IntPtr.Zero);

                // Could also call GL.DrawArrays which would ignore the ElementArrayBuffer and just use primitives
                // Of course we would have to reorder our data to be in the correct primitive order
            }

            // Restore the state
            GL.PopClientAttrib();
        }

        public void BindAttribs(int vertexBufferAttribName = -1, int uvBufferAttribName = -1, int normalBufferAttribName = -1, int tangentBufferAttribName = -1, int binormalBufferAttribName = -1)
        {
            if (vertexBufferAttribName != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferId);
                GL.VertexAttribPointer(vertexBufferAttribName, 3, VertexAttribPointerType.Float,
                    false, 0, 0);
            }

            if (uvBufferAttribName != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, UvBufferId);
                GL.VertexAttribPointer(uvBufferAttribName, 2, VertexAttribPointerType.Float,
                    false, 0, 0);
            }

            if (normalBufferAttribName != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);
                GL.VertexAttribPointer(normalBufferAttribName, 3, VertexAttribPointerType.Float,
                    false, 0, 0);
            }

            if (tangentBufferAttribName != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);
                GL.VertexAttribPointer(tangentBufferAttribName, 3, VertexAttribPointerType.Float,
                    false, 0, 0);
            }

            if (binormalBufferAttribName != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);
                GL.VertexAttribPointer(binormalBufferAttribName, 3, VertexAttribPointerType.Float,
                    false, 0, 0);
            }
        }
    }
}