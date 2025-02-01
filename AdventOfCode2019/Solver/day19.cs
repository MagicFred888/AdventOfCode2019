using AdventOfCode2019.Extensions;
using AdventOfCode2019.Tools;
using System.Drawing;

namespace AdventOfCode2019.Solver;

internal partial class Day19 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Tractor Beam";

    public override string GetSolution1(bool isChallenge)
    {
        QuickMatrix area = new(50, 50, 0);
        IntcodeComputer intcode = new(_puzzleInput[0]);
        for (int y = 0; y < 50; y++)
        {
            for (int x = 0; x < 50; x++)
            {
                intcode.Input.Enqueue(x);
                intcode.Input.Enqueue(y);
                intcode.RunProgram();
                area.Cell(x, y).LongVal = intcode.Output.Dequeue();
            }
        }
        return area.Cells.Count(c => c.LongVal > 0).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        // Initailize IntcodeComputer
        IntcodeComputer intcode = new(_puzzleInput[0]);

        // Make a first quick scan at x = 1000
        int y1 = 0;
        int beamYstart = -1;
        int beamYstop;
        do
        {
            y1 += 1;
            intcode.Input.Enqueue(1000);
            intcode.Input.Enqueue(y1);
            intcode.RunProgram();
            long beam = intcode.Output.Dequeue();
            if (beamYstart == -1 && beam > 0)
            {
                beamYstart = y1;
            }
            else if (beamYstart > 0 && beam <= 0)
            {
                beamYstop = y1 - 1;
                break;
            }
        } while (true);

        // Based on quick scan, make an estimation of the target position
        int estimationErrorMargin = 25;

        double tanAngle = (double)beamYstart / 1000d;
        double targetHigh = 100d * (1 + tanAngle);
        double highAt1000 = beamYstop - beamYstart + 1;
        double ratio = targetHigh / highAt1000;

        // Evaluate X pos
        int probableX = (int)Math.Round(1000d * ratio);
        int xStart = probableX - estimationErrorMargin;
        int xStop = probableX + 100 + estimationErrorMargin;

        // Evaluate Y pos
        int probableY = (int)Math.Round((probableX + 100) * ((double)beamYstart / 1000d));
        int yStart = probableY - estimationErrorMargin;
        int yStop = probableY + 100 + estimationErrorMargin;

        // Scan the area
        QuickGrid grid = new(xStart, xStop, yStart, yStop, ".");
        for (int x = xStart; x <= xStop; x++)
        {
            for (int y = yStart; y <= yStop; y++)
            {
                intcode.Input.Enqueue(x);
                intcode.Input.Enqueue(y);
                intcode.RunProgram();
                grid.Cell(x, y).StringVal = intcode.Output.Dequeue() > 0 ? "#" : ".";
            }
        }

        // Search best position
        double minDistance = double.MaxValue;
        Point minPoint = new();
        foreach (Point startPos in grid.Cells.Select(c => c.Position))
        {
            if (ScanFrom(grid, startPos, new(1, 0)) != 100 || ScanFrom(grid, startPos, new(0, 1)) != 100)
            {
                continue;
            }
            if (startPos.ManhattanDistance() < minDistance)
            {
                minDistance = startPos.ManhattanDistance();
                minPoint = startPos;
            }
        }

        // Return result
        return (10000 * minPoint.X + minPoint.Y).ToString();
    }

    private static int ScanFrom(QuickGrid grid, Point position, Point direction)
    {
        int result = 0;
        while (grid.Cell(position).StringVal == "#")
        {
            result++;
            position = position.Add(direction);
        }
        return result;
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
    }
}