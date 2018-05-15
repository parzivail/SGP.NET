using System;
using System.Text;


namespace SGP4_Sharp
{
  /**
 * @brief Generic vector
 *
 * Stores x, y, z, w
 */
  public class Vector
  {
    /**
     * Default constructor
     */
    public Vector()
    {
      x = 0.0;
      y = 0.0;
      z = 0.0;
      w = 0.0;
    }

    /**
     * Constructor
     * @param arg_x x value
     * @param arg_y y value
     * @param arg_z z value
     */
    public Vector(double arg_x,
                  double arg_y,
                  double arg_z)
    {
      x = arg_x;
      y = arg_y; 
      z = arg_z;
      w = 0.0;
    }

    /**
     * Constructor
     * @param arg_x x value
     * @param arg_y y value
     * @param arg_z z value
     * @param arg_w w value
     */
    public Vector(double arg_x,
                  double arg_y,
                  double arg_z,
                  double arg_w)
    {
      x = (arg_x);
      y = (arg_y);
      z = (arg_z);
      w = (arg_w);
    }

    /**
     * Copy constructor
     * @param v value to copy from
     */
    public Vector(Vector v)
    {
      x = v.x;
      y = v.y;
      z = v.z;
      w = v.w;
    }

    /**
     * Subtract operator
     * @param v value to suctract from
     */
    public static Vector operator -(Vector v, Vector v2)
    {
      return new Vector(v.x - v2.x,
        v.y - v2.y,
        v.z - v2.z,
        0.0);
    }

    /**
     * Calculates the magnitude of the vector
     * @returns magnitude of the vector
     */
    public double Magnitude()
    {
      return Math.Sqrt(x * x + y * y + z * z);
    }

    /**
     * Calculates the dot product
     * @returns dot product
     */
    public double Dot(Vector vec)
    {
      return (x * vec.x) +
      (y * vec.y) +
      (z * vec.z);
    }

    /**
     * Converts this vector to a string
     * @returns this vector as a string
     */
    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();
      builder.Append($"X: {x}");
      builder.Append($", Y: {y}");
      builder.Append($", Z: {z}");
      builder.Append($", W: {w}");
      
      return builder.ToString();
    }

    /** x value */
    public double x;
    /** y value */
    public double y;
    /** z value */
    public double z;
    /** w value */
    public double w;
  }

}
