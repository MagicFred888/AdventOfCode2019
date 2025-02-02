using AdventOfCode2019.Tools;
using System.Drawing;

namespace AdventOfCode2019.Solver;

internal partial class Day24 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Planet of Discord";

    public override string GetSolution1(bool isChallenge)
    {
        QuickMatrix eris = new(_puzzleInput);
        List<int> result = [];
        while (true)
        {
            eris = ComputeNextStep(eris);
            int biodiversity = CalculateBiodiversity(eris);
            if (result.Contains(biodiversity))
            {
                return biodiversity.ToString();
            }
            result.Add(biodiversity);
        }
    }

    public override string GetSolution2(bool isChallenge)
    {
        // For tests
        int nbrOfMinutes = isChallenge ? 200 : 10;

        // Create list of eris initial state
        int levelZeroId = nbrOfMinutes / 2;
        List<QuickMatrix> erisList = [];
        while (erisList.Count < nbrOfMinutes + 1)
        {
            erisList.Add(new(5, 5, "."));
        }
        erisList[levelZeroId] = new(_puzzleInput);

        // Run simulation for number of minutes
        for (int minute = 0; minute < nbrOfMinutes; minute++)
        {
            erisList = ComputeNextStepList(erisList);
        }

        // Count bugs
        return erisList.Sum(l => l.Cells.Count(c => c.StringVal == "#")).ToString();
    }

    private static QuickMatrix ComputeNextStep(QuickMatrix eris)
    {
        QuickMatrix result = eris.Clone();
        foreach (CellInfo cell in eris.Cells)
        {
            if (cell.StringVal == "#")
            {
                result.Cell(cell.Position).StringVal = eris.GetNeighbours(cell.Position, TouchingMode.HorizontalAndVertical).Count(c => c.StringVal == "#") == 1 ? "#" : ".";
            }
            else
            {
                result.Cell(cell.Position).StringVal = eris.GetNeighbours(cell.Position, TouchingMode.HorizontalAndVertical).Count(c => c.StringVal == "#") is 1 or 2 ? "#" : ".";
            }
        }
        return result;
    }

    private static int CalculateBiodiversity(QuickMatrix eris)
    {
        int result = 0;
        for (int i = 0; i < eris.Cells.Count; i++)
        {
            int x = i % eris.ColCount;
            int y = i / eris.ColCount;
            if (eris.Cell(x, y).StringVal == "#")
            {
                result += (int)Math.Pow(2, i);
            }
        }
        return result;
    }

    private static List<QuickMatrix> ComputeNextStepList(List<QuickMatrix> erisList)
    {
        List<QuickMatrix> newErisList = [];
        for (int levelId = 0; levelId < erisList.Count; levelId++)
        {
            QuickMatrix currentEris = erisList[levelId];
            QuickMatrix newEris = currentEris.Clone();
            foreach (CellInfo cell in currentEris.Cells)
            {
                if (cell.Position.X == cell.Position.Y && cell.Position.X == currentEris.ColCount / 2)
                {
                    continue; // We don't scan the center anymore
                }

                // Get bugs around
                List<string> currentValueAround = GetValues(cell.Position, erisList, levelId);
                if (cell.StringVal == "#")
                {
                    newEris.Cell(cell.Position).StringVal = currentValueAround.Count(s => s == "#") == 1 ? "#" : ".";
                }
                else
                {
                    newEris.Cell(cell.Position).StringVal = currentValueAround.Count(s => s == "#") is 1 or 2 ? "#" : ".";
                }
            }
            newErisList.Add(newEris);
        }
        return newErisList;
    }

    private static List<string> GetValues(Point position, List<QuickMatrix> erisList, int levelId)
    {
        // Get cells around in current level
        List<CellInfo> cellsAround = erisList[levelId].GetNeighbours(position, TouchingMode.HorizontalAndVertical);

        // Remove center one if in
        if (cellsAround.Any(c => c.Position.X == c.Position.Y && c.Position.X == erisList[0].ColCount / 2))
        {
            cellsAround.RemoveAll(c => c.Position.X == c.Position.Y && c.Position.X == erisList[0].ColCount / 2);
        }

        // Convert as string
        List<string> stringAround = cellsAround.Select(c => c.StringVal).ToList();

        // If we have four string, we can stop here
        if (stringAround.Count == 4)
        {
            return stringAround;
        }

        // Add left outer
        if (position.X == 0 && levelId > 0)
        {
            stringAround.Add(erisList[levelId - 1].Cell(1, 2).StringVal);
        }

        // Add right outer
        if (position.X == erisList[0].ColCount - 1 && levelId > 0)
        {
            stringAround.Add(erisList[levelId - 1].Cell(3, 2).StringVal);
        }

        // Add top outer
        if (position.Y == 0 && levelId > 0)
        {
            stringAround.Add(erisList[levelId - 1].Cell(2, 1).StringVal);
        }

        // Add bottom outer
        if (position.Y == erisList[0].RowCount - 1 && levelId > 0)
        {
            stringAround.Add(erisList[levelId - 1].Cell(2, 3).StringVal);
        }

        // Add left inner
        if (position.X == 1 && position.Y == 2 && levelId < erisList.Count - 1)
        {
            stringAround.AddRange(erisList[levelId + 1].Cols[0].Select(c => c.StringVal));
        }

        // Add right inner
        if (position.X == 3 && position.Y == 2 && levelId < erisList.Count - 1)
        {
            stringAround.AddRange(erisList[levelId + 1].Cols[erisList[levelId + 1].ColCount - 1].Select(c => c.StringVal));
        }

        // Add top inner
        if (position.X == 2 && position.Y == 1 && levelId < erisList.Count - 1)
        {
            stringAround.AddRange(erisList[levelId + 1].Rows[0].Select(c => c.StringVal));
        }

        // Add bottom inner
        if (position.X == 2 && position.Y == 3 && levelId < erisList.Count - 1)
        {
            stringAround.AddRange(erisList[levelId + 1].Rows[erisList[levelId + 1].RowCount - 1].Select(c => c.StringVal));
        }

        // Done
        return stringAround;
    }
}