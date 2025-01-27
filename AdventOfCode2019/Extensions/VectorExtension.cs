using System.Numerics;

namespace AdventOfCode2019.Extensions;

public static class VectorExtension
{
    /// <summary>
    /// Computes the angle in degree of the vector from the 12 o'clock position in a clockwise direction.
    /// </summary>
    /// <param name="vector">The vector for which to compute the angle.</param>
    /// <returns>The angle in degrees, ranging from 0 to 360.</returns>
    public static double Angle(this Vector2 vector)
    {
        // Compute the angle from 12 o'clock in clockwise direction
        double angle = Math.Atan2(vector.X, vector.Y) * 180 / Math.PI;
        return angle < 0 ? angle + 360 : angle;
    }
}