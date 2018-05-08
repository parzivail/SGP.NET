using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

namespace Sandbox
{
    class MainWindow : GameWindow
    {
        /*
         * Constants
         */
        public static Vector3 UpVector = Vector3.UnitY;
        public static Vector3 PosXVector = Vector3.UnitX;
        public static Vector3 NegXVector = -PosXVector;
        public static Vector3 PosZVector = Vector3.UnitZ;
        public static Vector3 NegZVector = -PosZVector;

        /*
         * Render-related
         */
        private float _zoom = 1;
        private double _angle = 45;
        private double _angleY = 160;

        private static ShaderProgram _shaderProgram;
        private static readonly List<Uniform> Uniforms = new List<Uniform>();
        private static readonly Uniform TintUniform = new Uniform("tint");

        private readonly SimpleVertexBuffer _terrainVbo = new SimpleVertexBuffer();
        private readonly BackgroundWorker _backgroundRenderer = new BackgroundWorker();
        private bool _dirty;

        /*
         * Terrain-related
         */
        private int _numVerts;
        private Color _tintColor;
        private Vector3 _tintColorVector;

        public Color TintColor
        {
            get { return _tintColor; }
            set
            {
                _tintColor = value;
                _tintColorVector = new Vector3(value.R / 255f, value.G / 255f, value.B / 255f);
            }
        }

        /*
         * Window-related
         */
        private bool _shouldDie;
        private Sparkline _fpsSparkline;
        private Sparkline _renderTimeSparkline;
        private readonly Profiler _profiler = new Profiler();
        private static KeyboardState _keyboard;
        private static BitmapFont _font;
        private Dictionary<string, TimeSpan> _profile = new Dictionary<string, TimeSpan>();

        public MainWindow() : base(800, 600)
        {
            // Wire up window
            Load += LoadHandler;
            Closing += CloseHandler;
            Resize += ResizeHandler;
            UpdateFrame += UpdateHandler;
            RenderFrame += RenderHandler;
            MouseWheel += WindowVisualize_MouseWheel;

            // Wire up background worker
            _backgroundRenderer.WorkerReportsProgress = true;
            _backgroundRenderer.WorkerSupportsCancellation = true;
            _backgroundRenderer.DoWork += DoBackgroundRender;
            //            _backgroundRenderer.ProgressChanged += DoBackgroundRenderProgress;
            _backgroundRenderer.RunWorkerCompleted += DoBackgroundRenderComplete;

            TintColor = Color.White;
        }

        private void LoadHandler(object sender, EventArgs e)
        {
            Lumberjack.Log($"Window Thread: {Thread.CurrentThread.ManagedThreadId}");

            // Set up lights
            const float diffuse = 0.9f;
            float[] matDiffuse = { diffuse, diffuse, diffuse };
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, matDiffuse);
            GL.Light(LightName.Light0, LightParameter.Position, new[] { 0.0f, 0.0f, 0.0f, 100.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new[] { diffuse, diffuse, diffuse, diffuse });

            // Set up lighting
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.ColorMaterial);

            // Set up caps
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.RescaleNormal);

            // Set up blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Set background color
            GL.ClearColor(Color.FromArgb(255, 13, 13, 13));

            // Load fonts
            _font = BitmapFont.LoadBinaryFont("dina", FontBank.FontDina, FontBank.BmDina);

            // Load sparklines
            _fpsSparkline = new Sparkline(_font, $"0-{(int)TargetRenderFrequency}fps", 50,
                (float)TargetRenderFrequency, Sparkline.SparklineStyle.Area);
            _renderTimeSparkline = new Sparkline(_font, "0-50ms", 50, 50, Sparkline.SparklineStyle.Area);

            // Init keyboard to ensure first frame won't NPE
            _keyboard = Keyboard.GetState();

            // Load shaders
            _shaderProgram = new DefaultShaderProgram("#version 120\r\n\r\nuniform vec3 tint;\r\n\r\nvoid main() { \r\n\tgl_FragColor = vec4(gl_Color.rgb * tint, 1);\r\n}");
            _shaderProgram.InitProgram();

            Lumberjack.Info("Window loaded");

