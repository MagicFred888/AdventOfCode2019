using System.Drawing;
using System.Runtime.InteropServices;

namespace AdventOfCode2019.Tools;

public static class SpecialQuickMaze
{
    private static Dictionary<Point, (string teleportName, Point exitPosition, Point exitDirection)>? _teleport = null;
    private static Dictionary<Point, (string teleportName, Point exitPosition, Point exitDirection)>? _innerTeleport = null;
    private static Dictionary<Point, (string teleportName, Point exitPosition, Point exitDirection)>? _outerTeleport = null;

    public static long GetMazeBestDistance(QuickMatrix maze, Point startPosition, Point endPosition, string wallValue, Dictionary<Point, (string teleportName, Point exitPosition, Point exitDirection)> teleport)
    {
        _teleport = teleport;
        long[,] _distanceToEnd = CalculateDistancesToPosition(maze, endPosition, wallValue);
        return _distanceToEnd[startPosition.X, startPosition.Y];
    }

    public static long[,] CalculateDistancesToPosition(QuickMatrix maze, Point endPosition, string wallValue)
    {
        int rows = maze.RowCount;
        int cols = maze.ColCount;

        long[,] distances = new long[cols, rows];

        // Get a reference to the first element
        ref long firstElement = ref distances[0, 0];

        // Treat the 2D array as a flat memory block
        var span = MemoryMarshal.CreateSpan(ref firstElement, cols * rows);
        span.Fill(-1);

        int[] dRow = [-1, 1, 0, 0];
        int[] dCol = [0, 0, -1, 1];

        Queue<(int col, int row)> queue = new();
        queue.Enqueue((endPosition.X, endPosition.Y));
        distances[endPosition.X, endPosition.Y] = 0;

        while (queue.Count > 0)
        {
            var (col, row) = queue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int newRow = row + dRow[i];
                int newCol = col + dCol[i];

                // Are we in a teleportal?
                if (_teleport != null && _teleport.ContainsKey(new Point(newCol, newRow)))
                {
                    Point exitPosition = _teleport[new Point(newCol, newRow)].exitPosition;
                    newRow = exitPosition.Y;
                    newCol = exitPosition.X;
                }

                if (IsValidMove(maze, distances, newRow, newCol, wallValue))
                {
                    distances[newCol, newRow] = distances[col, row] + 1;
                    queue.Enqueue((newCol, newRow));
                }
            }
        }

        return distances;
    }

    public static long GetMazeBestDistanceMultiLevel(QuickMatrix maze, Point startPosition, Point endPosition, string wallValue,
        Dictionary<Point, (string teleportName, Point exitPosition, Point exitDirection)> innerTeleport,
        Dictionary<Point, (string teleportName, Point exitPosition, Point exitDirection)> outerTeleport)
    {
        _innerTeleport = innerTeleport;
        _outerTeleport = outerTeleport;
        long[,] _distanceToEnd = CalculateDistancesToPositionMultiLevel(maze, endPosition, wallValue, startPosition);
        return _distanceToEnd[startPosition.X, startPosition.Y];
    }

    public static long[,] CalculateDistancesToPositionMultiLevel(QuickMatrix maze, Point endPosition, string wallValue, Point startPosition)
    {
        int rows = maze.RowCount;
        int cols = maze.ColCount;

        List<long[,]> distances = [];
        distances.Add(CreateNonVisitedLevel(cols, rows));

        int[] dRow = [-1, 1, 0, 0];
        int[] dCol = [0, 0, -1, 1];

        Queue<(int depth, int col, int row)> queue = new();
        queue.Enqueue((0, endPosition.X, endPosition.Y));
        distances[0][endPosition.X, endPosition.Y] = 0;

        while (queue.Count > 0)
        {
            var (depth, col, row) = queue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int newDepth = depth;
                Point newPos = new(col + dCol[i], row + dRow[i]);

                // Are we in a teleportal?
                if (_outerTeleport != null && _outerTeleport.TryGetValue(newPos, out (string teleportName, Point exitPosition, Point exitDirection) value1))
                {
                    newPos = value1.exitPosition;
                    newDepth = depth - 1;
                }
                else if (_innerTeleport != null && _innerTeleport.TryGetValue(newPos, out (string teleportName, Point exitPosition, Point exitDirection) value2))
                {
                    newPos = value2.exitPosition;
                    newDepth = depth + 1;
                }

                // Check
                if (newDepth < 0)
                {
                    continue;
                }
                else if (newDepth >= distances.Count)
                {
                    distances.Add(CreateNonVisitedLevel(cols, rows));
                }

                if (IsValidMove(maze, distances[newDepth], newPos.Y, newPos.X, wallValue))
                {
                    distances[newDepth][newPos.X, newPos.Y] = distances[depth][col, row] + 1;

                    // Reach the end
                    if (newDepth == 0 && newPos == startPosition)
                    {
                        return distances[0];
                    }

                    queue.Enqueue((newDepth, newPos.X, newPos.Y));
                }
            }
        }

        return distances[0];
    }

    private static long[,] CreateNonVisitedLevel(int cols, int rows)
    {
        long[,] result = new long[cols, rows];

        // Get a reference to the first element
        ref long firstElement = ref result[0, 0];

        // Treat the 2D array as a flat memory block
        var span = MemoryMarshal.CreateSpan(ref firstElement, cols * rows);
        span.Fill(-1);

        return result;
    }

    private static bool IsValidMove(QuickMatrix maze, long[,] distances, int row, int col, string wallValue)
    {
        return row >= 0 && row < maze.RowCount &&
               col >= 0 && col < maze.ColCount &&
               maze.Cell(col, row).StringVal != wallValue &&
               distances[col, row] == -1;
    }
}