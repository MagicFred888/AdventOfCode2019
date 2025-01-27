using AdventOfCode2019.Extensions;
using AdventOfCode2019.Tools;
using System.Drawing;
using System.Numerics;

namespace AdventOfCode2019.Solver;

internal partial class Day10 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Monitoring Station";

    public override string GetSolution1(bool isChallenge)
    {
        QuickMatrix asteroidBelt = new(_puzzleInput);
        return GetBestPosition(asteroidBelt).maxInView.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        QuickMatrix asteroidBelt = new(_puzzleInput);
        (_, Point position, Dictionary<Vector2, List<Point>> bestViewInfo) = GetBestPosition(asteroidBelt);

        // Need sort all asteroid in each list from closest to farthest
        foreach (List<Point> asteroidList in bestViewInfo.Values)
        {
            asteroidList.Sort((a, b) => a.ManhattanDistance(position).CompareTo(b.ManhattanDistance(position)));
        }

        // Sort by angle
        int targetAsteroidCount = 200;
        int nbrOfAsteroidsDestroyed = 0;
        List<Vector2> sortedAngles = [.. bestViewInfo.Keys];
        sortedAngles.Sort((a, b) => a.Angle().CompareTo(b.Angle()));
        for (int i = 0; i < int.MaxValue; i++)
        {
            // Because Y is inverted, we need to invert the search to do it clockwise
            // We first take a target from the last angle, then the one before, etc.
            Vector2 vector = sortedAngles[((targetAsteroidCount * sortedAngles.Count) - i) % sortedAngles.Count];

            // Check if we have asteroids to destroy
            if (bestViewInfo[vector].Count == 0)
            {
                continue;
            }

            // Destroy the asteroid
            nbrOfAsteroidsDestroyed++;
            Point destroyedAsteroid = bestViewInfo[vector][0];
            bestViewInfo[vector].RemoveAt(0);

            // Check if we have destroyed the 200th asteroid
            if (nbrOfAsteroidsDestroyed == 200)
            {
                return (destroyedAsteroid.X * 100 + destroyedAsteroid.Y).ToString();
            }
        }

        // Huston, we have a problem
        throw new InvalidDataException("No solution found");
    }

    private static (int maxInView, Point position, Dictionary<Vector2, List<Point>> bestViewInfo) GetBestPosition(QuickMatrix asteroidBelt)
    {
        int maxInViewAtBest = 0;
        Point bestPosition = new(0, 0);
        Dictionary<Vector2, List<Point>> bestViewInfo = [];
        foreach (Point asteroidPosition in asteroidBelt.Cells.FindAll(c => c.StringVal == "#").ConvertAll(a => a.Position))
        {
            Dictionary<Vector2, List<Point>> viewInfo = ComputeViewInfo(asteroidBelt, asteroidPosition);
            if (maxInViewAtBest < viewInfo.Count)
            {
                maxInViewAtBest = viewInfo.Count;
                bestPosition = asteroidPosition;
                bestViewInfo = viewInfo;
            }
        }
        return (maxInViewAtBest, bestPosition, bestViewInfo);
    }

    private static Dictionary<Vector2, List<Point>> ComputeViewInfo(QuickMatrix asteroidBelt, Point asteroidPosition)
    {
        Dictionary<Vector2, List<Point>> viewInfo = [];
        foreach (Point targetAsteroidPosition in asteroidBelt.Cells.FindAll(c => c.StringVal == "#" && c.Position != asteroidPosition).ConvertAll(a => a.Position))
        {
            Vector2 vector = asteroidPosition.UnitVectorTo(targetAsteroidPosition, 5);
            if (!viewInfo.TryGetValue(vector, out List<Point>? value))
            {
                viewInfo[vector] = [targetAsteroidPosition];
            }
            else
            {
                value.Add(targetAsteroidPosition);
            }
        }
        return viewInfo;
    }
}