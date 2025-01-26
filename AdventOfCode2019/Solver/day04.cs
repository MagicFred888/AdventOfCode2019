using AdventOfCode2019.Tools;

namespace AdventOfCode2019.Solver;

internal partial class Day04 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Secure Container";

    public override string GetSolution1(bool isChallenge)
    {
        int nbrOfPossiblePasswords = 0;
        if (isChallenge)
        {
            int startRange = int.Parse(_puzzleInput[0].Split('-')[0]);
            int endRange = int.Parse(_puzzleInput[0].Split('-')[1]);
            for (int potentialPassword = startRange; potentialPassword <= endRange; potentialPassword++)
            {
                nbrOfPossiblePasswords += CheckPassword(potentialPassword, false) ? 1 : 0;
            }
        }
        else
        {
            foreach (int potentialPassword in QuickList.ListOfInt(_puzzleInput))
            {
                nbrOfPossiblePasswords += CheckPassword(potentialPassword, false) ? 1 : 0;
            }
        }
        return nbrOfPossiblePasswords.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        int nbrOfPossiblePasswords = 0;
        if (isChallenge)
        {
            int startRange = int.Parse(_puzzleInput[0].Split('-')[0]);
            int endRange = int.Parse(_puzzleInput[0].Split('-')[1]);
            for (int potentialPassword = startRange; potentialPassword <= endRange; potentialPassword++)
            {
                nbrOfPossiblePasswords += CheckPassword(potentialPassword, true) ? 1 : 0;
            }
        }
        else
        {
            foreach (int potentialPassword in QuickList.ListOfInt(_puzzleInput))
            {
                nbrOfPossiblePasswords += CheckPassword(potentialPassword, true) ? 1 : 0;
            }
        }
        return nbrOfPossiblePasswords.ToString(); //476 too low
    }

    private static bool CheckPassword(int potentialPassword, bool maxTwoAdjascent)
    {
        string password = potentialPassword.ToString();
        List<string> passwordBlocks = [];
        int startPos = 0;
        for (int i = 1; i < password.Length; i++)
        {
            if (password[i] < password[i - 1])
            {
                return false;
            }
            if (password[i] != password[i - 1])
            {
                passwordBlocks.Add(password[startPos..i]);
                startPos = i;
            }
        }
        passwordBlocks.Add(password[startPos..password.Length]);

        // Result
        return maxTwoAdjascent ? passwordBlocks.Any(b => b.Length == 2) : passwordBlocks.Any(b => b.Length >= 2);
    }
}