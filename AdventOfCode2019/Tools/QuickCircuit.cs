namespace AdventOfCode2019.Tools;

public class QuickCircuit
{
    public enum GateType
    {
        NOT,
        SAME,
        AND,
        OR,
        XOR,
        LSHIFT,
        RSHIFT,
    }

    public enum DiagramType
    {
        Boolean,
        Analog,
    }

    public sealed class Wire(string name, DiagramType type, object? value)
    {
        public string Name { get; set; } = name;
        public DiagramType Type { get; set; } = type;
        public object? Value { get; set; } = value;
        public List<Gate> Connections { get; set; } = [];

        public override string ToString()
        {
            return $"{Name} = {(Value == null ? "NULL" : Value.ToString())} ({Type})";
        }
    }

    public sealed class Gate
    {
        public int Id { get; init; }
        public GateType Type { get; init; }
        public Wire Input1 { get; set; }
        public Wire? Input2 { get; set; } = null;
        public Wire Output { get; set; }
        public int ShiftSize { get; set; } = 0;

        public Gate(int id, GateType type, Wire input1, Wire input2, Wire output)
        {
            if (type is not (GateType.AND or GateType.OR or GateType.XOR))
            {
                throw new ArgumentException("Only AND, OR and XOR gate can use this constructor");
            }
            Id = id;
            Type = type;
            Input1 = input1;
            Input2 = input2;
            Output = output;
        }

        public Gate(int id, GateType type, Wire input1, Wire output)
        {
            if (type is not (GateType.NOT or GateType.SAME))
            {
                throw new ArgumentException("Only NOT and SAME gate can use this constructor");
            }
            Id = id;
            Type = type;
            Input1 = input1;
            Output = output;
        }

        public Gate(int id, GateType type, Wire input1, Wire output, int shiftSize)
        {
            if (type is not (GateType.LSHIFT or GateType.RSHIFT))
            {
                throw new ArgumentException("Only LSHIFT and RSHIFT gate can use this constructor");
            }
            Id = id;
            Type = type;
            Input1 = input1;
            Output = output;
            ShiftSize = shiftSize;
        }

        public void Compute()
        {
            // Check if calculation can be done
            if (Input1 == null || Input1.Value == null) return;
            if ((Input2 == null || Input2.Value == null) && Type is GateType.AND or GateType.OR or GateType.XOR) return;

            // Calculate
            if (Input1.Value is bool inputValue)
            {
                Output.Value = (Type) switch
                {
                    GateType.NOT => !inputValue,
                    GateType.SAME => inputValue,
                    GateType.AND => inputValue && (bool)Input2!.Value!,
                    GateType.OR => inputValue || (bool)Input2!.Value!,
                    GateType.XOR => inputValue ^ (bool)Input2!.Value!,
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                Output.Value = (ushort)((Type) switch
                {
                    GateType.NOT => ~(ushort)Input1.Value!,
                    GateType.SAME => (ushort)Input1.Value!,
                    GateType.AND => (ushort)Input1.Value! & (ushort)Input2!.Value!,
                    GateType.OR => (ushort)Input1.Value! | (ushort)Input2!.Value!,
                    GateType.XOR => (ushort)Input1.Value! ^ (ushort)Input2!.Value!,
                    GateType.LSHIFT => (ushort)Input1.Value! << ShiftSize,
                    GateType.RSHIFT => (ushort)Input1.Value! >> ShiftSize,
                    _ => throw new NotImplementedException(),
                });
            }
        }

        public override string ToString()
        {
            return (Type) switch
            {
                GateType.NOT or GateType.SAME => $"Gate{Id}: {Type} {Input1.Name}({Input1.Value}) -> {Output.Name}({Output.Value})",
                GateType.LSHIFT or GateType.RSHIFT => $"Gate{Id}: {Input1.Name}({Input1.Value}) {Type} {ShiftSize} -> {Output.Name}({Output.Value})",
                _ => $"Gate{Id}: {Input1.Name}({Input1.Value}) {Type} {Input2?.Name}({Input2?.Value}) -> {Output.Name}({Output.Value})",
            };
        }
    }

