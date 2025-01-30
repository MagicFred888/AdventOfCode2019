using AdventOfCode2019.Tools;
using System.Text.RegularExpressions;

namespace AdventOfCode2019.Solver;

internal partial class Day12 : BaseSolver
{
    public override string PuzzleTitle { get; } = "The N-Body Problem";

    private sealed partial class Moon
    {
        [GeneratedRegex(@"^<x=(?<x>-?[0-9]+), y=(?<y>-?[0-9]+), z=(?<z>-?[0-9]+)>$")]
        private static partial Regex CoordinateExtractionRegex();

        public int[] Position { get; init; }
        public int[] Velocity { get; init; }

        public decimal TotalEnergy => Position.Sum(Math.Abs) * Velocity.Sum(Math.Abs);

        public Moon(string rawData)
        {
            Match match = CoordinateExtractionRegex().Match(rawData);
            if (!match.Success)
            {
                throw new InvalidDataException("Invalid input data");
            }
            Position = [int.Parse(match.Groups["x"].Value), int.Parse(match.Groups["y"].Value), int.Parse(match.Groups["z"].Value)];
            Velocity = [0, 0, 0];
        }

        public void ApplyVelocityChange(Moon moon)
        {
            for (int i = 0; i < Position.Length; i++)
            {
                if (Position[i] < moon.Position[i])
                {
                    Velocity[i]++;
                    moon.Velocity[i]--;
                }
                else if (Position[i] > moon.Position[i])
                {
                    Velocity[i]--;
                    moon.Velocity[i]++;
                }
            }
        }

        public void ComputeNewPosition()
        {
            for (int i = 0; i < Position.Length; i++)
            {
                Position[i] += Velocity[i];
            }
        }
    }

    private int _totalSteps = 1000;
    private readonly List<Moon> _moons = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        for (int step = 0; step < _totalSteps; step++)
        {
            SimulateOneCycle();
        }
        return _moons.Sum(m => m.TotalEnergy).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        List<long> halfCycleDuration = Enumerable.Repeat(-1L, 3).ToList(); // Speed will be like sin curve... As soon as speed is back to 0 we will have half-period

        // Simulate
        for (int step = 0; step < int.MaxValue; step++)
        {
            // Look for speed back to 0
            for (int c = 0; c < 3; c++)
            {
                if (step > 0 && halfCycleDuration[c] == -1 && _moons.All(x => x.Velocity[c] == 0))
                {
                    halfCycleDuration[c] = step;
                }
            }
            if (halfCycleDuration.All(d => d != -1))
            {
                break;
            }

            // Simulate
            SimulateOneCycle();
        }

        // Compute LCM and double it since we have cycle for half period
        return (2 * SmallTools.LCM([.. halfCycleDuration])).ToString();
    }

    private void SimulateOneCycle()
    {
        // Velocity change
        for (int i = 0; i < _moons.Count; i++)
        {
            Moon refMoon = _moons[i];
            for (int j = i + 1; j < _moons.Count; j++)
            {
                refMoon.ApplyVelocityChange(_moons[j]);
            }
        }

        // Position change
        foreach (Moon moon in _moons)
        {
            moon.ComputeNewPosition();
        }
    }

    private void ExtractData()
    {
        _moons.Clear();
        _totalSteps = 1000;
        foreach (string line in _puzzleInput)
        {
            if (line.Contains("x="))
            {
                _moons.Add(new Moon(line));
            }
            else if (line.Contains("steps"))
            {
                // Extra for test samples
                _totalSteps = int.Parse(line.Split(" ")[0]);
            }
        }
    }
}