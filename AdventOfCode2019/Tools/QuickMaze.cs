using AdventOfCode2019.Extensions;
using System.Drawing;
using System.Text;

namespace AdventOfCode2019.Tools;

public static class QuickMaze
{
    public sealed class MazeCellInfos()
    {
        public bool IsStart { get; set; }

        public bool IsEnd { get; set; }

        public long DistanceToStart { get; set; }

        public long DistanceToEnd { get; set; }

        public long BestDistance { get; set; }

        public bool PartOfBest { get; set; }
    }

    public static bool SolveMaze(QuickMatrix maze, string startValue, string endValue, string wallValue)
    {
        Point startPosition = maze.Cells.Find(c => c.StringVal == startValue)?.Position ?? new(-1, -1);
        Point endPosition = maze.Cells.Find(c => c.StringVal == endValue)?.Position ?? new(-1, -1);
        return SolveMaze(maze, startPosition, endPosition, wallValue);
    }

    public static bool SolveMaze(QuickMatrix maze, Point startPosition, string endValue, string wallValue)
    {
        Point endPosition = maze.Cells.Find(c => c.StringVal == endValue)?.Position ?? new(-1, -1);
        return SolveMaze(maze, startPosition, endPosition, wallValue);
    }

    public static bool SolveMaze(QuickMatrix maze, string startValue, Point endPosition, string wallValue)
    {
        Point startPos = maze.Cells.Find(c => c.StringVal == startValue)?.Position ?? new(-1, -1);
        return SolveMaze(maze, startPos, endPosition, wallValue);
    }

    public static bool SolveMaze(QuickMatrix maze, Point startPosition, Point endPosition, string wallValue)
    {
        long bestDistance = long.MaxValue;
        long[,] _distanceToEnd = CalculateDistancesToPosition(maze, endPosition, wallValue);
        long[,] _distanceToStart = CalculateDistancesToPosition(maze, startPosition, wallValue);
        foreach (var cell in maze.Cells)
        {
            cell.ObjectVal = new MazeCellInfos()
            {
                DistanceToStart = _distanceToStart[cell.Position.X, cell.Position.Y],
                DistanceToEnd = _distanceToEnd[cell.Position.X, cell.Position.Y],
                IsStart = cell.Position == startPosition,
                IsEnd = cell.Position == endPosition
            };
            if (_distanceToStart[cell.Position.X, cell.Position.Y] >= 0 && _distanceToEnd[cell.Position.X, cell.Position.Y] >= 0)
            {
                bestDistance = Math.Min(_distanceToStart[cell.Position.X, cell.Position.Y] + _distanceToEnd[cell.Position.X, cell.Position.Y], bestDistance);
            }
        }
        foreach (var cell in maze.Cells)
        {
            MazeCellInfos info = (MazeCellInfos)cell.ObjectVal!;
            info.BestDistance = bestDistance;
            info.PartOfBest = info.DistanceToStart + info.DistanceToEnd == bestDistance;
        }
        return true;
    }

    public static List<string> GetAllBestDistances(QuickMatrix pad)
    {
        // Rebuid basic info from pre-calculated data
        Point startPosition = pad.Cells.Find(c => ((MazeCellInfos)c.ObjectVal!).IsStart)?.Position ?? new(-1, -1);
        Point endPosition = pad.Cells.Find(c => ((MazeCellInfos)c.ObjectVal!).IsEnd)?.Position ?? new(-1, -1);
        long[,] _distanceToEnd = new long[pad.ColCount, pad.RowCount];
        for (int i = 0; i < pad.ColCount; i++)
        {
            for (int j = 0; j < pad.RowCount; j++)
            {
                _distanceToEnd[i, j] = ((MazeCellInfos)pad.Cell(i, j).ObjectVal!).DistanceToEnd;
            }
        }

        // Get all best paths
        List<List<Point>> allPaths = GetAllBestPaths(pad, startPosition, endPosition, _distanceToEnd);

        // Convert them to directions string
        Dictionary<Point, string> converter = new()
        {
            {new(0, -1), "^"},
            {new(0, 1), "v"},
            {new(-1, 0), "<"},
            {new(1, 0), ">"}
        };
        List<string> pathStrings = [];
        foreach (var path in allPaths)
        {
            StringBuilder result = new();
            for (int i = 1; i < path.Count; i++)
            {
                result.Append(converter[path[i].Subtract(path[i - 1])]);
            }
            pathStrings.Add(result.Append('A').ToString());
        }

        // Return all best paths
        return pathStrings;
    }