    public static (Dictionary<string, Wire> allWire, List<Gate> allGates) CreateDiagram(List<string> diagramInfo, DiagramType type)
    {
        // Extract all wires
        Dictionary<string, Wire> allWire = [];
        List<(GateType type, string input1, string input2, string output)> gateInfos = [];
        foreach (string data in diagramInfo)
        {
            if (data.Contains(':'))
            {
                string[] parts = [.. data.Split(':').ToList().ConvertAll(x => x.Trim())];
                allWire.TryAdd(parts[1], new Wire(parts[1], type, ValueFromString(parts[0], type)));
            }
            if (data.Contains("->"))
            {
                // Split
                string leftPart = data.Split("->")[0].Trim();
                string rightPart = data.Split("->")[1].Trim();
                string[] leftPartData = leftPart.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                // Manage special wire
                if (leftPartData.Length == 1)
                {
                    if (Microsoft.VisualBasic.Information.IsNumeric(leftPartData[0]))
                    {
                        if (!allWire.TryAdd(rightPart, new Wire(rightPart, type, ValueFromString(leftPartData[0], type))))
                        {
                            allWire[rightPart].Value = ValueFromString(leftPartData[0], type);
                        }
                    }
                    else
                    {
                        _ = allWire.TryAdd(leftPartData[0], new Wire(leftPartData[0], type, ValueFromString(leftPartData[0], type)));
                        _ = allWire.TryAdd(rightPart, new Wire(rightPart, type, ValueFromString(rightPart, type)));
                        gateInfos.Add((GateType.SAME, leftPartData[0], "", rightPart));
                    }
                    continue;
                }

                // Standard gate
                GateType gateType = GateType.SAME;
                if (leftPartData.Length >= 1)
                {
                    gateType = (GateType)Enum.Parse(typeof(GateType), leftPartData.Length == 2 ? leftPartData[0] : leftPartData[1]);
                }

                // Treat all
                switch (gateType)
                {
                    case GateType.NOT:
                        gateInfos.Add((gateType, leftPartData[1], "", rightPart));
                        _ = allWire.TryAdd(leftPartData[1], new Wire(leftPartData[1], type, ValueFromString(leftPartData[1], type)));
                        _ = allWire.TryAdd(rightPart, new Wire(rightPart, type, ValueFromString(rightPart, type)));
                        break;

                    case GateType.SAME:
                        gateInfos.Add((gateType, leftPartData[0], "", rightPart));
                        _ = allWire.TryAdd(leftPartData[0], new Wire(leftPartData[0], type, ValueFromString(leftPartData[0], type)));
                        _ = allWire.TryAdd(rightPart, new Wire(rightPart, type, ValueFromString(rightPart, type)));
                        break;

                    case GateType.AND:
                    case GateType.OR:
                    case GateType.XOR:
                        gateInfos.Add((gateType, leftPartData[0], leftPartData[2], rightPart));
                        _ = allWire.TryAdd(leftPartData[0], new Wire(leftPartData[0], type, ValueFromString(leftPartData[0], type)));
                        _ = allWire.TryAdd(leftPartData[2], new Wire(leftPartData[2], type, ValueFromString(leftPartData[1], type)));
                        _ = allWire.TryAdd(rightPart, new Wire(rightPart, type, ValueFromString(rightPart, type)));
                        break;

                    case GateType.LSHIFT:
                    case GateType.RSHIFT:
                        gateInfos.Add((gateType, leftPartData[0], leftPartData[2], rightPart));
                        _ = allWire.TryAdd(leftPartData[0], new Wire(leftPartData[0], type, ValueFromString(leftPartData[0], type)));
                        _ = allWire.TryAdd(rightPart, new Wire(rightPart, type, ValueFromString(rightPart, type)));
                        break;

                    default:
                        break;
                }
            }
        }

        // Create all gates
        List<Gate> allGates = [];
        foreach (var info in gateInfos)
        {
            allGates.Add((info.type) switch
            {
                GateType.NOT or GateType.SAME => new Gate(allGates.Count, info.type, allWire[info.input1], allWire[info.output]),
                GateType.LSHIFT or GateType.RSHIFT => new Gate(allGates.Count, info.type, allWire[info.input1], allWire[info.output], int.Parse(info.input2)),
                _ => new Gate(allGates.Count, info.type, allWire[info.input1], allWire[info.input2], allWire[info.output]),
            });
        }

        // Done
        return (allWire, allGates);
    }

    private static object? ValueFromString(string value, DiagramType type)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        return (type) switch
        {
            DiagramType.Boolean => value switch
            {
                "0" => false,
                "1" => true,
                _ => null,
            },
            DiagramType.Analog => ushort.TryParse(value, out ushort result) ? result : null,
            _ => throw new NotImplementedException(),
        };
    }
}