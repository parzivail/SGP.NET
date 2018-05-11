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
        private readonly Sphere _sphere = new Sphere((float)(Global.kXKMPER / 100), (float)(Global.kXKMPER / 100), 60, 30);
        private readonly Sphere _sphereAtmosphere = new Sphere((float)(Global.kXKMPER / 100) * 1.07f, (float)(Global.kXKMPER / 100) * 1.07f, 60, 30);
        private static ShaderProgram _earthShader;
        private static ShaderProgram _earthAtmosShader;

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
        private static readonly Uniform InnerRadius = new Uniform("fInnerRadius");
        private static readonly Uniform OuterRadius = new Uniform("fOuterRadius");
        
        private static SimpleVertexBuffer _earthVbo;
        private static SimpleVertexBuffer _earthAtmosVbo;

        private int _vertexPositionAttribute;
        private int _vertexNormalAttribute;
        private int _textureCoordAttribute;

        private int _earthSpheremap;
        private int _earthSpheremapNight;
        private int _earthSpheremapSpecular;
        private int _earthSpheremapNormal;

        public void Init()
        {
            var pair = new Bitmap("earth_day.png").LoadGlTexture();
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

            _earthAtmosShader = new FragVertShaderProgram(
                    File.ReadAllText("earthAtmos.frag"),
                    File.ReadAllText("earth.vert")
                );
            _earthAtmosShader.InitProgram();

            GL.UseProgram(_earthShader.GetId());

            GL.BindAttribLocation(_earthShader.GetId(), _vertexPositionAttribute = 0, "aVertexPosition");
            GL.BindAttribLocation(_earthShader.GetId(), _vertexNormalAttribute = 1, "aVertexNormal");
            GL.BindAttribLocation(_earthShader.GetId(), _textureCoordAttribute = 2, "aTextureCoord");

            GL.EnableVertexAttribArray(_vertexPositionAttribute);
            GL.EnableVertexAttribArray(_vertexNormalAttribute);
            GL.EnableVertexAttribArray(_textureCoordAttribute);

            GL.UseProgram(0);
            
            _earthVbo = new SimpleVertexBuffer();
            _earthVbo.InitializeVbo(_sphere.MakeBuffers());

            _earthAtmosVbo = new SimpleVertexBuffer();
            _earthAtmosVbo.InitializeVbo(_sphereAtmosphere.MakeBuffers());
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
            PointLightingLocationUniform.Value = Vector3.TransformPosition(new Vector3(d * (float)Math.Cos(t), 8696, d * (float)Math.Sin(t)), modelViewMatrix);
            PointLightingSpecularColorUniform.Value = new Vector3(0.9f, 0.9f, 0.9f);
            PointLightingDiffuseColorUniform.Value = new Vector3(0.9f, 0.9f, 0.9f);

            InnerRadius.Value = (float) (Global.kXKMPER / 100) * 3.2f;
            OuterRadius.Value = (float) (Global.kXKMPER / 100) * 1.07f * 3.2f;

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
                PointLightingDiffuseColorUniform,
                InnerRadius,
                OuterRadius
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
            _earthVbo.BindAttribs(_vertexPositionAttribute, _textureCoordAttribute, _vertexNormalAttribute);
            _earthVbo.Render(PrimitiveType.Triangles);

            _earthAtmosShader.Use(uniforms);
            _earthAtmosVbo.BindAttribs(_vertexPositionAttribute, _textureCoordAttribute, _vertexNormalAttribute);

            GL.PushAttrib(AttribMask.EnableBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
            _earthAtmosVbo.Render(PrimitiveType.Triangles);
            GL.PopAttrib();

            //_sphere.Draw();
            GL.UseProgram(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
