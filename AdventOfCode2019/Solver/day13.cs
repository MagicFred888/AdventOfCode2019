using System.Drawing;

namespace AdventOfCode2019.Solver;

internal partial class Day13 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Care Package";

    private readonly Dictionary<int, string> _objects = new()
    {
        { 0, " " },
        { 1, "#" },
        { 2, "B" },
        { 3, "_" },
        { 4, "O" }
    };

    private readonly Dictionary<Point, string> _GameField = [];

    public override string GetSolution1(bool isChallenge)
    {
        _GameField.Clear();
        ShipComputer shipComputer = new(_puzzleInput[0])
        {
            PauseAfterXOutputNbr = 3
        };
        do
        {
            shipComputer.RunProgram();
            if (shipComputer.Output.Count == 3)
            {
                Point position = new((int)shipComputer.Output.Dequeue(), (int)shipComputer.Output.Dequeue());
                _GameField[position] = _objects[(int)shipComputer.Output.Dequeue()];
            }
        } while (shipComputer.IsRunning);
        return _GameField.Values.Count(v => v == "B").ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        int score = 0;
        _GameField.Clear();
        ShipComputer shipComputer = new(string.Concat("2", _puzzleInput[0].AsSpan(1)))
        {
            PauseAfterXOutputNbr = 3
        };
        do
        {
            shipComputer.RunProgram();
            if (shipComputer.Output.Count == 3)
            {
                Point position = new((int)shipComputer.Output.Dequeue(), (int)shipComputer.Output.Dequeue());
                if (position == new Point(-1, 0))
                {
                    // Update score
                    score = (int)shipComputer.Output.Dequeue();

                    // Uncomment if you want see the game in Output window
                    //Debug.WriteLine($"Score: {(int)shipComputer.Output.Dequeue()}");
                    //QuickGrid game = new QuickGrid(_GameField, " ");
                    //game.DebugPrint(CellInfoContentType.String);
                }
                else
                {
                    // Update Game field
                    _GameField[position] = _objects[(int)shipComputer.Output.Dequeue()];
                }
            }
            else
            {
                // Need play
                int ballX = _GameField.First(kvp => kvp.Value == "O").Key.X;
                int padleX = _GameField.First(kvp => kvp.Value == "_").Key.X;
                if (padleX > ballX)
                {
                    // Move paddle left
                    shipComputer.Input.Enqueue(-1);
                }
                else if (padleX < ballX)
                {
                    // Move paddle right
                    shipComputer.Input.Enqueue(1);
                }
                else
                {
                    // Do nothing
                    shipComputer.Input.Enqueue(0);
                }
            }
        } while (shipComputer.IsRunning);

        // Game ended
        return score.ToString();
    }

    private sealed class ShipComputer
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

        private int _nbrOfOutput = 0;
        private long _relativeBase = 0;
        private long _instructionPointer = 0;
        private Dictionary<long, long> _computerWorkingMemory = [];

        public ShipComputer(string computerInitialMemory)
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
            if (!IsRunning)
            {
                // Initialize
                _nbrOfOutput = 0;
                _instructionPointer = 0;
                _relativeBase = 0;
                _computerWorkingMemory = new(ComputerInitialMemory);
                Output.Clear();
                IsRunning = true;
            }

            int cycleNbr = 0;
            do
            {
                // Read opcode and define Modes
                cycleNbr++;
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
                        return;
                    }
                    _computerWorkingMemory[GetParmaValue(_instructionPointer + 1, parametersMode[0], true)] = Input.Dequeue();
                    _instructionPointer += 2;
                    continue;
                }
                else if (opcode == Opcode.Output)
                {
                    Output.Enqueue(GetParmaValue(_instructionPointer + 1, parametersMode[0], false));
                    _instructionPointer += 2;
                    if (PauseAfterXOutputNbr > 0 && ++_nbrOfOutput % PauseAfterXOutputNbr == 0)
                    {
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