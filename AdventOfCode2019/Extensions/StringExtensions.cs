namespace AdventOfCode2019.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Determines whether the specified string is numeric.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns><c>true</c> if the string is numeric; otherwise, <c>false</c>.</returns>
    public static bool IsNumeric(this string str)
    {
        return long.TryParse(str, out _);
    }

    /// <summary>
    /// Converts the specified string to a long.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The converted long value.</returns>
    public static long ToLong(this string str)
    {
        return long.Parse(str);
    }

    /// <summary>
    /// Converts the specified string to an int.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The converted int value.</returns>
    public static int ToInt(this string str)
    {
        return int.Parse(str);
    }
}