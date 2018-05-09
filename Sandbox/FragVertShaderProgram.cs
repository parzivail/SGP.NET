using OpenTK.Graphics.OpenGL;
using PFX.Shader;

namespace Sandbox
{
    class FragVertShaderProgram : ShaderProgram
    {
        private readonly string _program;
        private readonly string _vertprogram;

        public FragVertShaderProgram(string program, string vertprogram)
        {
            _program = program;
            _vertprogram = vertprogram;
        }

        protected override void Init()
        {
            LoadShader(_program, ShaderType.FragmentShader, PgmId, out FsId);
            LoadShader(_vertprogram, ShaderType.VertexShader, PgmId, out VsId);

            GL.LinkProgram(PgmId);
            Log(GL.GetProgramInfoLog(PgmId));
        }
    }
}
