namespace AdventOfCode2019.Solver;

internal partial class Day02 : BaseSolver
{
    public override string PuzzleTitle { get; } = "1202 Program Alarm";

    public override string GetSolution1(bool isChallenge)
    {
        List<int> program = _puzzleInput[0].Split(',').ToList().ConvertAll(int.Parse);
        if (isChallenge)
        {
            program[1] = 12;
            program[2] = 2;
        }
        program = RunProgram(program);
        return program[0].ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        List<int> referenceComputerMemory = _puzzleInput[0].Split(',').ToList().ConvertAll(int.Parse);
        for (int noun = 0; noun <= 99; noun++)
        {
            for (int verb = 0; verb <= 99; verb++)
            {
                List<int> computerMemory = new(referenceComputerMemory)
                {
                    [1] = noun,
                    [2] = verb
                };
                computerMemory = RunProgram(computerMemory);
                if (computerMemory[0] == 19690720)
                {
                    return (100 * noun + verb).ToString();
                }
            }
        }
        throw new InvalidDataException("No solution for current dataset!");
    }

    private static List<int> RunProgram(List<int> computerMemory)
    {
        int instructionPointer = 0;
        do
        {
            int opcode = computerMemory[instructionPointer];
            if (opcode == 99)
            {
                return computerMemory;
            }
            computerMemory[computerMemory[instructionPointer + 3]] = opcode switch
            {
                1 => computerMemory[computerMemory[instructionPointer + 1]] + computerMemory[computerMemory[instructionPointer + 2]],
                2 => computerMemory[computerMemory[instructionPointer + 1]] * computerMemory[computerMemory[instructionPointer + 2]],
                _ => throw new NotImplementedException()
            };
            instructionPointer += 4;
        } while (true);
    }
}