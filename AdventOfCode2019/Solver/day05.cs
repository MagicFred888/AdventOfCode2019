namespace AdventOfCode2019.Solver;

internal partial class Day05 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Sunny with a Chance of Asteroids";

    public override string GetSolution1(bool isChallenge)
    {
        List<int> program = _puzzleInput[0].Split(',').ToList().ConvertAll(int.Parse);
        ShipComputer shipComputer = new(program);
        List<int> output = shipComputer.RunProgram(1);
        return output[^1].ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        List<int> program = _puzzleInput[0].Split(',').ToList().ConvertAll(int.Parse);
        ShipComputer shipComputer = new(program);
        List<int> output = shipComputer.RunProgram(5);
        return output[^1].ToString();
    }

    private sealed class ShipComputer(List<int> computerInitialMemory)
    {
        private enum Opcode
        {
            Add = 1,
            Multiply = 2,
            Input = 3,
            Output = 4,
            JumpIfTrue = 5,
            JumpIfFalse = 6,
            LessThan = 7,
            Equals = 8,
            End = 99
        }

        private enum Mode
        {
            Position = 0,
            Immediate = 1
        }

        public List<int> ComputerInitialMemory => computerInitialMemory;

        public List<int> RunProgram(int input)
        {
            int instructionPointer = 0;
            List<int> output = [];
            List<int> computerMemory = new(ComputerInitialMemory);

            do
            {
                // Read opcode and define Modes
                string fullOpcode = computerMemory[instructionPointer].ToString().PadLeft(5, '0');
                Opcode opcode = (Opcode)int.Parse(fullOpcode[3..]);
                Mode modeParam1 = (Mode)int.Parse(fullOpcode[2].ToString());
                Mode modeParam2 = (Mode)int.Parse(fullOpcode[1].ToString());

                // Check if we must end ?
                if (opcode == Opcode.End)
                {
                    return output;
                }

                // Check if input or output ?
                if (opcode == Opcode.Input)
                {
                    computerMemory[computerMemory[instructionPointer + 1]] = input;
                    instructionPointer += 2;
                    continue;
                }
                else if (opcode == Opcode.Output)
                {
                    output.Add(computerMemory[computerMemory[instructionPointer + 1]]);
                    instructionPointer += 2;
                    continue;
                }

                // Get proper values
                int param1 = computerMemory[instructionPointer + 1];
                if (modeParam1 == Mode.Position)
                {
                    param1 = computerMemory[param1];
                }
                int param2 = computerMemory[instructionPointer + 2];
                if (modeParam2 == Mode.Position)
                {
                    param2 = computerMemory[param2];
                }
                int param3 = computerMemory[instructionPointer + 3];

                // Execute opcode
                int jumpSize = 0;
                switch (opcode)
                {
                    case Opcode.Add:
                        computerMemory[param3] = param1 + param2;
                        jumpSize = 4;
                        break;

                    case Opcode.Multiply:
                        computerMemory[param3] = param1 * param2;
                        jumpSize = 4;
                        break;

                    case Opcode.JumpIfTrue:
                        if (param1 != 0)
                        {
                            instructionPointer = param2;
                            continue;
                        }
                        jumpSize = 3;
                        break;

                    case Opcode.JumpIfFalse:
                        if (param1 == 0)
                        {
                            instructionPointer = param2;
                            continue;
                        }
                        jumpSize = 3;
                        break;

                    case Opcode.LessThan:
                        computerMemory[param3] = param1 < param2 ? 1 : 0;
                        jumpSize = 4;
                        break;

                    case Opcode.Equals:
                        computerMemory[param3] = param1 == param2 ? 1 : 0;
                        jumpSize = 4;
                        break;

                    default:
                        break;
                }

                // Move to next instruction
                instructionPointer += jumpSize;
            } while (true);
        }
    }
}