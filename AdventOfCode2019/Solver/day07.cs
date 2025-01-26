namespace AdventOfCode2019.Solver;

internal partial class Day07 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Amplification Circuit";

    public override string GetSolution1(bool isChallenge)
    {
        List<int> program = _puzzleInput[0].Split(',').ToList().ConvertAll(int.Parse);
        if (isChallenge)
        {
            int maxThrust = int.MinValue;
            foreach (List<int> settingSequence in Tools.SmallTools.GeneratePermutations(5))
            {
                int thrust = ChainRun(program, settingSequence);
                if (thrust > maxThrust)
                {
                    maxThrust = thrust;
                }
            }
            return maxThrust.ToString();
        }
        else
        {
            List<int> settingSequence = _puzzleInput[1].Split(',').ToList().ConvertAll(int.Parse);
            return ChainRun(program, settingSequence).ToString();
        }
    }

    public override string GetSolution2(bool isChallenge)
    {
        List<int> program = _puzzleInput[0].Split(',').ToList().ConvertAll(int.Parse);
        if (isChallenge)
        {
            int maxThrust = int.MinValue;
            foreach (List<int> settingSequence in Tools.SmallTools.GeneratePermutations(5))
            {
                int thrust = ChainRunWithFeedbackLoop(program, [.. settingSequence.ConvertAll(i => i + 5)]);
                if (thrust > maxThrust)
                {
                    maxThrust = thrust;
                }
            }
            return maxThrust.ToString();
        }
        else
        {
            List<int> settingSequence = _puzzleInput[1].Split(',').ToList().ConvertAll(int.Parse);
            return ChainRunWithFeedbackLoop(program, settingSequence).ToString();
        }
    }

    private static int ChainRunWithFeedbackLoop(List<int> program, List<int> settingSequence)
    {
        // Initialize computers
        List<ShipComputer> shipComputers = [];
        for (int i = 0; i < settingSequence.Count; i++)
        {
            shipComputers.Add(new(program));
            shipComputers[i].Input.Enqueue(settingSequence[i]);
        }

        // Run loop with feedback loop
        int maxThrust = int.MinValue;
        int computerIndex = 0;
        int amplifierResult = 0;
        do
        {
            for (int i = 0; i < settingSequence.Count; i++)
            {
                // Add input, run and check if we have an output
                shipComputers[computerIndex].Input.Enqueue(amplifierResult);
                shipComputers[computerIndex].RunProgram();
                if (shipComputers[computerIndex].Output.Count == 0)
                {
                    return maxThrust;
                }
                amplifierResult = shipComputers[computerIndex].Output.Dequeue();
                computerIndex = (computerIndex + 1) % settingSequence.Count;
                if (computerIndex == 0 && amplifierResult > maxThrust)
                {
                    maxThrust = amplifierResult;
                }
            }
        }
        while (true);
    }

    private static int ChainRun(List<int> program, List<int> settingSequence)
    {
        int amplifierResult = 0;
        for (int i = 0; i < settingSequence.Count; i++)
        {
            ShipComputer shipComputer = new(program);
            shipComputer.Input.Enqueue(settingSequence[i]);
            shipComputer.Input.Enqueue(amplifierResult);
            shipComputer.RunProgram();
            amplifierResult = shipComputer.Output.Dequeue();
        }
        return amplifierResult;
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

        public bool IsRunning { get; private set; } = false;
        public List<int> ComputerInitialMemory => computerInitialMemory;
        public Queue<int> Input { get; init; } = [];
        public Queue<int> Output { get; init; } = [];

        private int _instructionPointer = 0;
        private List<int> _computerMemory = [];

        public void RunProgram()
        {
            if (!IsRunning)
            {
                // Initialize
                _instructionPointer = 0;
                Output.Clear();
                _computerMemory = new(ComputerInitialMemory);
                IsRunning = true;
            }

            do
            {
                // Read opcode and define Modes
                string fullOpcode = _computerMemory[_instructionPointer].ToString().PadLeft(5, '0');
                Opcode opcode = (Opcode)int.Parse(fullOpcode[3..]);
                Mode modeParam1 = (Mode)int.Parse(fullOpcode[2].ToString());
                Mode modeParam2 = (Mode)int.Parse(fullOpcode[1].ToString());

                // Check if we must end ?
                if (opcode == Opcode.End)
                {
                    IsRunning = false;
                    return;
                }

                // Check if input or output ?
                if (opcode == Opcode.Input)
                {
                    if (Input.Count == 0)
                    {
                        return;
                    }
                    _computerMemory[_computerMemory[_instructionPointer + 1]] = Input.Dequeue();
                    _instructionPointer += 2;
                    continue;
                }
                else if (opcode == Opcode.Output)
                {
                    Output.Enqueue(_computerMemory[_computerMemory[_instructionPointer + 1]]);
                    _instructionPointer += 2;
                    return;
                }

                // Get proper values
                int param1 = _computerMemory[_instructionPointer + 1];
                if (modeParam1 == Mode.Position)
                {
                    param1 = _computerMemory[param1];
                }
                int param2 = _computerMemory[_instructionPointer + 2];
                if (modeParam2 == Mode.Position)
                {
                    param2 = _computerMemory[param2];
                }
                int param3 = _computerMemory[_instructionPointer + 3];

                // Execute opcode
                int jumpSize = 0;
                switch (opcode)
                {
                    case Opcode.Add:
                        _computerMemory[param3] = param1 + param2;
                        jumpSize = 4;
                        break;

                    case Opcode.Multiply:
                        _computerMemory[param3] = param1 * param2;
                        jumpSize = 4;
                        break;

                    case Opcode.JumpIfTrue:
                        if (param1 != 0)
                        {
                            _instructionPointer = param2;
                            continue;
                        }
                        jumpSize = 3;
                        break;

                    case Opcode.JumpIfFalse:
                        if (param1 == 0)
                        {
                            _instructionPointer = param2;
                            continue;
                        }
                        jumpSize = 3;
                        break;

                    case Opcode.LessThan:
                        _computerMemory[param3] = param1 < param2 ? 1 : 0;
                        jumpSize = 4;
                        break;

                    case Opcode.Equals:
                        _computerMemory[param3] = param1 == param2 ? 1 : 0;
                        jumpSize = 4;
                        break;

                    default:
                        break;
                }

                // Move to next instruction
                _instructionPointer += jumpSize;
            } while (true);
        }
    }
}