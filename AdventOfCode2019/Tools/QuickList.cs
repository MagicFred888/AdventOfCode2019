namespace AdventOfCode2019.Tools;

public static class QuickList
{
    public static List<long> ListOfLong(List<string> rawData, bool removeEmpty = false)
    {
        return rawData.Aggregate(new List<long>(), (acc, line) =>
        {
            if (long.TryParse(line, out long value))
            {
                acc.Add(value);
            }
            else if (!removeEmpty)
            {
                throw new InvalidDataException();
            }
            return acc;
        });
    }

    public static List<int> ListOfInt(List<string> rawData, bool removeEmpty = false)
    {
        return rawData.Aggregate(new List<int>(), (acc, line) =>
        {
            if (int.TryParse(line, out int value))
            {
                acc.Add(value);
            }
            else if (!removeEmpty)
            {
                throw new InvalidDataException();
            }
            return acc;
        });
    }

    public static List<List<byte>> ListOfListByte(List<string> rawData, string separator = "", bool removeEmpty = false)
    {
        return ListOfListString(rawData, [separator], removeEmpty).ConvertAll(r => r.ConvertAll(v => byte.Parse(v)));
    }

    public static List<List<byte>> ListOfListByte(List<string> rawData, string[] separator, bool removeEmpty = false)
    {
        return ListOfListString(rawData, separator, removeEmpty).ConvertAll(r => r.ConvertAll(v => byte.Parse(v)));
    }

    public static List<List<int>> ListOfListInt(List<string> rawData, string separator = "", bool removeEmpty = false)
    {
        return ListOfListString(rawData, [separator], removeEmpty).ConvertAll(r => r.ConvertAll(v => int.Parse(v)));
    }

    public static List<List<int>> ListOfListInt(List<string> rawData, string[] separator, bool removeEmpty = false)
    {
        return ListOfListString(rawData, separator, removeEmpty).ConvertAll(r => r.ConvertAll(v => int.Parse(v)));
    }

    public static List<List<string>> ListOfListString(List<string> rawData, string[] separator, bool removeEmpty = false)
    {
        // Extract data
        List<List<string>> result = [];
        foreach (string line in rawData)
        {
            List<string> values = [];
            if (separator.Length == 0)
            {
                // Each char in a new box
                for (int x = 0; x < line.Length; x++)
                {
                    values.Add(line[x].ToString());
                }
            }
            else
            {
                // Each item in a cell
                string[] items = line.Split(separator, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
                for (int x = 0; x < items.Length; x++)
                {
                    values.Add(items[x]);
                }
            }
            result.Add(values);
        }
        return result;
    }
}