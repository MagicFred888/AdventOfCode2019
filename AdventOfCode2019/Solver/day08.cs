using AdventOfCode2019.Tools;

namespace AdventOfCode2019.Solver;

internal partial class Day08 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Space Image Format";

    private readonly List<List<char>> _allLayer = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData(isChallenge);
        int minZero = _allLayer.Min(l => l.Count(c => c == '0'));
        List<char> targetLayer = _allLayer.FirstOrDefault(l => l.Count(c => c == '0') == minZero)!;
        return (targetLayer.Count(c => c == '1') * targetLayer.Count(c => c == '2')).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData(isChallenge);
        QuickMatrix image = new(25, 6, ".");
        for (int i = 0; i < 25 * 6; i++)
        {
            foreach (List<char> layer in _allLayer)
            {
                if (layer[i] != '2')
                {
                    image.Cell(i % 25, i / 25).StringVal = layer[i] == '0' ? " " : "#"; // Invert color for easier reading
                    break;
                }
            }
        }
        return string.Join("\r\n    ", image.GetDebugPrintString());
    }

    private void ExtractData(bool isChallenge)
    {
        _allLayer.Clear();
        List<char> layer1 = [.. _puzzleInput[0].ToArray()];
        int pixelPerLayer = isChallenge ? 25 * 6 : 3 * 2;
        while (layer1.Count > 0)
        {
            _allLayer.Add(layer1[..pixelPerLayer]);
            layer1 = layer1[pixelPerLayer..];
        }
    }
}