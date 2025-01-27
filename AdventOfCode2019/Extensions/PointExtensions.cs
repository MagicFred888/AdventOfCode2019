using System.Drawing;
using System.Numerics;

namespace AdventOfCode2019.Extensions;

public static class PointExtensions
{
    /// <summary>
    /// Calculates the unit vector from the current point to the target point and rounds the result to the specified number of decimal places.
    /// </summary>
    /// <param name="point">The starting point.</param>
    /// <param name="target">The target point.</param>
    /// <param name="nbrOfDecimal">The number of decimal places to round to.</param>
    /// <returns>A unit vector pointing from the current point to the target point, rounded to the specified number of decimal places.</returns>
    public static Vector2 UnitVectorTo(this Point point, Point target, int nbrOfDecimal)
    {
        Vector2 vector = UnitVectorTo(point, target);
        return new Vector2((float)Math.Round(vector.X, nbrOfDecimal), (float)Math.Round(vector.Y, nbrOfDecimal));
    }

    /// <summary>
    /// Calculates the unit vector from the current point to the target point.
    /// </summary>
    /// <param name="point">The starting point.</param>
    /// <param name="target">The target point.</param>
    /// <returns>A unit vector pointing from the current point to the target point.</returns>
    public static Vector2 UnitVectorTo(this Point point, Point target)
    {
        Point vector = point.Subtract(target);
        float length = (float)Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));
        return new Vector2(vector.X / length, vector.Y / length);
    }

    /// <summary>
    /// Calculates the Manhattan distance from the current point to the origin (0, 0).
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The Manhattan distance from the current point to the origin.</returns>
    public static long ManhattanDistance(this Point point)
    {
        return Math.Abs(point.X) + Math.Abs(point.Y);
    }

    /// <summary>
    /// Calculates the Manhattan distance from the current point to the target point.
    /// </summary>
    /// <param name="point">The starting point.</param>
    /// <param name="target">The target point.</param>
    /// <returns>The Manhattan distance from the current point to the target point.</returns>
    public static long ManhattanDistance(this Point point, Point target)
    {
        return Math.Abs(point.X - target.X) + Math.Abs(point.Y - target.Y);
    }

    /// <summary>
    /// Rotates the current point 90 degrees clockwise around the origin (0, 0).
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <returns>The rotated point.</returns>
    public static Point RotateClockwise(this Point point)
    {
        return point.RotateClockwise(new Point(0, 0));
    }

    /// <summary>
    /// Rotates the current point 90 degrees clockwise around the specified center point.
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <param name="centerPoint">The center point to rotate around.</param>
    /// <returns>The rotated point.</returns>
    public static Point RotateClockwise(this Point point, Point centerPoint)
    {
        int x = point.X - centerPoint.X;
        int y = point.Y - centerPoint.Y;
        return new Point(centerPoint.X + y, centerPoint.Y - x);
    }

    /// <summary>
    /// Rotates the current point 90 degrees counterclockwise around the origin (0, 0).
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <returns>The rotated point.</returns>
    public static Point RotateCounterclockwise(this Point point)
    {
        return point.RotateCounterclockwise(new Point(0, 0));
    }

    /// <summary>
    /// Rotates the current point 90 degrees counterclockwise around the specified center point.
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <param name="centerPoint">The center point to rotate around.</param>
    /// <returns>The rotated point.</returns>
    public static Point RotateCounterclockwise(this Point point, Point centerPoint)
    {
        int x = point.X - centerPoint.X;
        int y = point.Y - centerPoint.Y;
        return new Point(centerPoint.X - y, centerPoint.Y + x);
    }

    /// <summary>
    /// Rotates the current point 180 degrees around the origin (0, 0).
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <returns>The rotated point.</returns>
    public static Point Rotate180Degree(this Point point)
    {
        return point.Rotate180Degree(new Point(0, 0));
    }

    /// <summary>
    /// Rotates the current point 180 degrees around the specified center point.
    /// </summary>
    /// <param name="point">The point to rotate.</param>
    /// <param name="centerPoint">The center point to rotate around.</param>
    /// <returns>The rotated point.</returns>
    public static Point Rotate180Degree(this Point point, Point centerPoint)
    {
        int x = point.X - centerPoint.X;
        int y = point.Y - centerPoint.Y;
        return new Point(centerPoint.X - x, centerPoint.Y - y);
    }

    /// <summary>
    /// Subtracts the specified point from the current point.
    /// </summary>
    /// <param name="p1">The point to subtract from.</param>
    /// <param name="p2">The point to subtract.</param>
    /// <returns>The result of the subtraction.</returns>
    public static Point Subtract(this Point p1, Point p2)
    {
        return new Point(p1.X - p2.X, p1.Y - p2.Y);
    }

    /// <summary>
    /// Adds the specified point to the current point.
    /// </summary>
    /// <param name="p1">The point to add to.</param>
    /// <param name="p2">The point to add.</param>
    /// <returns>The result of the addition.</returns>
    public static Point Add(this Point p1, Point p2)
    {
        return new Point(p1.X + p2.X, p1.Y + p2.Y);
    }

    /// <summary>
    /// Adds the specified x and y values to the current point.
    /// </summary>
    /// <param name="p1">The point to add to.</param>
    /// <param name="x">The x value to add.</param>
    /// <param name="y">The y value to add.</param>
    /// <returns>The result of the addition.</returns>
    public static Point Add(this Point p1, int x, int y)
    {
        return new Point(p1.X + x, p1.Y + y);
    }

    /// <summary>
    /// Multiplies the current point by the specified factor.
    /// </summary>
    /// <param name="p">The point to multiply.</param>
    /// <param name="factor">The factor to multiply by.</param>
    /// <returns>The result of the multiplication.</returns>
    public static Point Multiply(this Point p, int factor)
    {
        return new Point(p.X * factor, p.Y * factor);
    }
}