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
using DateTime = System.DateTime;
using KeyPressEventArgs = OpenTK.KeyPressEventArgs;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using TimeSpan = System.TimeSpan;

namespace Sandbox
{
    class MainWindow : GameWindow
    {
        public static bool FastGraphics;

        /// <summary>
        /// Onscreen font
        /// </summary>
        public BitmapFont Font { get; set; }
        /// <summary>
        /// Keyboard State
        /// </summary>
        public KeyboardState KeyboardState { get; private set; }

        private readonly Profiler _profiler = new Profiler();
        private Dictionary<string, TimeSpan> _profile = new Dictionary<string, TimeSpan>();
        private Sparkline _fpsSparkline;
        private Sparkline _renderTimeSparkline;

        private static readonly BackgroundWorker ObserverBackgroundWorker = new BackgroundWorker();

        private Vector3 _rotation = new Vector3(0, 180, 0);

        private static readonly Earth Earth = new Earth();
        private static readonly SatelliteNetwork Network = new SatelliteNetwork(new CoordGeodetic(30.332184, -81.655647, 0));

        private List<SatelliteObservation> _todaysObservations = new List<SatelliteObservation>();
        private DateTime _observationsDirtyTime = DateTime.Now;


        public MainWindow() : base(960, 540)
        {
            Resize += MainWindow_Resize;
            Load += MainWindow_Load;
            MouseWheel += OnMouseWheel;
            RenderFrame += MainWindow_RenderFrame;
            UpdateFrame += MainWindow_UpdateFrame;

            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;

            KeyPress += MainWindow_KeyPress;

            ObserverBackgroundWorker.DoWork += PredictFutureObservations;
            ObserverBackgroundWorker.RunWorkerCompleted += CollectPredictedObservations;

            Lumberjack.TraceLevel = OutputLevel.Debug;
        }

