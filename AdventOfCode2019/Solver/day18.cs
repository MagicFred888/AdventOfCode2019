using AdventOfCode2019.Extensions;
using AdventOfCode2019.Tools;
using System.Drawing;

namespace AdventOfCode2019.Solver;

internal partial class Day18 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Many-Worlds Interpretation";

    private QuickMatrix _maze = new();
    private List<Point> _mazeEntriesPosition = [];
    private Dictionary<string, Point> _allKeysPosition = [];
    private Dictionary<string, Point> _allDoorsPosition = [];

    public override string GetSolution1(bool isChallenge)
    {
        // Get data
        ExtractData(1);

        // Solve
        return CollectAllKeysPart1().ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        // Get data
        ExtractData(2);

        // Solve
        return CollectAllKeysPart2().ToString();
    }

    private int CollectAllKeysPart1()
    {
        // Make all doors as walls initially
        foreach (var door in _allDoorsPosition)
        {
            _maze.Cell(door.Value).StringVal = "#";
        }

        // Map keys to bit positions for faster state tracking
        Dictionary<string, int> keyToBit = _allKeysPosition.Keys
            .Select((key, index) => new { key, index })
            .ToDictionary(x => x.key, x => x.index);

        // Using A* with a priority queue to explore the maze
        PriorityQueue<(Point position, int cumulatedSteps, int collectedKeys), int> toCheck = new();
        Dictionary<long, int> visited = [];
        toCheck.Enqueue((_mazeEntriesPosition[0], 0, 0), 0);
        int minSteps = int.MaxValue;

        static long EncodeState(int x, int y, int keys) => ((long)x << 48) | ((long)y << 32) | (long)keys;

        while (toCheck.Count > 0)
        {
            // Get next move to check
            var (currentPosition, currentCumulatedSteps, collectedKeys) = toCheck.Dequeue();

            // Generate unique state key
            long visitedKey = EncodeState(currentPosition.X, currentPosition.Y, collectedKeys);
            if (visited.TryGetValue(visitedKey, out int existingSteps) && currentCumulatedSteps >= existingSteps)
            {
                continue;
            }
            visited[visitedKey] = currentCumulatedSteps;

            // If all keys are collected, update min steps
            if (collectedKeys == (1 << _allKeysPosition.Count) - 1)
            {
                minSteps = Math.Min(minSteps, currentCumulatedSteps);
                continue;
            }

            // Adjust maze doors
            foreach ((string doorName, Point doorPosition) in _allDoorsPosition)
            {
                _maze.Cell(doorPosition).StringVal = ((collectedKeys & (1 << keyToBit[doorName])) != 0) ? "." : "#";
            }

            // Cache distances from current position to all other positions
            long[,] distanceFromPosition = QuickMaze.CalculateDistancesToPosition(_maze, currentPosition, "#");

            // Explore remaining keys
            Dictionary<string, Point> remainingKeys = _allKeysPosition.Where(k => (collectedKeys & (1 << keyToBit[k.Key])) == 0).ToDictionary(k => k.Key, k => k.Value);
            foreach ((string keyName, Point keyPosition) in remainingKeys)
            {
                // Check if the key is reachable
                if (distanceFromPosition[keyPosition.X, keyPosition.Y] < 0)
                {
                    continue;
                }

                // Add new state to priority queue
                int newCollectedKeys = collectedKeys | (1 << keyToBit[keyName]);
                int newSteps = currentCumulatedSteps + (int)distanceFromPosition[keyPosition.X, keyPosition.Y];
                int heuristic = newSteps + Heuristic(keyPosition, [.. remainingKeys.Values]);
                toCheck.Enqueue((keyPosition, newSteps, newCollectedKeys), heuristic);
            }
        }

        // Done
        return minSteps;
    }

    private static int Heuristic(Point currentPosition, List<Point> remainingKeys)
    {
        // Simple heuristic: sum of Manhattan distances to all remaining keys
        return remainingKeys.Sum(key => Math.Abs(currentPosition.X - key.X) + Math.Abs(currentPosition.Y - key.Y));
    }

    private int CollectAllKeysPart2()
    {
        // Make all doors as walls initially
        foreach (var door in _allDoorsPosition)
        {
            _maze.Cell(door.Value).StringVal = "#";
        }

        // Map keys to bit positions for faster state tracking
        Dictionary<string, int> keyToBit = _allKeysPosition.Keys
            .Select((key, index) => new { key, index })
            .ToDictionary(x => x.key, x => x.index);

        // Using DFS with a stack to explore the maze
        PriorityQueue<(List<Point> positions, int cumulatedSteps, int collectedKeys), int> toCheck = new();
        Dictionary<long, int> visited = [];
        toCheck.Enqueue((new(_mazeEntriesPosition), 0, 0), 0);
        int minSteps = int.MaxValue;

        static long EncodeState(int x, int y, int keys) => ((long)x << 48) | ((long)y << 32) | (long)keys;

        while (toCheck.Count > 0)
        {
            // Get next move to check
            var (currentPositions, currentCumulatedSteps, collectedKeys) = toCheck.Dequeue();

            // Generate unique state key
            for (int positionId = 0; positionId < currentPositions.Count; positionId++)
            {
                Point currentPosition = currentPositions[positionId];
                long visitedKey = EncodeState(currentPosition.X, currentPosition.Y, collectedKeys);
                if (visited.TryGetValue(visitedKey, out int existingSteps) && currentCumulatedSteps >= existingSteps)
                {
                    continue;
                }
                visited[visitedKey] = currentCumulatedSteps;

                // If all keys are collected, update min steps
                if (collectedKeys == (1 << _allKeysPosition.Count) - 1)
                {
                    minSteps = Math.Min(minSteps, currentCumulatedSteps);
                    continue;
                }

                // Adjust maze doors
                foreach ((string doorName, Point doorPosition) in _allDoorsPosition)
                {
                    _maze.Cell(doorPosition).StringVal = ((collectedKeys & (1 << keyToBit[doorName])) != 0) ? "." : "#";
                }
                // Cache distances from current position to all other positions
                long[,] distanceFromPosition = QuickMaze.CalculateDistancesToPosition(_maze, currentPosition, "#");

                // Explore remaining keys
                Dictionary<string, Point> remainingKeys = _allKeysPosition.Where(k => (collectedKeys & (1 << keyToBit[k.Key])) == 0).ToDictionary(k => k.Key, k => k.Value);
                foreach ((string keyName, Point keyPosition) in remainingKeys)
                {
                    // Check if the key is reachable
                    if (distanceFromPosition[keyPosition.X, keyPosition.Y] < 0)
                    {
                        continue;
                    }

                    // Add new state to priority queue
                    int newCollectedKeys = collectedKeys | (1 << keyToBit[keyName]);
                    int newSteps = currentCumulatedSteps + (int)distanceFromPosition[keyPosition.X, keyPosition.Y];
                    int heuristic = newSteps + Heuristic(keyPosition, [.. remainingKeys.Values]);
                    List<Point> newPositions = new(currentPositions)
                    {
                        [positionId] = keyPosition
                    };
                    toCheck.Enqueue((newPositions, newSteps, newCollectedKeys), heuristic);
                }
            }
        }

        // Done
        return minSteps;
    }

    private void ExtractData(int step)
    {
        _maze = new(_puzzleInput);
        _allKeysPosition = _maze.Cells.Where(c => char.IsLower(c.StringVal[0])).ToDictionary(c => c.StringVal.ToUpper(), c => c.Position);
        _allDoorsPosition = _maze.Cells.Where(c => char.IsUpper(c.StringVal[0])).ToDictionary(c => c.StringVal.ToUpper(), c => c.Position);
        _mazeEntriesPosition = _maze.Cells.FindAll(c => c.StringVal == "@").ConvertAll(c => c.Position);
        if (step == 2)
        {
            // Replace center with 4 new entries
            Point entry = _mazeEntriesPosition[0];
            _maze.Cell(entry.Add(new(-1, -1))).StringVal = "@";
            _maze.Cell(entry.Add(new(1, -1))).StringVal = "@";
            _maze.Cell(entry.Add(new(-1, 1))).StringVal = "@";
            _maze.Cell(entry.Add(new(1, 1))).StringVal = "@";
            _maze.Cell(entry.Add(new(-1, 0))).StringVal = "#";
            _maze.Cell(entry.Add(new(1, 0))).StringVal = "#";
            _maze.Cell(entry.Add(new(0, -1))).StringVal = "#";
            _maze.Cell(entry.Add(new(0, 1))).StringVal = "#";
            _maze.Cell(entry).StringVal = "#";

            // Update entries
            _mazeEntriesPosition = _maze.Cells.FindAll(c => c.StringVal == "@").ConvertAll(c => c.Position);
        }
    }
}