            _dirty = true;
        }

        private void WindowVisualize_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _zoom -= e.DeltaPrecise / 4f;

            if (_zoom < 0.5f)
                _zoom = 0.5f;
            if (_zoom > 20)
                _zoom = 20;
        }

        private void CloseHandler(object sender, CancelEventArgs e)
        {
            //            if (!_shouldDie)
            //                _terrainLayerList?.Close();
        }

        public void Kill()
        {
            _shouldDie = true;
        }

        private void ResizeHandler(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            var aspectRatio = Width / (float)Height;
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1, 1024);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        public bool IsRendering()
        {
            return _backgroundRenderer.IsBusy;
        }

        public void CancelRender()
        {
            Lumberjack.Warn("Render cancelled");
            _backgroundRenderer.CancelAsync();

            while (IsRendering())
                Application.DoEvents();
        }

        public void CancelBackgroundTasks()
        {
            if (IsRendering())
                CancelRender();
        }

        public void ReRender(bool manualOverride = false, bool regenHeightmap = true)
        {
            // If there's an ongoing render, cancel it
            if (IsRendering())
                CancelRender();

            // Fire up the render
            _backgroundRenderer.RunWorkerAsync(new BackgroundRenderArgs());
        }

        private void DoBackgroundRenderComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // If the render was manually cancelled, go no further
            if (e.Cancelled)
                return;

            Lumberjack.Log($"DBRC Thread: {Thread.CurrentThread.ManagedThreadId}");

            var result = (VertexBufferInitializer)e.Result;
            // Take the render result and upload it to the VBO
            _numVerts = result.Vertices.Count;
            _terrainVbo.InitializeVbo(result);
            GC.Collect();

            // Wait for render thread to exit
            while (IsRendering())
                Application.DoEvents();
        }

        private void DoBackgroundRender(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Grab worker and report progress
                var worker = (BackgroundWorker)sender;
                var args = (BackgroundRenderArgs)e.Argument;

                // Init VBO-needed lists
                var vbi = new VertexBufferInitializer();

                const int size = 100;

                //for (var x = -size; x < size; x++)
                //    for (var y = -size; y < size; y++)
                //    {
                //        var val = new Vector3(x, _terrainGenerator.Eval(x, y), y);
                //        vbi.AddVertex(val, _terrainGenerator.EvalNormal(x, y));

                //        val = new Vector3(x - 1, _terrainGenerator.Eval(x - 1, y), y);
                //        vbi.AddVertex(val, _terrainGenerator.EvalNormal(x - 1, y));

                //        val = new Vector3(x - 1, _terrainGenerator.Eval(x - 1, y - 1), y - 1);
                //        vbi.AddVertex(val, _terrainGenerator.EvalNormal(x - 1, y - 1));

                //        val = new Vector3(x, _terrainGenerator.Eval(x, y - 1), y - 1);
                //        vbi.AddVertex(val, _terrainGenerator.EvalNormal(x, y - 1));
                //    }

                // Send the result back to the worker
                e.Result = vbi;
            }
            catch (Exception ex)
            {
                Lumberjack.Error(ex.Message);
                e.Result = new VertexBufferInitializer();
            }
        }

        private void UpdateHandler(object sender, FrameEventArgs e)
        {
            if (_shouldDie)
                Exit();

            // Grab the new keyboard state
            _keyboard = Keyboard.GetState();

            // Compute input-based rotations
            var delta = e.Time;
            var amount = _keyboard[Key.LShift] || _keyboard[Key.RShift] ? 45 : 90;

            if (_keyboard[Key.Left])
                _angle += amount * delta;
            if (_keyboard[Key.Right])
                _angle -= amount * delta;
            if (_keyboard[Key.Up])
                _angleY += amount * delta;
            if (_keyboard[Key.Down])
                _angleY -= amount * delta;

            if (_dirty)
            {
                ReRender();
                _dirty = false;
            }
        }


        private void RenderHandler(object sender, FrameEventArgs e)
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

            var lookat = Matrix4.LookAt(0, 128, 256, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            // "Center" the terrain
            GL.Translate(0, -25, 0);

            // Zoom and scale the terrain
            var scale = new Vector3(4 * (1 / _zoom), -4 * (1 / _zoom), 4 * (1 / _zoom));
            GL.Scale(scale);
            GL.Rotate(_angleY, 1.0f, 0.0f, 0.0f);
            GL.Rotate(_angle, 0.0f, 1.0f, 0.0f);

            // Reset the frag shader uniforms
            Uniforms.Clear();

            // Set up uniforms
            TintUniform.Value = _tintColorVector;
            Uniforms.Add(TintUniform);

            GL.Color3(Color.White);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            // Engage shader, render, disengage
            _shaderProgram.Use(Uniforms);
            _terrainVbo.Render();
            GL.UseProgram(0);

            // Set up 2D mode
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, 1, -1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.PushMatrix();

            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.Lighting);
            GL.Color3(Color.White);

            // Render diagnostic data
            GL.Enable(EnableCap.Texture2D);
            if (_keyboard[Key.D])
            {
                // Static diagnostic header
                GL.PushMatrix();
                _font.RenderString($"FPS: {(int)Math.Ceiling(RenderFrequency)}");
                GL.Translate(0, _font.Common.LineHeight, 0);
                _font.RenderString($"Verts: {_numVerts}");
                GL.PopMatrix();

                // Sparklines
                GL.Translate(0, Height - _font.Common.LineHeight * 1.4f * 2, 0);
                _fpsSparkline.Render();
                GL.Translate(0, _font.Common.LineHeight * 1.4f, 0);
                _renderTimeSparkline.Render();
            }
            else
            {
                // Info footer
                GL.PushMatrix();
                _font.RenderString($"SPG4Sandbox - Development Build");
                GL.Translate(0, Height - _font.Common.LineHeight, 0);
                _font.RenderString("PRESS 'D' FOR DIAGNOSTICS");
                GL.PopMatrix();
            }

            GL.Disable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Lighting);
            GL.Disable(EnableCap.Blend);

            GL.PopMatrix();

            // Swap the graphics buffer
            SwapBuffers();

            // Stop profiling and get the results
            _profiler.End();
            _profile = _profiler.Reset();
        }
    }
}
