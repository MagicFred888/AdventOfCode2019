using AdventOfCode2019.Tools;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode2019.Solver;

internal partial class Day25 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Cryostasis";

    private readonly Dictionary<string, string> _exitDoor = new()
    {
        ["north"] = "south",
        ["south"] = "north",
        ["west"] = "east",
        ["east"] = "west",
    };

    private IntcodeComputer remoteDroid = new();
    private readonly string checkPointRoomName = "Security Checkpoint";
    private readonly Dictionary<string, List<string>> _roomPaths = [];
    private readonly List<string> _doNotTouchItems = ["infinite loop", "giant electromagnet"]; // These cannot be handled by this code, adjust to your puzzle

    public override string GetSolution1(bool isChallenge)
    {
        remoteDroid = new(_puzzleInput[0]);
        remoteDroid.RunProgram();

        // Explore ship and back to initial room with collected objects
        ExploreShip();

        // Move to security checkpoint
        foreach (string move in _roomPaths[checkPointRoomName])
        {
            ExecuteCommand(move);
        }

        // Get security check direction
        List<string> doors = ExtractRoomInfos().doors;
        doors.Remove(_roomPaths[checkPointRoomName][^1]);
        string testDoor = doors[0];

        // Get list of all items we carry
        List<string> inventory = GetInventory();

        // Drop all items
        foreach (string item in inventory)
        {
            TakeOrDropItem(item, false);
        }

        // Test each item one by one to eliminate ones that are too heavy
        List<string> tooHeavy = [];
        List<string> tooLight = [];
        foreach (string item in inventory)
        {
            TakeOrDropItem(item, true);
            ExecuteCommand(testDoor);
            if (IsTooHeavy())
            {
                tooHeavy.Add(item);
            }
            else
            {
                tooLight.Add(item);
            }
            TakeOrDropItem(item, false);
        }

        // Test all possible combinations
        List<string> objectsCarried = [];
        for (int nbrOfItem = 2; nbrOfItem < tooLight.Count; nbrOfItem++)
        {
            foreach (List<int> itemList in SmallTools.GenerateCombinations(tooLight.Count, nbrOfItem))
            {
                // Drop carried objects that are not in the list
                foreach (string item in objectsCarried)
                {
                    if (!itemList.Contains(tooLight.IndexOf(item)))
                    {
                        TakeOrDropItem(item, false);
                    }
                }

                // Take objects in the list that are not yet carried
                foreach (int itemIndex in itemList)
                {
                    if (!objectsCarried.Contains(tooLight[itemIndex]))
                    {
                        TakeOrDropItem(tooLight[itemIndex], true);
                    }
                }

                // Update what we carry
                objectsCarried = GetInventory();

                // Test if we can go
                ExecuteCommand(testDoor);
                if (!remoteDroid.IsRunning)
                {
                    // Extract answer and give few details
                    string code = FinalCodeExtractor().Match(GetMultilineOutput().Find(s => FinalCodeExtractor().IsMatch(s)) ?? "").Groups["code"].Value;
                    return $"{code}"; // List of objects available in objectsCarried if interested
                }
            }
        }
        // Oops
        throw new InvalidCastException();
    }

    private void ExecuteCommand(string command)
    {
        foreach (char character in command)
        {
            remoteDroid.Input.Enqueue(character);
        }
        remoteDroid.Input.Enqueue('\n');
        remoteDroid.Output.Clear();
        remoteDroid.RunProgram();
    }

    private bool IsTooHeavy()
    {
        string? message = GetMultilineOutput().FirstOrDefault(s => s.StartsWith("A loud, robotic voice", StringComparison.InvariantCultureIgnoreCase));
        if (string.IsNullOrEmpty(message))
        {
            throw new InvalidDataException();
        }
        return message.Contains("lighter", StringComparison.InvariantCultureIgnoreCase);
    }

    private void TakeOrDropItem(string item, bool takeItem)
    {
        // Drop or take item
        ExecuteCommand($"{(takeItem ? "take" : "drop")} {item}");
    }

    private void ExploreShip()
    {
        // Get initial room info and save it
        (string roomName, List<string> doors, _) = ExtractRoomInfos();
        _roomPaths.Add(roomName, []);

        // Explore all doors
        foreach (string door in doors)
        {
            // Move to next room
            ExecuteCommand(door);

            // Explore
            ExploreRoom([door], _exitDoor[door]);
        }
    }

    private void ExploreRoom(List<string> path, string exitDoor)
    {
        // Get initial room info and save it
        (string roomName, List<string> doors, string item) = ExtractRoomInfos();
        _roomPaths.TryAdd(roomName, path);

        // Is there an item to pick
        if (!string.IsNullOrEmpty(item) && !_doNotTouchItems.Any(i => i.Equals(item, StringComparison.InvariantCultureIgnoreCase)))
        {
            // Backup computer
            IntcodeComputer backup = remoteDroid.Clone();

            // Try to take the item
            TakeOrDropItem(item, true);

            // Dead?
            if (!remoteDroid.IsRunning)
            {
                _doNotTouchItems.Add(item);
                remoteDroid = backup;
            }
        }

        // Explore each room except the exit
        if (roomName != checkPointRoomName)
        {
            doors.Remove(exitDoor);
            foreach (string door in doors)
            {
                // Move to next room
                ExecuteCommand(door);

                // Explore
                List<string> tmpPath = new(path)
                {
                    door
                };
                ExploreRoom(tmpPath, _exitDoor[door]);
            }
        }

        // Exit
        ExecuteCommand(exitDoor);
    }

    private List<string> GetInventory()
    {
        // Ask for inventory
        ExecuteCommand("inv");

        // Return inventory
        return GetMultilineOutput()
            .FindAll(s => s.StartsWith('-'))
            .ConvertAll(s => s.Trim('-', ' '));
    }

    private List<string> GetMultilineOutput()
    {
        StringBuilder output = new();
        while (remoteDroid.Output.Count > 0)
        {
            output.Append((char)remoteDroid.Output.Dequeue());
        }
        return [.. output.ToString().Split('\n')];
    }

    private (string roomName, List<string> doors, string item) ExtractRoomInfos()
    {
        // Extract
        string roomName = string.Empty;
        List<string> doors = [];
        string item = string.Empty;
        foreach (string line in GetMultilineOutput())
        {
            if (line.StartsWith("=="))
            {
                roomName = line.Trim('=', ' ');
            }
            if (line.StartsWith('-'))
            {
                string cleaned = line.Trim('-', ' ');
                if (cleaned is "north" or "south" or "west" or "east")
                {
                    doors.Add(cleaned);
                }
                else
                {
                    item = cleaned;
                }
            }
        }
        return (roomName, doors, item);
    }

    public override string GetSolution2(bool isChallenge)
    {
        return "Merry Christmas!";
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

        public IntcodeComputer()
        {
            ComputerInitialMemory = [];
        }

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

                // Check if we must end?
                if (opcode == Opcode.End)
                {
                    IsRunning = false;
                    return;
                }

                // Extract parameters mode and values for needed ones
                List<Mode> parametersMode = [];
                parametersMode.Add((Mode)int.Parse(fullOpcode[2].ToString()));
                parametersMode.Add((Mode)int.Parse(fullOpcode[1].ToString()));
                parametersMode.Add((Mode)int.Parse(fullOpcode[0].ToString()));

                // Check if input or output?
                if (opcode == Opcode.Input)
                {
                    if (Input.Count == 0)
                    {
                        return;
                    }
                    _computerWorkingMemory[GetParamValue(_instructionPointer + 1, parametersMode[0], true)] = Input.Dequeue();
                    _instructionPointer += 2;
                    continue;
                }
                else if (opcode == Opcode.Output)
                {
                    Output.Enqueue(GetParamValue(_instructionPointer + 1, parametersMode[0], false));
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
                    parameters.Add(GetParamValue(_instructionPointer + 1 + i, parametersMode[i], i == 2));
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

        private long GetParamValue(long memoryPosition, Mode mode, bool isTarget)
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
            IntcodeComputer clone = new(ComputerInitialMemory.ToDictionary(entry => entry.Key, entry => entry.Value))
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

    [GeneratedRegex("(?<code>\\d{4,})")]
    private static partial Regex FinalCodeExtractor();
}