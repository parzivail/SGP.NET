using System;
using OpenTK;
using SGPdotNET;
using Vector3 = OpenTK.Vector3;

namespace Sandbox
{
    static class Extensions
    {
        public static Vector3 ToGlVector3(this SGPdotNET.Vector3 v)
        {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }
    }
}