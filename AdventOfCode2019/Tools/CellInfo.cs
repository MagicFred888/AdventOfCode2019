using System.Drawing;

namespace AdventOfCode2019.Tools;

public enum CellInfoContentType
{
    String,
    Long,
    Bool,
    Object
}

public class CellInfo
{
    public Point Position { get; init; }
    public string StringVal { get; set; } = "";
    public long LongVal { get; set; } = 0;
    public bool BoolVal { get; set; } = false;
    public object? ObjectVal { get; set; } = null;
    public bool IsValid => Position.X != int.MinValue && Position.Y != int.MinValue;

    public CellInfo(int x, int y, string stringVal) : this(new Point(x, y), stringVal)
    { }

    public CellInfo(Point position, string stringVal)
    {
        Position = position;
        StringVal = stringVal;
    }

    public CellInfo(int x, int y, long longVal) : this(new Point(x, y), longVal)
    { }

    public CellInfo(Point position, long longVal)
    {
        Position = position;
        LongVal = longVal;
    }

    public CellInfo(int x, int y, bool boolVal) : this(new Point(x, y), boolVal)
    { }

    public CellInfo(Point position, bool boolVal)
    {
        Position = position;
        BoolVal = boolVal;
    }

    public string ToString(CellInfoContentType contentType)
    {
        return contentType switch
        {
            CellInfoContentType.String => StringVal,
            CellInfoContentType.Long => LongVal.ToString(),
            CellInfoContentType.Bool => BoolVal ? "1" : "0",
            CellInfoContentType.Object => ObjectVal?.ToString() ?? "",
            _ => "X"
        };
    }

    public CellInfo Clone()
    {
        // return a deep copy
        return new CellInfo(Position.X, Position.Y, StringVal)
        {
            LongVal = LongVal,
            BoolVal = BoolVal,
            ObjectVal = ObjectVal
        };
    }

    public void Set(CellInfo cellInfo)
    {
        StringVal = cellInfo.StringVal;
        LongVal = cellInfo.LongVal;
        BoolVal = cellInfo.BoolVal;
        ObjectVal = cellInfo.ObjectVal;
    }
}