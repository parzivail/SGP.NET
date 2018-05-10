using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PFX.Shader;
using PFX.Util;
using SGP4_Sharp;

namespace Sandbox
{
    class Earth
    {
        private readonly Sphere _sphere = new Sphere((float)(Global.kXKMPER / 100), (float)(Global.kXKMPER / 100), 60, 20);
        private static ShaderProgram _earthShader;

        private static readonly Uniform PMatrixUniform = new Uniform("uPMatrix");
        private static readonly Uniform MvMatrixUniform = new Uniform("uMVMatrix");
        private static readonly Uniform NMatrixUniform = new Uniform("uNMatrix");
        private static readonly Uniform ColorMapSamplerUniform = new Uniform("uColorMapSampler");
        private static readonly Uniform SpecularMapSamplerUniform = new Uniform("uSpecularMapSampler");
        private static readonly Uniform NightMapSamplerUniform = new Uniform("uNightMapSampler");
        private static readonly Uniform NormalMapSamplerUniform = new Uniform("uNormalMapSampler");
        private static readonly Uniform AmbientColorUniform = new Uniform("uAmbientColor");
        private static readonly Uniform PointLightingLocationUniform = new Uniform("uPointLightingLocation");
        private static readonly Uniform PointLightingSpecularColorUniform = new Uniform("uPointLightingSpecularColor");
        private static readonly Uniform PointLightingDiffuseColorUniform = new Uniform("uPointLightingDiffuseColor");

        private static GlslBufferInitializer _earthBuffer;

        private int _vNormBuf;
        private int _vTexBuf;
        private int _vPosBuffer;
        private int _vIdxBuf;

        private int _vertexPositionAttribute;
        private int _vertexNormalAttribute;
        private int _textureCoordAttribute;

        private int _earthSpheremap;
        private int _earthSpheremapNight;
        private int _earthSpheremapSpecular;
        private int _earthSpheremapNormal;

        public void Init()
        {
            var pair = new Bitmap("earth_day.jpg").LoadGlTexture();
            _earthSpheremap = pair.Key;
            pair = new Bitmap("earth_night.jpg").LoadGlTexture();
            _earthSpheremapNight = pair.Key;
            pair = new Bitmap("earth_specmap.png").LoadGlTexture();
            _earthSpheremapSpecular = pair.Key;
            pair = new Bitmap("earth_normalmap.png").LoadGlTexture();
            _earthSpheremapNormal = pair.Key;

            _earthShader = new FragVertShaderProgram(
                    File.ReadAllText("earth.frag"),
                    File.ReadAllText("earth.vert")
                );
            _earthShader.InitProgram();

            GL.UseProgram(_earthShader.GetId());
            _vertexPositionAttribute = GL.GetAttribLocation(_earthShader.GetId(), "aVertexPosition");
            GL.EnableVertexAttribArray(_vertexPositionAttribute);
            _vertexNormalAttribute = GL.GetAttribLocation(_earthShader.GetId(), "aVertexNormal");
            GL.EnableVertexAttribArray(_vertexNormalAttribute);
            _textureCoordAttribute = GL.GetAttribLocation(_earthShader.GetId(), "aTextureCoord");
            GL.EnableVertexAttribArray(_textureCoordAttribute);

            _earthBuffer = _sphere.MakeBuffers();

            _vNormBuf = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vNormBuf);
            GL.BufferData(BufferTarget.ArrayBuffer, _earthBuffer.Normals.Length * 3 * sizeof(float), _earthBuffer.Normals, BufferUsageHint.StaticDraw);

            _vTexBuf = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vTexBuf);
            GL.BufferData(BufferTarget.ArrayBuffer, _earthBuffer.Uvs.Length * 2 * sizeof(float), _earthBuffer.Uvs, BufferUsageHint.StaticDraw);

            _vPosBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vPosBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _earthBuffer.Positions.Length * 3 * sizeof(float), _earthBuffer.Positions, BufferUsageHint.StaticDraw);

            _vIdxBuf = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vIdxBuf);
            GL.BufferData(BufferTarget.ArrayBuffer, _earthBuffer.SphereElements.Length * sizeof(ushort), _earthBuffer.SphereElements, BufferUsageHint.StreamDraw);

            GL.UseProgram(0);
        }

        public void Draw(Matrix4 projectionMatrix, Matrix4 modelViewMatrix)
        {
            PMatrixUniform.Value = projectionMatrix;
            MvMatrixUniform.Value = modelViewMatrix;

            var normalMatrix = new Matrix3(modelViewMatrix);
            normalMatrix.Invert();
            normalMatrix.Transpose();
            NMatrixUniform.Value = normalMatrix;

            ColorMapSamplerUniform.Value = 0;
            SpecularMapSamplerUniform.Value = 1;
            NightMapSamplerUniform.Value = 2;
            NormalMapSamplerUniform.Value = 3;

            // Percent through a day (1440m/day)
            var t = System.DateTime.UtcNow.TimeOfDay.TotalMinutes / 1440f * Math.PI * 2;
            const float d = 20000;

            AmbientColorUniform.Value = new Vector3(0.5f, 0.5f, 0.5f);
            PointLightingLocationUniform.Value = Vector3.TransformPosition(new Vector3(d * (float)Math.Cos(t), 0, d * (float)Math.Sin(t)), modelViewMatrix);
            PointLightingSpecularColorUniform.Value = new Vector3(0.9f, 0.9f, 0.9f);
            PointLightingDiffuseColorUniform.Value = new Vector3(0.9f, 0.9f, 0.9f);

            var uniforms = new List<Uniform>
            {
                PMatrixUniform,
                MvMatrixUniform,
                NMatrixUniform,
                ColorMapSamplerUniform,
                SpecularMapSamplerUniform,
                NightMapSamplerUniform,
                NormalMapSamplerUniform,
                AmbientColorUniform,
                PointLightingLocationUniform,
                PointLightingSpecularColorUniform,
                PointLightingDiffuseColorUniform
            };

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _earthSpheremap);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _earthSpheremapSpecular);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, _earthSpheremapNight);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, _earthSpheremapNormal);

            _earthShader.Use(uniforms);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vPosBuffer);
            GL.VertexAttribPointer(_vertexPositionAttribute, 3, VertexAttribPointerType.Float,
                false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vTexBuf);
            GL.VertexAttribPointer(_textureCoordAttribute, 2, VertexAttribPointerType.Float,
                false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vNormBuf);
            GL.VertexAttribPointer(_vertexNormalAttribute, 3, VertexAttribPointerType.Float,
                false, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vIdxBuf);
            GL.DrawElements(BeginMode.Triangles, _earthBuffer.SphereElements.Length, DrawElementsType.UnsignedShort, 0);

            //_sphere.Draw();
            GL.UseProgram(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
