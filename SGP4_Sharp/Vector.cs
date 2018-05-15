using System;

namespace SGP4_Sharp
{
    /// <summary>
    ///     Generic 3-dimensional vector
    /// </summary>
    public class Vector
    {
        public Vector() : this(0, 0, 0)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector(double x,
            double y,
            double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="v"></param>
        public Vector(Vector v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

        public static Vector operator -(Vector v, Vector v2)
        {
            return new Vector(v.X - v2.X, v.Y - v2.Y, v.Z - v2.Z);
        }

        /// <summary>
        ///     Calculates the dot product of this vector and another
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public double Dot(Vector vec)
        {
            return X * vec.X + Y * vec.Y + Z * vec.Z;
        }

        public override string ToString()
        {
            return $"Vector[X={X}, Y={Y}, Z={Z}, Length={Length}]";
        }

        public override bool Equals(object obj)
        {
            return obj is Vector vector &&
                   Equals(X, vector.X) &&
                   Equals(Y, vector.Y) &&
                   Equals(Z, vector.Z);
        }

        public override int GetHashCode()
        {
            var hashCode = 707706286;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }
    }
}