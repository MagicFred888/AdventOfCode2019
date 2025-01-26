namespace AdventOfCode2019.Extensions;

public static class StringExtensions
{
    public static bool IsNumeric(this string str)
    {
        return long.TryParse(str, out _);
    }

    public static long ToLong(this string str)
    {
        return long.Parse(str);
    }

    public static int ToInt(this string str)
    {
        return int.Parse(str);
    }
}