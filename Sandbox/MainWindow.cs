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
using DateTime = SGP4_Sharp.DateTime;
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

        private Vector3 _rotation = new Vector3(0, 180, 0);

        private static readonly Earth Earth = new Earth();
        private static readonly SatelliteNetwork Network = new SatelliteNetwork(new CoordGeodetic(30.332184, -81.655647, 0));

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

            GL.Disable(EnableCap.Lighting);
            foreach (var satellite in Network.Satellites)
            {
                var posVec = satellite
                    .Predict()
                    .ToGeodetic()
                    .ToSpherical() / 100;

                GL.Color3(Color.White);
                GL.Begin(PrimitiveType.Points);
                GL.Vertex3(posVec);
                GL.End();

                GL.Color3(Color.Yellow);
                GL.LineWidth(1);
                var footprint = satellite.GetFootprint();
                var center = satellite.Predict().ToGeodetic();
                GL.PushMatrix();
                GL.Rotate((float)(center.longitude / Math.PI * 180) - 90, 0, 1, 0);
                GL.Rotate(90 - (float)(center.latitude / Math.PI * 180), 1, 0, 0);
                GL.Begin(PrimitiveType.LineLoop);
                foreach (var coord in footprint)
                    GL.Vertex3(coord.ToSpherical() / 100f);
                GL.End();
                GL.PopMatrix();
            }
            GL.Enable(EnableCap.Lighting);

            Earth.Draw(projection, modelViewMatrix);

            GL.PopMatrix();

            // Set up 2D mode
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1000, 1000);
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

            foreach (var satellite in Network.Satellites)
            {
                var posVec = satellite
                    .Predict()
                    .ToGeodetic()
                    .ToSpherical() / 100;

                GL.PushMatrix();
                var mat = modelViewMatrix * projection;
                posVec = Vector3.Project(posVec, 0, Height, Width, -Height, -1, 1, mat);
                GL.Translate((int)posVec.X + 3, (int)(posVec.Y - Font.Common.LineHeight / 2f), posVec.Z);
                Font.RenderString(satellite.Name);
                GL.PopMatrix();
            }

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

            Earth.Init();

            var n19 = new Satellite(
                "NOAA 19",
                "1 33591U 09005A   18130.59156788  .00000107  00000-0  83181-4 0  9990",
                "2 33591  99.1393 107.8779 0015028  73.6080 286.6741 14.12276632476607"
            );

            Network.Satellites.Add(n19);

            Network.Satellites.Add(new Satellite(
                "NOAA 18",
                "1 28654U 05018A   18130.56017312 -.00000013  00000-0  18347-4 0  9999",
                "2 28654  99.1486 165.2406 0013553 230.0996 129.8984 14.12374893668377"
            ));

            Network.Satellites.Add(new Satellite(
                "ISS (ZARYA)",
                "1 25544U 98067A   18130.73760359  .00003171  00000-0  54904-4 0  9991",
                "2 25544  51.6417 205.3936 0003324  65.1735  80.0101 15.54246051112695"
            ));

            Network.Satellites.Add(new Satellite(
                "ATLAS CENTAUR 2",
                "1 00694U 63047A   18130.58476307  .00000390  00000-0  38670-4 0  9992",
                "2 00694  30.3560 163.3191 0587017 352.3983   6.8031 14.02314778728702"
            ));
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
