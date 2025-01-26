using AdventOfCode2019.Tools;

namespace AdventOfCode2019.Solver;

internal partial class Day06 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Universal Orbit Map";

    public sealed class Orbit(string name)
    {
        public string Name = name;
        public Orbit? Center { get; set; } = null;
        public List<Orbit> Satellites { get; set; } = [];
        public int NbrOfOrbit => Center == null ? 0 : 1 + Center.NbrOfOrbit;
    }

    private readonly Dictionary<string, Orbit> _orbits = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        return _orbits.Values.Sum(o => o.NbrOfOrbit).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        List<(string from, string to, long transfer)> allOrbitalTrensfer = [];
        foreach (string orbit in _puzzleInput)
        {
            allOrbitalTrensfer.Add((orbit.Split(')')[0], orbit.Split(')')[1], 1));
        }
        QuickDijkstra quickDijkstra = new(allOrbitalTrensfer);
        return (quickDijkstra.GetShortestWay("YOU", "SAN") - 2).ToString();
    }

    private void ExtractData()
    {
        _orbits.Clear();
        foreach (string orbit in _puzzleInput)
        {
            string[] split = orbit.Split(')');
            string centerName = split[0];
            string satelliteName = split[1];
            Orbit center = _orbits.TryGetValue(centerName, out Orbit? valueCenter) ? valueCenter : new Orbit(centerName);
            Orbit satellite = _orbits.TryGetValue(satelliteName, out Orbit? valueSatellite) ? valueSatellite : new Orbit(satelliteName);
            center.Satellites.Add(satellite);
            satellite.Center = center;
            _orbits[centerName] = center;
            _orbits[satelliteName] = satellite;
        }
    }
}