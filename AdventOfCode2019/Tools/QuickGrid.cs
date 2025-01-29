using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace AdventOfCode2019.Tools;

public class QuickGrid
{
    public int MinX { get; init; }
    public int MaxX { get; init; }
    public int MinY { get; init; }
    public int MaxY { get; init; }
    public int NbrRow { get; init; }
    public int NbrCol { get; init; }

    public enum TouchingMode
    {
        Horizontal,
        Vertical,
        Diagonal,
        All
    }

    private readonly Dictionary<TouchingMode, List<Point>> _touchingMode = new()
    {
        { TouchingMode.Horizontal, new() { new Point(-1, 0), new Point(1, 0) } },
        { TouchingMode.Vertical, new() { new Point(0, -1), new Point(0, 1) } },
        { TouchingMode.Diagonal, new() { new Point(-1, -1), new Point(1, 1), new Point(-1, 1), new Point(1, -1) } },
        { TouchingMode.All, new() { new Point(-1, -1), new Point(0, -1), new Point(1, -1), new Point(-1, 0), new Point(1, 0), new Point(-1, 1), new Point(0, 1), new Point(1, 1) } }
    };

    private readonly Dictionary<Point, CellInfo> _allCells = [];

    public QuickGrid()
    {
        MinX = 0;
        MaxX = 0;
        MinY = 0;
        MaxY = 0;
        NbrRow = 1;
        NbrCol = 1;
        _allCells.Add(new(0, 0), new(new(0, 0), 0));
    }

    public QuickGrid(int xMin, int xMax, int yMin, int yMax, long defaultValue)
    {
        MinX = xMin;
        MaxX = xMax;
        MinY = yMin;
        MaxY = yMax;
        NbrRow = MaxY - MinY + 1;
        NbrCol = MaxX - MinX + 1;

        for (int x = MinX; x <= MaxX; x++)
        {
            for (int y = MinY; y <= MaxY; y++)
            {
                _allCells.Add(new(x, y), new(new(x, y), defaultValue));
            }
        }
    }

    public QuickGrid(int xMin, int xMax, int yMin, int yMax, string defaultValue)
    {
        MinX = xMin;
        MaxX = xMax;
        MinY = yMin;
        MaxY = yMax;
        NbrRow = MaxY - MinY + 1;
        NbrCol = MaxX - MinX + 1;

        for (int x = MinX; x <= MaxX; x++)
        {
            for (int y = MinY; y <= MaxY; y++)
            {
                _allCells.Add(new(x, y), new(new(x, y), defaultValue));
            }
        }
    }

    public QuickGrid(List<Point> knownCells, string knowntValue, string unknowntValue)
    {
        MinX = knownCells.Min(p => p.X);
        MaxX = knownCells.Max(p => p.X);
        MinY = knownCells.Min(p => p.Y);
        MaxY = knownCells.Max(p => p.Y);

        for (int x = MinX; x <= MaxX; x++)
        {
            for (int y = MinY; y <= MaxY; y++)
            {
                Point position = new(x, y);
                _allCells.Add(position, new(position, knownCells.Contains(position) ? knowntValue : unknowntValue));
            }
        }
    }

    public QuickGrid(Dictionary<Point, string> knownCells, string unknowntValue)
    {
        MinX = knownCells.Min(p => p.Key.X);
        MaxX = knownCells.Max(p => p.Key.X);
        MinY = knownCells.Min(p => p.Key.Y);
        MaxY = knownCells.Max(p => p.Key.Y);

        for (int x = MinX; x <= MaxX; x++)
        {
            for (int y = MinY; y <= MaxY; y++)
            {
                Point position = new(x, y);
                _allCells.Add(position, new(position, knownCells.TryGetValue(position, out string? value) ? value : unknowntValue));
            }
        }
    }

    public CellInfo Cell(Point position) => Cell(position.X, position.Y);

    public CellInfo Cell(int x, int y)
    {
        if (x < MinX || x > MaxX || y < MinY || y > MaxY)
        {
            return new CellInfo(new(int.MinValue, int.MinValue), "");
        }
        return _allCells[new(x, y)];
    }

    public List<CellInfo> TouchingCells(Point position, TouchingMode touchingMode) => TouchingCells(position.X, position.Y, touchingMode);

    public List<CellInfo> TouchingCells(int x, int y, TouchingMode touchingMode)
    {
        List<CellInfo> result = [];
        foreach (Point move in _touchingMode[touchingMode])
        {
            CellInfo? cell = Cell(x + move.X, y + move.Y);
            if (cell != null)
            {
                result.Add(cell);
            }
        }
        return result;
    }

    public void DebugPrint(CellInfoContentType contentType)
    {
        Debug.WriteLine(string.Join("\r\n", GetDebugPrintLines(contentType)));
    }

    public List<string> GetDebugPrintLines(CellInfoContentType contentType)
    {
        List<string> lines = [];
        for (int y = MinY; y <= MaxY; y++)
        {
            StringBuilder line = new();
            for (int x = MinX; x <= MaxX; x++)
            {
                CellInfo? cell = Cell(x, y);
                line.Append(cell == null ? "?" : cell.ToString(contentType));
            }
            lines.Add(line.ToString());
        }
        return lines;
    }

    public List<CellInfo> Cells => new(_allCells.Values);
}