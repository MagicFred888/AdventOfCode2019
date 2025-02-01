// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Blocker Code Smell", "S2368:Public methods should not have multidimensional array parameters", Justification = "This is a debug methode to display an array", Scope = "member", Target = "~M:AdventOfCode2019.Tools.SmallTools.DebugPrint(System.Object[,],System.Collections.Generic.Dictionary{System.String,System.String},System.String)")]
[assembly: SuppressMessage("Blocker Code Smell", "S2368:Public methods should not have multidimensional array parameters", Justification = "This is a debug methode to display an array", Scope = "member", Target = "~M:AdventOfCode2019.Tools.SmallTools.DebugPrint(System.Char[,])")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is a sample class that will be removed at the end of the project", Scope = "member", Target = "~M:AdventOfCode2019.Solver.DayXX.ExtractData")]
[assembly: SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>", Scope = "member", Target = "~M:AdventOfCode2019.Solver.Day17.IntcodeComputer.Clone~AdventOfCode2019.Solver.Day17.IntcodeComputer")]
