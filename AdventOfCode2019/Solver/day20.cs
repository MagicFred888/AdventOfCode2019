using AdventOfCode2019.Extensions;
using AdventOfCode2019.Tools;
using System.Drawing;

namespace AdventOfCode2019.Solver;

internal partial class Day20 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Donut Maze";

    private QuickMatrix _maze = new();
    private (Point position, Point direction) _entry = new();
    private Point _exit = new();
    private readonly Dictionary<Point, (string teleportName, Point exitPosition, Point exitDirection)> _allTeleport = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        return SpecialQuickMaze.GetMazeBestDistance(_maze, _entry.position, _exit, "#", _allTeleport).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();

        // Split portals in two groups
        Dictionary<Point, (string teleportName, Point exitPosition, Point exitDirection)> innerTeleport = [];
        Dictionary<Point, (string teleportName, Point exitPosition, Point exitDirection)> outerTeleport = [];
        foreach (KeyValuePair<Point, (string teleportName, Point exitPosition, Point exitDirection)> teleport in _allTeleport)
        {
            if (teleport.Key.X < 5 || teleport.Key.Y < 5 || teleport.Key.X > _maze.ColCount - 5 || teleport.Key.Y > _maze.RowCount - 5)
            {
                outerTeleport.Add(teleport.Key, teleport.Value);
            }
            else
            {
                innerTeleport.Add(teleport.Key, teleport.Value);
            }
        }

        return SpecialQuickMaze.GetMazeBestDistanceMultiLevel(_maze, _entry.position, _exit, "#", innerTeleport, outerTeleport).ToString();
    }

    private void ExtractData()
    {
        // Extract map
        _maze = new QuickMatrix(_puzzleInput);

        // Scan map to get all doors
        List<(string doorName, Point entryPosition, Point exitPosition, Point exitDirection)> allTeleportDoors = [];
        foreach (CellInfo InOutletterCell in _maze.Cells.Where(c => c.StringVal is not "." and not "#" and not " "))
        {
            // Are we on the entry point?
            if (!_maze.GetNeighbours(InOutletterCell.Position, TouchingMode.HorizontalAndVertical).Any(c => c.StringVal == "."))
            {
                continue;
            }

            // Get other letter
            CellInfo otherLetterCell = _maze.GetNeighbours(InOutletterCell.Position, TouchingMode.HorizontalAndVertical)
                .Find(c => c.StringVal is not "." and not "#" and not " ")!;

            // Compute name
            string teleportName;
            if (InOutletterCell.Position.X < otherLetterCell.Position.X || InOutletterCell.Position.Y < otherLetterCell.Position.Y)
            {
                teleportName = $"{InOutletterCell.StringVal}{otherLetterCell.StringVal}";
            }
            else
            {
                teleportName = $"{otherLetterCell.StringVal}{InOutletterCell.StringVal}";
            }

            // Get exit position
            Point mazePos = _maze.GetNeighbours(InOutletterCell.Position, TouchingMode.HorizontalAndVertical)
                .Find(c => c.StringVal is ".")!.Position;

            // Get exit direction
            Point exitDirection = mazePos.Subtract(InOutletterCell.Position);

            // Save
            allTeleportDoors.Add((teleportName, InOutletterCell.Position, mazePos, exitDirection));

            // Plug entry and exit door to avoid passing outside
            if (teleportName == "AA" || teleportName == "ZZ")
            {
                InOutletterCell.StringVal = "#";
            }
        }

        // Save entry and exit infos
        (string doorName, Point entryPosition, Point exitPosition, Point exitDirection) entry = allTeleportDoors.Find(d => d.doorName == "AA");
        _entry = (entry.exitPosition, entry.exitDirection);
        allTeleportDoors.Remove(entry);
        (string doorName, Point entryPosition, Point exitPosition, Point exitDirection) exit = allTeleportDoors.Find(d => d.doorName == "ZZ");
        _exit = exit.exitPosition;
        allTeleportDoors.Remove(exit);

        // Build teleport dictionary
        _allTeleport.Clear();
        while (allTeleportDoors.Count > 0)
        {
            // Get pair of doors
            List<(string doorName, Point entryPosition, Point exitPosition, Point exitDirection)> doors = [allTeleportDoors[0]];
            allTeleportDoors.RemoveAt(0);
            doors.Add(allTeleportDoors.Find(d => d.doorName == doors[0].doorName)!);
            allTeleportDoors.Remove(doors[1]);
            for (int i = 0; i < doors.Count; i++)
            {
                _allTeleport.Add(doors[i].entryPosition, (doors[0].doorName, doors[1 - i].exitPosition, doors[1 - i].exitDirection));
            }
        }
    }
}