namespace AdventOfCode2019.Solver;

internal partial class Day23 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Category Six";

    private readonly List<IntcodeComputer> _allComputer = [];
    private readonly List<(int destination, long X, long Y)> networkMessages = [];

    public override string GetSolution1(bool isChallenge)
    {
        LoadData();
        return RunNetwork(true);
    }

    public override string GetSolution2(bool isChallenge)
    {
        LoadData();
        return RunNetwork(false);
    }

    private string RunNetwork(bool stopWhenGettingFirstNatPacket)
    {
        // Run network until we get the first Y value sent to address 255
        bool networkIsIdle;
        long lastNatY = -1;
        (int destination, long X, long Y) natMemory = (-1, -1, -1);
        do
        {
            networkIsIdle = true;
            foreach (IntcodeComputer computer in _allComputer)
            {
                computer.RunProgram();
                while (networkMessages.Count != 0)
                {
                    networkIsIdle = false;
                    (int destination, long X, long Y) = networkMessages[0];
                    networkMessages.RemoveAt(0);
                    if (destination == 255)
                    {
                        if (stopWhenGettingFirstNatPacket)
                        {
                            return Y.ToString();
                        }
                        natMemory = (destination, X, Y);
                    }
                    else
                    {
                        _allComputer[destination].Input.Enqueue(X);
                        _allComputer[destination].Input.Enqueue(Y);
                    }
                }
            }
            if (networkIsIdle)
            {
                _allComputer[0].Input.Enqueue(natMemory.X);
                _allComputer[0].Input.Enqueue(natMemory.Y);
                if (lastNatY == natMemory.Y)
                {
                    return natMemory.Y.ToString();
                }
                lastNatY = natMemory.Y;
            }
        } while (true);
    }

    private void LoadData()
    {
        _allComputer.Clear();
        for (int i = 0; i < 50; i++)
        {
            IntcodeComputer computer = new(_puzzleInput[0])
            {
                NetworkMessages = networkMessages
            };
            computer.Input.Enqueue(i);
            _allComputer.Add(computer);
        }
    }

    private sealed class IntcodeComputer
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
            AdjustsRelativeBase = 9,
            End = 99
        }

        private enum Mode
        {
            Position = 0,
            Immediate = 1,
            Relative = 2,
        }

        public bool IsRunning { get; private set; } = false;
        public Dictionary<long, long> ComputerInitialMemory { get; init; }
        public Queue<long> Input { get; init; } = [];
        public Queue<long> Output { get; init; } = [];

        public int PauseAfterXOutputNbr { get; set; } = -1;
        public List<(int destination, long X, long Y)> NetworkMessages { get; set; } = [];

        private long _relativeBase = 0;
        private long _instructionPointer = 0;
        private Dictionary<long, long> _computerWorkingMemory = [];

        public IntcodeComputer(string computerInitialMemory)
        {
            // Initialize
            ComputerInitialMemory = [];
            foreach (string value in computerInitialMemory.Split(','))
            {
                ComputerInitialMemory.Add(ComputerInitialMemory.Count, long.Parse(value));
            }
        }

        public void RunProgram()
        {
            List<long> partialMessage = [];
            int nbrInputAtZero = 0;

            if (!IsRunning)
            {
                // Initialize
                _instructionPointer = 0;
                _relativeBase = 0;
                _computerWorkingMemory = new(ComputerInitialMemory);
                Output.Clear();
                IsRunning = true;
            }

            do
            {
                // Read opcode and define Modes
                string fullOpcode = _computerWorkingMemory[_instructionPointer].ToString().PadLeft(5, '0');
                int opcodeValue = int.Parse(fullOpcode[3..]);
                if (!Enum.IsDefined(typeof(Opcode), opcodeValue))
                {
                    throw new InvalidOperationException($"Invalid opcode: {opcodeValue}");
                }
                Opcode opcode = (Opcode)opcodeValue;

                // Check if we must end ?
                if (opcode == Opcode.End)
                {
                    IsRunning = false;
                    return;
                }

                // Extract parameters mode and values for needed one
                List<Mode> parametersMode = [];
                parametersMode.Add((Mode)int.Parse(fullOpcode[2].ToString()));
                parametersMode.Add((Mode)int.Parse(fullOpcode[1].ToString()));
                parametersMode.Add((Mode)int.Parse(fullOpcode[0].ToString()));

                // Check if input or output ?
                if (opcode == Opcode.Input)
                {
                    if (Input.Count == 0)
                    {
                        nbrInputAtZero++;
                        _computerWorkingMemory[GetParmaValue(_instructionPointer + 1, parametersMode[0], true)] = -1;
                    }
                    else
                    {
                        nbrInputAtZero = 0;
                        _computerWorkingMemory[GetParmaValue(_instructionPointer + 1, parametersMode[0], true)] = Input.Dequeue();
                    }
                    _instructionPointer += 2;
                    if (nbrInputAtZero > 10)
                    {
                        return;
                    }
                    continue;
                }
                else if (opcode == Opcode.Output)
                {
                    partialMessage.Add(GetParmaValue(_instructionPointer + 1, parametersMode[0], false));
                    _instructionPointer += 2;
                    if (partialMessage.Count == 3)
                    {
                        NetworkMessages.Add(((int)partialMessage[0], partialMessage[1], partialMessage[2]));
                        return;
                    }
                    continue;
                }

                // Get proper values
                List<long> parameters = [];
                int nbrParams = opcode switch
                {
                    Opcode.Add => 3,
                    Opcode.Multiply => 3,
                    Opcode.JumpIfTrue => 2,
                    Opcode.JumpIfFalse => 2,
                    Opcode.LessThan => 3,
                    Opcode.Equals => 3,
                    Opcode.AdjustsRelativeBase => 1,
                    _ => 0,
                };
                for (int i = 0; i < nbrParams; i++)
                {
                    parameters.Add(GetParmaValue(_instructionPointer + 1 + i, parametersMode[i], i == 2));
                }

                // Execute opcode
                switch (opcode)
                {
                    case Opcode.Add:
                        _computerWorkingMemory[parameters[2]] = parameters[0] + parameters[1];
                        break;

                    case Opcode.Multiply:
                        _computerWorkingMemory[parameters[2]] = parameters[0] * parameters[1];
                        break;

                    case Opcode.JumpIfTrue:
                        if (parameters[0] != 0)
                        {
                            _instructionPointer = parameters[1];
                            continue;
                        }
                        break;

                    case Opcode.JumpIfFalse:
                        if (parameters[0] == 0)
                        {
                            _instructionPointer = parameters[1];
                            continue;
                        }
                        break;

                    case Opcode.LessThan:
                        _computerWorkingMemory[parameters[2]] = parameters[0] < parameters[1] ? 1 : 0;
                        break;

                    case Opcode.Equals:
                        _computerWorkingMemory[parameters[2]] = parameters[0] == parameters[1] ? 1 : 0;
                        break;

                    case Opcode.AdjustsRelativeBase:
                        _relativeBase += parameters[0];
                        break;

                    default:
                        break;
                }

                // Move to next instruction
                _instructionPointer += nbrParams + 1;
            } while (true);
        }

        private long GetParmaValue(long memoryPosition, Mode mode, bool isTarget)
        {
            // This is the default mode. For special cases, there is literal mode:
            //     Mode 0 and 1 resolve to raw
            //     Mode 2 resolves to relative_base + raw

            // In interpreted mode:
            //     Mode 0 resolves to ram[raw].
            //     Mode 1 resolves to raw
            //     Mode 2 resolves to ram[relative_base + raw]

            long raw = ReadAndAddIfMissing(memoryPosition);
            if (isTarget)
            {
                return mode switch
                {
                    Mode.Position or Mode.Immediate => raw,
                    Mode.Relative => _relativeBase + raw,
                    _ => throw new InvalidDataException("Invalid mode"),
                };
            }
            else
            {
                return mode switch
                {
                    Mode.Position => ReadAndAddIfMissing(raw),
                    Mode.Immediate => raw,
                    Mode.Relative => ReadAndAddIfMissing(_relativeBase + raw),
                    _ => throw new InvalidDataException("Invalid mode"),
                };
            }
        }

        private long ReadAndAddIfMissing(long memoryPosition)
        {
            if (memoryPosition < 0)
            {
                throw new InvalidDataException("Memory position cannot be negative");
            }
            if (!_computerWorkingMemory.TryGetValue(memoryPosition, out long value))
            {
                value = 0;
                _computerWorkingMemory.Add(memoryPosition, value);
            }
            return value;
        }
    }
}