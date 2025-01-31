using AdventOfCode2019.Tools;
using System.Drawing;
using static AdventOfCode2019.Tools.QuickMaze;

namespace AdventOfCode2019.Solver;

internal partial class Day15 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Oxygen System";

    public override string GetSolution1(bool isChallenge)
    {
        QuickMatrix areaToExplore = ScanFullArea(_puzzleInput[0]);
        _ = QuickMaze.SolveMaze(areaToExplore, "S", "O", "#");
        Point oxygenPos = areaToExplore.Cells.Find(c => c.StringVal == "O")?.Position ?? throw new InvalidDataException("O not found!");
        return ((MazeCellInfos)areaToExplore.Cell(oxygenPos).ObjectVal!).DistanceToStart.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        QuickMatrix areaToExplore = ScanFullArea(_puzzleInput[0]);
        _ = QuickMaze.SolveMaze(areaToExplore, "S", "O", "#");
        return areaToExplore.Cells.Max(c => ((MazeCellInfos)c.ObjectVal!).DistanceToEnd).ToString();
    }

    private static QuickMatrix ScanFullArea(string opCode)
    {
        // Initialization
        int gridSize = 60;
        IntcodeComputer computer = new(opCode);
        Point startPos = new(gridSize / 2, gridSize / 2);
        QuickMatrix areaToExplore = new(gridSize, gridSize, " ");

        // Prepare BFS Queue
        Queue<(Point position, IntcodeComputer robotComputer)> toExplore = [];
        toExplore.Enqueue((startPos, computer));
        areaToExplore.Cell(startPos).BoolVal = true;
        areaToExplore.Cell(startPos).StringVal = "S";

        // Scan while we have value
        while (toExplore.Count > 0)
        {
            (Point position, IntcodeComputer robotComputer) = toExplore.Dequeue();
            for (int move = 1; move <= 4; move++)
            {
                // Compute next theorical position
                Point newPosition = move switch
                {
                    1 => new Point(position.X, position.Y - 1),
                    2 => new Point(position.X, position.Y + 1),
                    3 => new Point(position.X - 1, position.Y),
                    4 => new Point(position.X + 1, position.Y),
                    _ => throw new InvalidDataException("Invalid move")
                };

                // Already visited
                if (areaToExplore.Cell(newPosition).BoolVal)
                {
                    continue;
                }

                // Out of grid ?
                if (!areaToExplore.Cell(newPosition).IsValid)
                {
                    continue;
                    throw new InvalidDataException("Full hull not contained into grid!");
                }

                // Check if we can move
                IntcodeComputer tmpComputer = robotComputer.Clone();
                tmpComputer.Input.Enqueue(move);
                tmpComputer.RunProgram();
                long objectFound = tmpComputer.Output.Dequeue();
                if (objectFound == 0)
                {
                    // Hit a
                    areaToExplore.Cell(newPosition).StringVal = "#";
                }
                else
                {
                    // Move is valid
                    areaToExplore.Cell(newPosition).StringVal = objectFound == 1 ? "." : "O";
                    if (!areaToExplore.Cell(newPosition).BoolVal)
                    {
                        toExplore.Enqueue((newPosition, tmpComputer.Clone()));
                    }
                }
                areaToExplore.Cell(newPosition).BoolVal = true;
            }
        }

        // Done
        return areaToExplore;
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

        private int _nbrOfOutput = 0;
        private long _relativeBase = 0;
        private long _instructionPointer = 0;
        private Dictionary<long, long> _computerWorkingMemory = [];

        public IntcodeComputer(Dictionary<long, long> computerInitialMemory)
        {
            ComputerInitialMemory = new(computerInitialMemory);
        }

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

        public IntcodeComputer Clone()
        {
            IntcodeComputer clone = new(ComputerInitialMemory)
            {
                IsRunning = IsRunning,
                Input = new(Input),
                Output = new(Output),
                PauseAfterXOutputNbr = PauseAfterXOutputNbr,
                _nbrOfOutput = _nbrOfOutput,
                _relativeBase = _relativeBase,
                _instructionPointer = _instructionPointer,
                _computerWorkingMemory = _computerWorkingMemory.ToDictionary(entry => entry.Key, entry => entry.Value)
            };
            return clone;
        }
    }
}