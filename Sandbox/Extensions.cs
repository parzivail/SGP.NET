using OpenTK;

namespace Sandbox
{
    internal static class Extensions
    {
        public static Vector3 ToGlVector3(this SGPdotNET.Util.Vector3 v)
        {
            return new Vector3((float) v.X, (float) v.Y, (float) v.Z);
        }
    }
}