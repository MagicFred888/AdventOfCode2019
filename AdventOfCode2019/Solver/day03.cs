using AdventOfCode2019.Extensions;
using System.Drawing;

namespace AdventOfCode2019.Solver;

internal partial class Day03 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Crossed Wires";

    private readonly List<List<Point>> _wirePath = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        List<Point> commonPoints = _wirePath[0].Intersect(_wirePath[1]).ToList();
        commonPoints.Remove(new(0, 0));
        return commonPoints.Min(p => Math.Abs(p.X) + Math.Abs(p.Y)).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        List<Point> commonPoints = _wirePath[0].Intersect(_wirePath[1]).ToList();
        commonPoints.Remove(new(0, 0));
        return commonPoints.Min(p => _wirePath[0].IndexOf(p) + _wirePath[1].IndexOf(p)).ToString();
    }

    private void ExtractData()
    {
        _wirePath.Clear();
        foreach (string line in _puzzleInput)
        {
            Point current = new(0, 0);
            List<Point> wirePoints = [current];
            foreach (string move in line.Split(','))
            {
                char moveDirectionChar = move[0];
                int distance = int.Parse(move[1..]);
                Point direction = moveDirectionChar switch
                {
                    'U' => new Point(0, -1),
                    'D' => new Point(0, 1),
                    'L' => new Point(-1, 0),
                    'R' => new Point(1, 0),
                    _ => throw new InvalidOperationException("Invalid direction"),
                };
                for (int i = 0; i < distance; i++)
                {
                    current = current.Add(direction);
                    wirePoints.Add(current);
                }
            }
            _wirePath.Add(wirePoints);
        }
    }
}