using AdventOfCode2019.Solver;

namespace AdventOfCode2019;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Check if we are still under development by checking if DayXX still exists
        string dayUnderDeveloppment = string.Empty;

        bool fullySolved = !AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Any(p => typeof(BaseSolver).IsAssignableFrom(p) && !p.IsAbstract && p.Name == "DayXX");

        // Pre-select day to latest with a solver
        if (!fullySolved)
        {
            // Get all solver name who derivate BaseSolver class
            Type solverToLoad = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(BaseSolver).IsAssignableFrom(p) && !p.IsAbstract && p.Name != "DayXX")
                .OrderByDescending(p => p.Name)
                .First();
            dayUnderDeveloppment = solverToLoad?.Name.Replace("Day", "") ?? "";
        }

        // Main loop
        do
        {
            // For fun
            // Thanks to https://patorjk.com/software/taag/#p=display&v=3&f=Big&t=Type%20Something%20
            Console.Clear();
            Console.WriteLine(@"                            _                 _    ____   __  _____          _      ___   ___  __  ___   ");
            Console.WriteLine(@"                   /\      | |               | |  / __ \ / _|/ ____|        | |    |__ \ / _ \/_ |/ _ \  ");
            Console.WriteLine(@"                  /  \   __| |_   _____ _ __ | |_| |  | | |_| |     ___   __| | ___   ) | | | || | (_) | ");
            Console.WriteLine(@"                 / /\ \ / _` \ \ / / _ \ '_ \| __| |  | |  _| |    / _ \ / _` |/ _ \ / /| | | || |\__, | ");
            Console.WriteLine(@"                / ____ \ (_| |\ V /  __/ | | | |_| |__| | | | |___| (_) | (_| |  __// /_| |_| || |  / /  ");
            Console.WriteLine(@"               /_/    \_\__,_| \_/ \___|_| |_|\__|\____/|_|  \_____\___/ \__,_|\___|____|\___/ |_| /_/   ");
            Console.WriteLine("");

            // Item to manage automatic solver selection
            string toSolveInfo = "";
            List<BaseSolver> allSolvers;

            // Loading loop
            do
            {
                // Fix solver if still under development
                if (string.IsNullOrEmpty(toSolveInfo) && !string.IsNullOrEmpty(dayUnderDeveloppment))
                {
                    toSolveInfo = dayUnderDeveloppment;
                }

                // Load solver
                allSolvers = [];
                if (!string.IsNullOrEmpty(toSolveInfo))
                {
                    if (int.TryParse(toSolveInfo, out int day))
                    {
                        // Load requested day
                        Type? solverToLoad = Type.GetType($"AdventOfCode2019.Solver.Day{day:00}");
                        if (solverToLoad != null)
                        {
                            object? newInstance = Activator.CreateInstance(solverToLoad);
                            if (newInstance != null)
                            {
                                allSolvers.Add((BaseSolver)newInstance);
                            }
                        }
                    }
                    else if (toSolveInfo.Equals("all", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Load all solvers
                        allSolvers = AppDomain.CurrentDomain.GetAssemblies()?
                            .SelectMany(s => s.GetTypes())
                            .Where(p => typeof(BaseSolver).IsAssignableFrom(p) && !p.IsAbstract && p.Name != "DayXX")
                            .Select(p => Activator.CreateInstance(p) as BaseSolver)
                            .Where(instance => instance != null)
                            .ToList()!;
                    }
                }

                // Ask user if no solver selected
                if (allSolvers.Count == 0)
                {
                    Console.Write("Please select day challenge you want: ");
                    string? input = Console.ReadLine();
                    toSolveInfo = input != null && (Microsoft.VisualBasic.Information.IsNumeric(input) || input.Equals("all", StringComparison.CurrentCultureIgnoreCase)) ? input : "";
                    if (string.IsNullOrEmpty(toSolveInfo))
                    {
                        return;
                    }
                }
            } while (allSolvers.Count == 0);

            // Solve all
            Console.WriteLine("");
            foreach (BaseSolver solver in allSolvers)
            {
                if (allSolvers.Count == 1)
                {
                    // Title
                    solver.PrintTitle();
                    bool status;

                    // Sample 1
                    if (solver.HaveSampleData(RoundId.FirstRound))
                    {
                        Console.WriteLine("Solving sample 1:");
                        status = solver.SolveSample(RoundId.FirstRound, out string[] answers);
                        Console.WriteLine($"--> {string.Join("\r\n--> ", answers)}");
                        if (!status) break;
                    }

                    // Challenge 1
                    if (solver.HaveChallengeData())
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Solving challenge 1:");
                        status = solver.SolveChallenge(RoundId.FirstRound, out string answer);
                        Console.WriteLine($"--> {answer}");
                        if (!status) break;
                    }
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Challenge data file not found !");
                    }

                    // Sample 2
                    if (solver.HaveSampleData(RoundId.SecondRound))
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Solving sample 2:");
                        status = solver.SolveSample(RoundId.SecondRound, out string[] answers);
                        Console.WriteLine($"--> {string.Join("\r\n--> ", answers)}");
                        if (!status) break;
                    }

                    // Challenge 2
                    if (solver.HaveChallengeData())
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Solving challenge 2:");
                        status = solver.SolveChallenge(RoundId.SecondRound, out string answer);
                        Console.WriteLine($"--> {answer}");
                        if (!status) break;
                    }
                }
                else
                {
                    // Print puzzle as single line
                    int longestPuzzleTitle = allSolvers.Max(s => s.PuzzleTitle.Length);
                    Console.WriteLine(solver.SolveFullDay(longestPuzzleTitle));
                }
            }

            // Wait to quit
            Console.WriteLine("");
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        } while (true);
    }
}