        private void MainWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'q')
                FastGraphics = !FastGraphics;
        }
        
        private void MainWindow_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            KeyboardState = Keyboard.GetState();
        }

        private void MainWindow_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            KeyboardState = Keyboard.GetState();
        }

        private void CollectPredictedObservations(object sender, RunWorkerCompletedEventArgs args)
        {
            _todaysObservations = (List<SatelliteObservation>)args.Result;
        }

        private static void PredictFutureObservations(object sender, DoWorkEventArgs args)
        {
            var observations = Network
                .Satellites
                .SelectMany(satellite => Network.Observe(satellite))
                .OrderBy(observation => observation.Start.ToSystemDateTime())
                .ToList();
            args.Result = observations;
        }

        public SatelliteObservation GetNextObservation()
        {
            if (_todaysObservations.Count < 0)
                return null;
            return _todaysObservations.First(observation =>
                observation.Start.ToSystemDateTime().ToLocalTime() > DateTime.Now);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs args)
        {
        }

        private void MainWindow_UpdateFrame(object sender, FrameEventArgs e)
        {
            var t = (float)(50 * e.Time);

            if (KeyboardState[Key.Left])
                _rotation.Y -= t;
            if (KeyboardState[Key.Right])
                _rotation.Y += t;
            if (KeyboardState[Key.Up])
                _rotation.X -= t;
            if (KeyboardState[Key.Down])
                _rotation.X += t;

            if (DateTime.Now >= _observationsDirtyTime)
            {
                ObserverBackgroundWorker.RunWorkerAsync();
                _observationsDirtyTime = DateTime.Now + TimeSpan.FromMinutes(10);
            }

            //_rotation.Y += t * 0.1f;
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            Lumberjack.Debug($"Set viewport: {ClientRectangle}");

            // Set up the viewport
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1000, 1000);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void MainWindow_RenderFrame(object sender, FrameEventArgs e)
        {
            // Start profiling
            _profiler.Start("render");

            // Update sparklines
            if (_profile.ContainsKey("render"))
                _renderTimeSparkline.Enqueue((float)_profile["render"].TotalMilliseconds);

            _fpsSparkline.Enqueue((float)RenderFrequency);

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
            GL.Enable(EnableCap.Blend);
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

                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(2, 0xAAAA);
                GL.LineWidth(1);

                var time = SGP4_Sharp.DateTime.Now().AddMinutes(-2);
                GL.Begin(PrimitiveType.LineStrip);
                for (var i = 0; i < 8; i++)
                {
                    var predictEci = satellite
                        .Predict(time);
                    var predictPos = predictEci
                                     .ToGeodetic()
                                     .ToSpherical() / 100;

                    GL.Color3(Network.IsVisible(predictEci) ? Color.Blue : Color.Yellow);

                    GL.Vertex3(predictPos);
                    time = time.AddMinutes(1);
                }
                GL.End();

                GL.Color3(Color.Yellow);
                GL.LineStipple(4, 0xAAAA);
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
                GL.Disable(EnableCap.LineStipple);
            }
            GL.Disable(EnableCap.Blend);
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

            GL.PushMatrix();
            GL.Color3(Color.White);
            if (KeyboardState[Key.D])
            {
                // Static diagnostic header
                GL.PushMatrix();
                Font.RenderString($"FPS: {(int)Math.Ceiling(RenderFrequency)}\n" +
                                  $"Fast Graphics: {FastGraphics}");
                GL.PopMatrix();

                // Sparklines
                GL.Translate(0, Height - (int)(Font.Common.LineHeight * 1.4f * 2), 0);
                _fpsSparkline.Render();
                GL.Translate(0, (int)(Font.Common.LineHeight * 1.4f), 0);
                _renderTimeSparkline.Render();
            }
            else
            {
                Font.RenderString($"Development Build");

                GL.Translate(0, Height - Font.Common.LineHeight, 0);
                if (_todaysObservations.Count > 0)
                {
                    var next = GetNextObservation();
                    var time = next.Start.ToSystemDateTime().ToLocalTime();
                    Font.RenderString(
                        $"Next: {next.Satellite.Name} at {(time):h\\:mm\\:ss} (T-{time - DateTime.Now:h\\:mm\\:ss})");
                }
                else if (ObserverBackgroundWorker.IsBusy)
                    Font.RenderString($"Recalculating observations...");
                else
                    Font.RenderString($"No observations <1d");
            }

            GL.PopMatrix();

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

            // Stop profiling and get the results
            _profiler.End();
            _profile = _profiler.Reset();
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

            const float diffuse = 1.5f;
            float[] matDiffuse = { diffuse, diffuse, diffuse };
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, matDiffuse);
            GL.Light(LightName.Light0, LightParameter.Position, new[] { 0.0f, 0.0f, 0.0f, 256.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new[] { diffuse, diffuse, diffuse, diffuse });
            Lumberjack.Debug("Created lights");

            // Set up lighting
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.ShadeModel(ShadingModel.Smooth);

            // Set up caps
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.RescaleNormal);
            Lumberjack.Debug("Loaded OpenGL settings");

            // Load the font
            Font = BitmapFont.LoadBinaryFont("dina", FontBank.FontDina, FontBank.BmDina);
            Lumberjack.Debug("Loaded fonts");

            // Load sparklines
            _fpsSparkline = new Sparkline(Font, $"0-{(int)TargetRenderFrequency}fps", 50,
                (float)TargetRenderFrequency, Sparkline.SparklineStyle.Area);
            _renderTimeSparkline = new Sparkline(Font, "0-50ms", 50, 50, Sparkline.SparklineStyle.Area);

            Earth.Init();
            Lumberjack.Debug("Loaded Earth entity");

            Network.Satellites.Add(new Satellite(
                "NOAA 19",
                "1 33591U 09005A   18133.56712982  .00000090  00000-0  73994-4 0  9996",
                "2 33591  99.1396 110.9116 0014982  65.8583 294.4152 14.12277340477026"
            ));

            Network.Satellites.Add(new Satellite(
                "NOAA 18",
                "1 28654U 05018A   18133.60637508  .00000026  00000-0  39118-4 0  9994",
                "2 28654  99.1482 168.3501 0013661 221.0542 138.9601 14.12375330668802"
            ));

            Network.Satellites.Add(new Satellite(
                "ISS (ZARYA)",
                "1 25544U 98067A   18133.55965878  .00079487  00000-0  11977-2 0  9991",
                "2 25544  51.6395 191.3285 0003974  83.9302  21.4973 15.54083292113132"
            ));

            Network.Satellites.Add(new Satellite(
                "ATLAS CENTAUR 2",
                "1 00694U 63047A   18133.50403603  .00000393  00000-0  40022-4 0  9993",
                "2 00694  30.3551 147.2516 0586973  17.7385 344.2819 14.02316828729119"
            ));

            Lumberjack.Info("Window loaded");
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
