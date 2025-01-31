namespace AdventOfCode2019.Solver;

internal partial class Day16 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Flawed Frequency Transmission";

    public override string GetSolution1(bool isChallenge)
    {
        List<int> data = _puzzleInput[0].ToCharArray().Select(c => int.Parse(c.ToString())).ToList();
        int nbrOfCycle = _puzzleInput.Count > 1 ? int.Parse(_puzzleInput[1]) : 100;
        List<int> basePattern = [0, 1, 0, -1];

        // Compute
        data = ComputeFlawedFrequencyTransmission(data, basePattern, nbrOfCycle);
        return string.Join("", data.Take(8));
    }

    public override string GetSolution2(bool isChallenge)
    {
        List<int> data = _puzzleInput[0].ToCharArray().Select(c => int.Parse(c.ToString())).ToList();
        List<int> extendedInput = [];
        for (int i = 0; i < 10_000; i++)
        {
            extendedInput.AddRange(data);
        }
        int offset = int.Parse(_puzzleInput[0][..7]);

        // Compute
        extendedInput = ComputeFlawedFrequencyTransmission(extendedInput, 100, offset);
        return string.Join("", extendedInput.Skip(offset).Take(8));
    }

    private static List<int> ComputeFlawedFrequencyTransmission(List<int> data, int nbrOfCycles, int offset)
    {
        // Work only if the offset is in the second half of the data
        // Thanks to https://www.reddit.com/r/adventofcode/comments/ebai4g/2019_day_16_solutions/ for the hint

        List<int> _transformedData = new int[data.Count].ToList();
        for (int i = 0; i < nbrOfCycles; i++)
        {
            int cumulativeSum = 0;
            for (int currentIndex = offset; currentIndex < data.Count; currentIndex++)
            {
                cumulativeSum += data[currentIndex];
            }
            for (int currentIndex = offset; currentIndex < data.Count; currentIndex++)
            {
                _transformedData[currentIndex] = cumulativeSum % 10;
                cumulativeSum -= data[currentIndex];
            }
            (data, _transformedData) = (_transformedData, data);
        }
        return data;
    }

    private static List<int> ComputeFlawedFrequencyTransmission(List<int> data, List<int> basePattern, int nbrOfCycle)
    {
        for (int phaseId = 0; phaseId < nbrOfCycle; phaseId++)
        {
            List<int> newData = [];
            for (int outputId = 0; outputId < data.Count; outputId++)
            {
                int sum = 0;
                for (int inputId = 0; inputId < data.Count; inputId++)
                {
                    sum += data[inputId] * basePattern[(inputId + 1) / (outputId + 1) % basePattern.Count];
                }
                newData.Add(Math.Abs(sum) % 10);
            }
            data = newData;
        }
        return data;
    }
}