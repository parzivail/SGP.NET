using System;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PFX.Shader;
using PFX.Util;
using SGPdotNET;
using Vector3 = OpenTK.Vector3;

namespace Sandbox
{
    class Earth
    {
        private static readonly float EarthRadiusScaled = (float) (SgpConstants.EarthRadiusKm / 100);
        private readonly Sphere _sphere = new Sphere(EarthRadiusScaled, EarthRadiusScaled, 60, 30);
        private readonly Sphere _sphereAtmosphere = new Sphere(EarthRadiusScaled * 1.07f, EarthRadiusScaled * 1.07f, 60, 30);
        //private readonly Sphere _sphereSpace = new Sphere(257, 257, 60, 30);
        private static ShaderProgram _earthShader;
        private static ShaderProgram _earthAtmosShader;
        //private static ShaderProgram _spaceShader;

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
        private static readonly Uniform Scatter = new Uniform("iScatter");

        private static SimpleVertexBuffer _earthVbo;
        private static SimpleVertexBuffer _earthAtmosVbo;
        //private static SimpleVertexBuffer _spaceVbo;

        private int _vertexPositionAttribute;
        private int _vertexNormalAttribute;
        private int _textureCoordAttribute;

        private int _earthSpheremap;
        private int _earthSpheremapNight;
        private int _earthSpheremapSpecular;
        private int _earthSpheremapNormal;
        //private int _spaceSpheremap;

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
            pair = new Bitmap("space.jpg").LoadGlTexture();
            //_spaceSpheremap = pair.Key;

            _earthShader = new FragVertShaderProgram(
                    File.ReadAllText("earth.frag"),
                    File.ReadAllText("earth.vert")
                );
            _earthShader.InitProgram();

            _earthAtmosShader = new FragVertShaderProgram(
                    File.ReadAllText("atmos.frag"),
                    File.ReadAllText("earth.vert")
                );
            _earthAtmosShader.InitProgram();

            //_spaceShader = new FragVertShaderProgram(
            //        File.ReadAllText("space.frag"),
            //        File.ReadAllText("earth.vert")
            //    );
            //_spaceShader.InitProgram();

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

            //_spaceVbo = new SimpleVertexBuffer();
            //_spaceVbo.InitializeVbo(_sphereSpace.MakeBuffers());

            ColorMapSamplerUniform.Value = 0;
            SpecularMapSamplerUniform.Value = 1;
            NightMapSamplerUniform.Value = 2;
            NormalMapSamplerUniform.Value = 3;
            PointLightingSpecularColorUniform.Value = new Vector3(0.9f, 0.9f, 0.9f);
            PointLightingDiffuseColorUniform.Value = new Vector3(0.9f, 0.9f, 0.9f);
            AmbientColorUniform.Value = new Vector3(0.5f, 0.5f, 0.5f);
        }

        public void Draw(Matrix4 projectionMatrix, Matrix4 modelViewMatrix)
        {
            PMatrixUniform.Value = projectionMatrix;
            MvMatrixUniform.Value = modelViewMatrix;

            var normalMatrix = new Matrix3(modelViewMatrix);
            normalMatrix.Invert();
            normalMatrix.Transpose();
            NMatrixUniform.Value = normalMatrix;

            // Percent through a day (1440m/day)
            var t = DateTime.UtcNow.TimeOfDay.TotalMinutes / 1440f * Math.PI * 2;
            const float sunDistance = 20000;

            PointLightingLocationUniform.Value = Vector3.TransformPosition(new Vector3(sunDistance * (float)Math.Cos(t), 8696, sunDistance * (float)Math.Sin(t)), modelViewMatrix);

            var scale = projectionMatrix.ExtractScale();
            InnerRadius.Value = (EarthRadiusScaled * 1.1f * scale).Length;
            OuterRadius.Value = (EarthRadiusScaled * 1.15f * scale).Length;

            Scatter.Value = MainWindow.FastGraphics ? 2 : 8;

            var uniforms = new[]
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
                OuterRadius,
                Scatter
            };

            //GL.ActiveTexture(TextureUnit.Texture0);
            //GL.BindTexture(TextureTarget.Texture2D, _spaceSpheremap);

            //_spaceShader.Use(uniforms);
            //_spaceVbo.BindAttribs(_vertexPositionAttribute, _textureCoordAttribute, _vertexNormalAttribute);
            //_spaceVbo.Render(PrimitiveType.Triangles);

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
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            _earthAtmosVbo.Render(PrimitiveType.Triangles);
            GL.PopAttrib();

            GL.UseProgram(0);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
    }
}
