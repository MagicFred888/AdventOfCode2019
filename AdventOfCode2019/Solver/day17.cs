using AdventOfCode2019.Extensions;
using AdventOfCode2019.Tools;
using System.Drawing;
using System.Text;

namespace AdventOfCode2019.Solver;

internal partial class Day17 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Set and Forget";

    public override string GetSolution1(bool isChallenge)
    {
        IntcodeComputer vision = new(_puzzleInput[0]);
        vision.RunProgram();
        QuickMatrix image = GetVideoFeed(vision);
        return image.Cells
            .Where(c => c.StringVal == "#")
            .Where(c => image.GetNeighbours(c.Position, TouchingMode.HorizontalAndVertical).All(n => n.StringVal == "#"))
            .Where(c => image.GetNeighbours(c.Position, TouchingMode.HorizontalAndVertical).Count == 4)
            .Select(c => c.Position)
            .Aggregate(0, (acc, val) => acc + val.X * val.Y)
            .ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        // Get Map
        IntcodeComputer video = new(_puzzleInput[0]);
        video.RunProgram();
        QuickMatrix map = GetVideoFeed(video);

        // Get initial position
        Point robotPos = map.Cells.Find(c => c.StringVal is "^" or "v" or "<" or ">")!.Position;
        Point robotDir = map.Cell(robotPos).StringVal switch
        {
            "^" => new Point(0, -1),
            "v" => new Point(0, 1),
            "<" => new Point(-1, 0),
            ">" => new Point(1, 0),
            _ => throw new InvalidDataException("Invalid robot direction")
        };

        // Simple path, move robot from start to the end of the scaffolding (there are many other way but this one looks ok)
        StringBuilder simplePath = new();
        Point pos = robotPos;
        Point dir = robotDir;
        while (true)
        {
            Point nextPos = pos.Add(dir);
            if (map.Cell(nextPos).IsValid && map.Cell(nextPos).StringVal == "#")
            {
                pos = nextPos;
                simplePath.Append('#');
                continue;
            }
            Point left = dir.RotateClockwise();
            Point right = dir.RotateCounterclockwise();
            if (map.Cell(pos.Add(left)).IsValid && map.Cell(pos.Add(left)).StringVal == "#")
            {
                simplePath.Append('L');
                dir = left;
            }
            else if (map.Cell(pos.Add(right)).IsValid && map.Cell(pos.Add(right)).StringVal == "#")
            {
                simplePath.Append('R');
                dir = right;
            }
            else
            {
                break;
            }
        }

        // Get the sequence of the path
        List<string> fullMoveSequence = MakeSequenceWithoutHashtag(simplePath.ToString());

        // Get sub function sequences
        List<string> functionSequences = FindRepeatingPatterns(fullMoveSequence);

        // Compute main sequence
        List<string> main = [];
        string fullSequence = string.Join(",", fullMoveSequence);
        while (fullSequence != string.Empty)
        {
            string match = functionSequences.FirstOrDefault(s => fullSequence.StartsWith(s))!;
            main.Add("ABC"[functionSequences.IndexOf(match)].ToString());
            fullSequence = fullSequence[match.Length..].Trim(',');
        }

        // join all and inject that in computer Input
        string fullInput = string.Join(",", main) + "\n" + string.Join("\n", functionSequences) + "\nn\n";
        IntcodeComputer robot = new(_puzzleInput[0]);
        robot.ComputerInitialMemory[0] = 2;
        foreach (byte c in Encoding.ASCII.GetBytes(fullInput))
        {
            robot.Input.Enqueue(c);
        }

        // Run the program
        robot.RunProgram();

        // Return last value (robot is talking a lot...)
        return robot.Output.Last().ToString();
    }

    private static List<string> MakeSequenceFromString(string fullSequenceString)
    {
        List<string> sequence = [""];
        foreach (char c in fullSequenceString)
        {
            if (c is 'L' or 'R' or '-')
            {
                if (string.IsNullOrEmpty(sequence[^1]))
                {
                    sequence[^1] = c.ToString();
                }
                else
                {
                    sequence.Add(c.ToString());
                }
                sequence.Add("");
            }
            else
            {
                sequence[^1] += c;
            }
        }
        if (string.IsNullOrEmpty(sequence[^1]))
        {
            sequence.RemoveAt(sequence.Count - 1);
        }
        if (sequence.Contains(""))
        {
            throw new InvalidDataException();
        }
        return sequence;
    }

    private static List<string> MakeSequenceWithoutHashtag(string fullSequenceString)
    {
        List<string> sequence = [];
        int nbrHashtag = 0;
        foreach (char c in fullSequenceString)
        {
            if (c is 'L' or 'R')
            {
                if (nbrHashtag > 0)
                {
                    sequence.Add(nbrHashtag.ToString());
                    nbrHashtag = 0;
                }
                sequence.Add(c.ToString());
            }
            else
            {
                nbrHashtag++;
            }
        }
        if (nbrHashtag > 0)
        {
            sequence.Add(nbrHashtag.ToString());
        }
        return sequence;
    }

    private static List<string> FindRepeatingPatterns(List<string> fullPatterns)
    {
        for (int aSequenceSize = 4; aSequenceSize <= 10; aSequenceSize += 2)
        {
            for (int bSequenceSize = 4; bSequenceSize <= 10; bSequenceSize += 2)
            {
                for (int cSequenceSize = 4; cSequenceSize <= 10; cSequenceSize += 2)
                {
                    string fullString = string.Join(",", fullPatterns);
                    List<string> result = [string.Join(",", fullPatterns.GetRange(0, aSequenceSize))];
                    while (fullString.Length > 0)
                    {
                        string? match = result.Find(s => fullString.StartsWith(s));
                        if (match != null)
                        {
                            fullString = fullString[match.Length..].Trim(',');
                        }
                        else if (result.Count == 1)
                        {
                            List<string> tmp = MakeSequenceFromString(fullString.Replace(",", ""));
                            result.Add(string.Join(",", tmp.GetRange(0, bSequenceSize)));
                        }
                        else if (result.Count == 2)
                        {
                            List<string> tmp = MakeSequenceFromString(fullString.Replace(",", ""));
                            result.Add(string.Join(",", tmp.GetRange(0, cSequenceSize)));
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(fullString))
                    {
                        return result;
                    }
                }
            }
        }
        throw new InvalidDataException();
    }

    private static QuickMatrix GetVideoFeed(IntcodeComputer vision)
    {
        List<string> rawImage = [""];
        while (vision.Output.Count > 0)
        {
            char pixel = (char)vision.Output.Dequeue();
            if (pixel == 10)
            {
                rawImage.Add("");
            }
            else
            {
                rawImage[^1] += pixel;
            }
        }
        while (rawImage[^1] == "")
        {
            rawImage.RemoveAt(rawImage.Count - 1);
        }
        return new QuickMatrix(rawImage);
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