    private static List<List<Point>> GetAllBestPaths(QuickMatrix maze, Point startPosition, Point endPosition, long[,] distanceToEnd)
    {
        List<List<Point>> allPaths = [];
        List<Point> currentPath = [];
        FindAllPaths(maze, startPosition, endPosition, distanceToEnd, currentPath, allPaths);
        return allPaths;
    }

    private static void FindAllPaths(QuickMatrix maze, Point currentPosition, Point endPosition, long[,] distanceToEnd, List<Point> currentPath, List<List<Point>> allPaths)
    {
        currentPath.Add(currentPosition);

        if (currentPosition == endPosition)
        {
            allPaths.Add(new List<Point>(currentPath));
            currentPath.RemoveAt(currentPath.Count - 1);
            return;
        }

        long currentDistance = distanceToEnd[currentPosition.X, currentPosition.Y];
        Point[] directions = [new(0, -1), new(0, 1), new(-1, 0), new(1, 0)];

        foreach (var direction in directions)
        {
            Point newPosition = currentPosition.Add(direction);
            if (newPosition.X < 0 || newPosition.X >= maze.ColCount || newPosition.Y < 0 || newPosition.Y >= maze.RowCount || maze.Cell(newPosition).StringVal == "#")
            {
                continue;
            }
            if (distanceToEnd[newPosition.X, newPosition.Y] == currentDistance - 1)
            {
                FindAllPaths(maze, newPosition, endPosition, distanceToEnd, currentPath, allPaths);
            }
        }

        currentPath.RemoveAt(currentPath.Count - 1);
    }

    private static List<Point> GetMazeMove(QuickMatrix maze, Point startPosition, Point endPosition, long[,] distanceToEnd)
    {
        List<Point> result = [];
        if (startPosition == endPosition) return result;

        long iniValue = distanceToEnd[startPosition.X, startPosition.Y];

        // Search all cells around to find cells with score just 1 up from the current cell
        Point[] directions = [new(0, -1), new(0, 1), new(-1, 0), new(1, 0)];
        foreach (var direction in directions)
        {
            Point newPosition = startPosition.Add(direction);
            if (newPosition.X < 0 || newPosition.X >= maze.ColCount || newPosition.Y < 0 || newPosition.Y >= maze.RowCount || maze.Cell(newPosition).StringVal == "#")
            {
                continue;
            }
            if (distanceToEnd[newPosition.X, newPosition.Y] == iniValue - 1)
            {
                result.Add(direction);
                result.AddRange(GetMazeMove(maze, newPosition, endPosition, distanceToEnd));
                break;
            }
        }
        return result;
    }

    public static long[,] CalculateDistancesToPosition(QuickMatrix maze, Point endPosition, string wallValue)
    {
        int rows = maze.RowCount;
        int cols = maze.ColCount;

        long[,] distances = new long[cols, rows];
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                distances[i, j] = -1; // Initialize distances to -1 (unreachable)
            }
        }

        int[] dRow = [-1, 1, 0, 0];
        int[] dCol = [0, 0, -1, 1];

        Queue<(int col, int row)> queue = new();
        queue.Enqueue((endPosition.X, endPosition.Y));
        distances[endPosition.X, endPosition.Y] = 0; // Distance to itself is 0

        while (queue.Count > 0)
        {
            var (col, row) = queue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int newRow = row + dRow[i];
                int newCol = col + dCol[i];

                if (IsValidMove(maze, distances, newRow, newCol, wallValue))
                {
                    distances[newCol, newRow] = distances[col, row] + 1;
                    queue.Enqueue((newCol, newRow));
                }
            }
        }

        return distances;
    }

    private static bool IsValidMove(QuickMatrix maze, long[,] distances, int row, int col, string wallValue)
    {
        return row >= 0 && row < maze.RowCount &&
               col >= 0 && col < maze.ColCount &&
               maze.Cell(col, row).StringVal != wallValue &&
               distances[col, row] == -1;
    }
}