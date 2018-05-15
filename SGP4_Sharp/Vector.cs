using System;
using System.Text;


namespace SGP4_Sharp
{
    /// <summary>
    /// Generic vector
    /// </summary>
    public class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }

        public Vector() : this(0, 0, 0, 0)
        {
        }

        /**
         * Constructor
         * @param x x value
         * @param y y value
         * @param z z value
         */
        public Vector(double x,
                      double y,
                      double z)
        {
            X = x;
            Y = y;
            Z = z;
            W = 0.0;
        }

        /**
         * Constructor
         * @param x x value
         * @param y y value
         * @param z z value
         * @param w w value
         */
        public Vector(double x,
                      double y,
                      double z,
                      double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /**
         * Copy constructor
         * @param v value to copy from
         */
        public Vector(Vector v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = v.W;
        }

        /**
         * Subtract operator
         * @param v value to suctract from
         */
        public static Vector operator -(Vector v, Vector v2)
        {
            return new Vector(v.X - v2.X, v.Y - v2.Y, v.Z - v2.Z, 0);
        }

        /**
         * Calculates the magnitude of the vector
         * @returns magnitude of the vector
         */
        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        /**
         * Calculates the dot product
         * @returns dot product
         */
        public double Dot(Vector vec)
        {
            return X * vec.X + Y * vec.Y + Z * vec.Z;
        }

        /**
         * Converts this vector to a string
         * @returns this vector as a string
         */
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"X: {X}");
            builder.Append($", Y: {Y}");
            builder.Append($", Z: {Z}");
            builder.Append($", W: {W}");

            return builder.ToString();
        }
    }
}
