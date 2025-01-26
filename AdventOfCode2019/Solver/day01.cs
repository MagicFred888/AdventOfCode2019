using AdventOfCode2019.Tools;

namespace AdventOfCode2019.Solver;

internal partial class Day01 : BaseSolver
{
    public override string PuzzleTitle { get; } = "The Tyranny of the Rocket Equation";

    public override string GetSolution1(bool isChallenge)
    {
        List<long> allStage = QuickList.ListOfLong(_puzzleInput);
        return allStage.Sum(v => ComputeFuel(v, false)).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        List<long> allStage = QuickList.ListOfLong(_puzzleInput);
        return allStage.Sum(v => ComputeFuel(v, true)).ToString();
    }

    private static long ComputeFuel(long moduleMass, bool computeFuelForFuel)
    {
        long result = (moduleMass / 3) - 2;
        if (result <= 0)
        {
            return 0;
        }
        return result + (computeFuelForFuel ? ComputeFuel(result, true) : 0);
    }
}