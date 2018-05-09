using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using PFX;
using PFX.BmFont;
using PFX.Shader;
using PFX.Util;
using SGP4_Sharp;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Sandbox
{
    class MainWindow : GameWindow
    {
        /// <summary>
        /// Onscreen font
        /// </summary>
        public BitmapFont Font { get; set; }
        /// <summary>
        /// Keyboard State
        /// </summary>
        public KeyboardState KeyboardState { get; private set; }

        private Sphere _sphere = new Sphere((float)(Global.kXKMPER / 100), (float)(Global.kXKMPER / 100), 40, 20);

        private Vector3 _rotation = Vector3.Zero;

        private static Tle _tle;
        private static SGP4 _sgp4;
        private static CoordGeodetic _geo = new CoordGeodetic(0, 0, 0);
        private static List<CoordGeodetic> _geoPredictions = new List<CoordGeodetic>();

        private static ShaderProgram _earthShader;

        /*
         shaderProgram.vertexPositionAttribute = gl.getAttribLocation(shaderProgram, "aVertexPosition");
        gl.enableVertexAttribArray(shaderProgram.vertexPositionAttribute);
        shaderProgram.vertexNormalAttribute = gl.getAttribLocation(shaderProgram, "aVertexNormal");
        gl.enableVertexAttribArray(shaderProgram.vertexNormalAttribute);
        shaderProgram.textureCoordAttribute = gl.getAttribLocation(shaderProgram, "aTextureCoord");
        gl.enableVertexAttribArray(shaderProgram.textureCoordAttribute);
        */

        private static Uniform pMatrixUniform = new Uniform("uPMatrix");
        private static Uniform mvMatrixUniform = new Uniform("uMVMatrix");
        private static Uniform nMatrixUniform = new Uniform("uNMatrix");
        private static Uniform colorMapSamplerUniform = new Uniform("uColorMapSampler");
        private static Uniform specularMapSamplerUniform = new Uniform("uSpecularMapSampler");
        private static Uniform useColorMapUniform = new Uniform("uUseColorMap");
        private static Uniform useSpecularMapUniform = new Uniform("uUseSpecularMap");
        private static Uniform useLightingUniform = new Uniform("uUseLighting");
        private static Uniform ambientColorUniform = new Uniform("uAmbientColor");
        private static Uniform pointLightingLocationUniform = new Uniform("uPointLightingLocation");
        private static Uniform pointLightingSpecularColorUniform = new Uniform("uPointLightingSpecularColor");
        private static Uniform pointLightingDiffuseColorUniform = new Uniform("uPointLightingDiffuseColor");
        private static GlslBufferInitializer _earthBuffer;
        private int _vertexPositionAttribute;
        private int _vertexNormalAttribute;
        private int _textureCoordAttribute;

        private int _earthSpheremap;
        private int _earthSpheremapSpecular;
        private int _vNormBuf;
        private int _vTexBuf;
        private int _vPosBuffer;
        private int _vIdxBuf;

        public MainWindow() : base(960, 540)
        {
            Resize += MainWindow_Resize;
            Load += MainWindow_Load;
            MouseWheel += OnMouseWheel;
            RenderFrame += MainWindow_RenderFrame;
            UpdateFrame += MainWindow_UpdateFrame;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs args)
        {
        }

        private void MainWindow_UpdateFrame(object sender, FrameEventArgs e)
        {
            KeyboardState = Keyboard.GetState();

            var t = (float)(50 * e.Time);

            if (KeyboardState[Key.Left])
                _rotation.Y -= t;
            if (KeyboardState[Key.Right])
                _rotation.Y += t;
            if (KeyboardState[Key.Up])
                _rotation.X -= t;
            if (KeyboardState[Key.Down])
                _rotation.X += t;

            _geoPredictions.Clear();

            var now = SGP4_Sharp.DateTime.Now();
            var eci = _sgp4.FindPosition(now);
            _geo = eci.ToGeodetic();
            _geoPredictions.Add(eci.ToGeodetic());

            for (var i = 0; i < 60; i++)
            {
                now = now.AddMinutes(1);
                eci = _sgp4.FindPosition(now);
                _geoPredictions.Add(eci.ToGeodetic());
            }
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            // Set up the viewport
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1000, 1000);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void MainWindow_RenderFrame(object sender, FrameEventArgs e)
        {
            // Reset the view
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            // Reload the projection matrix
            var aspectRatio = Width / (float)Height;
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 1024);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            var lookat = Matrix4.LookAt(0, 0, 256, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            GL.Color3(Color.White);

            GL.Rotate(_rotation.X, 1.0f, 0.0f, 0.0f);
            GL.Rotate(_rotation.Y, 0.0f, 1.0f, 0.0f);

            // Capture last matrix
            GL.GetFloat(GetPName.ModelviewMatrix, out Matrix4 modelViewMatrix);

            GL.PushMatrix();

            pMatrixUniform.Value = projection;
            mvMatrixUniform.Value = modelViewMatrix;

            var normalMatrix = new Matrix3(modelViewMatrix);
            normalMatrix.Invert();
            normalMatrix.Transpose();
            nMatrixUniform.Value = normalMatrix;

            colorMapSamplerUniform.Value = 0;
            specularMapSamplerUniform.Value = 1;

            useColorMapUniform.Value = true;
            useSpecularMapUniform.Value = true;
            useLightingUniform.Value = true;

            ambientColorUniform.Value = new Vector3(0.4f, 0.4f, 0.4f);
            pointLightingLocationUniform.Value = new Vector3(-4, 10, -20);
            pointLightingSpecularColorUniform.Value = new Vector3(5, 5, 5);
            pointLightingDiffuseColorUniform.Value = new Vector3(0.8f, 0.8f, 0.8f);

            var uniforms = new List<Uniform>
            {
                pMatrixUniform,
                mvMatrixUniform,
                nMatrixUniform,
                colorMapSamplerUniform,
                specularMapSamplerUniform,
                useColorMapUniform,
                useSpecularMapUniform,
                useLightingUniform,
                ambientColorUniform,
                pointLightingLocationUniform,
                pointLightingSpecularColorUniform,
                pointLightingDiffuseColorUniform
            };

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _earthSpheremap);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _earthSpheremapSpecular);

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
            GL.DrawElements(BeginMode.Triangles, 1, DrawElementsType.UnsignedShort, 0);

            //_sphere.Draw();
            GL.UseProgram(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Disable(EnableCap.Texture2D);


            GL.Disable(EnableCap.Lighting);
            var posVec = _geo.ToSpherical();
            posVec *= 1 / 100f;

            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(posVec);
            GL.End();

            GL.Color3(Color.Yellow);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineStrip);
            foreach (var prediction in _geoPredictions)
                GL.Vertex3(prediction.ToSpherical() / 100);
            GL.End();

            GL.PopMatrix();

            // Set up 2D mode
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, 1000, -1000);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.PushMatrix();

            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.Lighting);

            GL.Color3(Color.White);

            // Render diagnostic data
            GL.Enable(EnableCap.Texture2D);
            // Info footer
            GL.PushMatrix();
            Font.RenderString($"Development Build");

            GL.StencilFunc(StencilFunction.Always, 1, 0x00);
            var mat = modelViewMatrix * projection;
            var proj = WorldToScreen(posVec, ref mat, ClientRectangle);
            GL.Translate((int)proj.X + 3, (int)(proj.Y - Font.Common.LineHeight / 2f), 0);
            Font.RenderString($"NOAA 19");

            GL.PopMatrix();
            GL.Disable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Lighting);
            GL.Disable(EnableCap.Blend);

            GL.PopMatrix();

            var ec = GL.GetError();
            if (ec != ErrorCode.NoError)
                Console.WriteLine(ec);

            // Swap the graphics buffer
            SwapBuffers();
        }

        public static Vector2 WorldToScreen(Vector3 worldPos, ref Matrix4 viewProjMat, Rectangle clientRect)
        {
            var pos = Vector4.Transform(new Vector4(worldPos, 1f), viewProjMat);
            pos /= pos.W;
            pos.Y = -pos.Y;
            var screenSize = new Vector2(clientRect.Width, clientRect.Height);
            var screenCenter = new Vector2(clientRect.X, clientRect.Y) + screenSize / 2f;
            return screenCenter + pos.Xy * screenSize / 2f;
        }

        private Vector3 F(double u, double v, double r)
        {
            return new Vector3((float)(Math.Cos(u) * Math.Sin(v) * r), (float)(Math.Cos(v) * r), (float)(Math.Sin(u) * Math.Cos(v) * r));
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Setup OpenGL data
            GL.ClearColor(Color.FromArgb(13, 13, 13));
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.LineSmooth);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.PointSize(4);
            GL.LineWidth(2);
            Lumberjack.Log("Loaded OpenGL settings");

            const float diffuse = 1.5f;
            float[] matDiffuse = { diffuse, diffuse, diffuse };
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, matDiffuse);
            GL.Light(LightName.Light0, LightParameter.Position, new[] { 0.0f, 0.0f, 0.0f, 256.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new[] { diffuse, diffuse, diffuse, diffuse });

            // Set up lighting
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.ShadeModel(ShadingModel.Smooth);

            // Set up caps
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.RescaleNormal);

            // Load the font
            Font = BitmapFont.LoadBinaryFont("dina", FontBank.FontDina, FontBank.BmDina);

            var pair = new Bitmap("earth.jpg").LoadGlTexture();
            _earthSpheremap = pair.Key;
            pair = new Bitmap("earth-specular.gif").LoadGlTexture();
            _earthSpheremapSpecular = pair.Key;

            // Load the map
            //if (!File.Exists("map.png"))
            //    Lumberjack.Kill("Unable to locate 'map.png'", Util.ErrorCode.FnfMap);
            //var pair = new Bitmap("map.png").LoadGlTexture();

            _tle = new Tle(
                "1 33591U 09005A   18126.90753522  .00000083  00000-0  70028-4 0  9998",
                "2 33591  99.1390 104.1221 0015038  83.2147 277.0734 14.12275499476086"
                );
            _sgp4 = new SGP4(_tle);

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
            GL.BufferData(BufferTarget.ArrayBuffer, _earthBuffer.Normals.Count, _earthBuffer.Normals.ToArray(), BufferUsageHint.StaticDraw);

            _vTexBuf = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vTexBuf);
            GL.BufferData(BufferTarget.ArrayBuffer, _earthBuffer.Uvs.Count, _earthBuffer.Uvs.ToArray(), BufferUsageHint.StaticDraw);

            _vPosBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vPosBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _earthBuffer.Positions.Count, _earthBuffer.Positions.ToArray(), BufferUsageHint.StaticDraw);

            _vIdxBuf = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vIdxBuf);
            GL.BufferData(BufferTarget.ArrayBuffer, _earthBuffer.SphereElements.Length, _earthBuffer.SphereElements, BufferUsageHint.StaticDraw);

            GL.UseProgram(0);
        }

        /// <summary>
        /// Cartesian distance formula
        /// </summary>
        /// <param name="x1">The X of point 1</param>
        /// <param name="y1">The Y of point 1</param>
        /// <param name="x2">The X of point 2</param>
        /// <param name="y2">The Y of point 2</param>
        /// <returns></returns>
        public double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        /// <summary>
        /// Saves the current screen frame to a PNG
        /// </summary>
        /// <param name="filename">The PNG filename to save</param>
        public void SaveScreen(string filename)
        {
            using (var bmp = new Bitmap(ClientRectangle.Width, ClientRectangle.Height))
            {
                var data = bmp.LockBits(ClientRectangle, ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                GL.ReadPixels(0, 0, ClientRectangle.Width, ClientRectangle.Height, PixelFormat.Bgr,
                    PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                bmp.Save(filename, ImageFormat.Png);
            }
        }
    }
}
