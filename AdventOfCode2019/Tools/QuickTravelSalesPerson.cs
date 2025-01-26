namespace AdventOfCode2019.Tools;

public static class TravelingSalespersonProblem
{
    public static long FindMinDistance(List<(string from, string to, int distance)> allPair, bool travelBackToStart)
    {
        return FindMinDistance(GetDistanceTable(allPair), travelBackToStart);
    }

    public static long FindMaxDistance(List<(string from, string to, int distance)> allPair, bool travelBackToStart)
    {
        return FindMaxDistance(GetDistanceTable(allPair), travelBackToStart);
    }

    private static int[,] GetDistanceTable(List<(string from, string to, int distance)> allPair)
    {
        // Extract all from and to and keep only one instance of each to create node
        List<string> cities = [.. allPair.SelectMany(d => new[] { d.from, d.to }).Distinct().Order()];

        // Fill matrix based on pair received
        int[,] distanceTable = new int[cities.Count, cities.Count];

        // Fill all slot in matrix with long.MaxValue or 0 if i == j
        for (int i = 0; i < cities.Count; i++)
        {
            for (int j = 0; j < cities.Count; j++)
            {
                distanceTable[i, j] = i == j ? 0 : int.MaxValue;
            }
        }

        // Fill matrix with distance known from pair
        foreach ((string from, string to, int distance) in allPair)
        {
            int fromIndex = cities.IndexOf(from);
            int toIndex = cities.IndexOf(to);
            distanceTable[fromIndex, toIndex] = distance;
            distanceTable[toIndex, fromIndex] = distance;
        }

        return distanceTable;
    }

    private static int FindMinDistance(int[,] graph, bool travelBackToStart)
    {
        // Initialize DP table with infinity
        int n = graph.GetLength(0);
        int[,] dp = new int[1 << n, n];
        for (int i = 0; i < (1 << n); i++)
        {
            for (int j = 0; j < n; j++)
            {
                dp[i, j] = int.MaxValue;
            }
        }

        // Initialize all possible starting points
        for (int i = 0; i < n; i++)
        {
            dp[1 << i, i] = 0;
        }

        // Iterate over all subsets of nodes
        for (int mask = 1; mask < (1 << n); mask++)
        {
            for (int u = 0; u < n; u++)
            {
                if ((mask & (1 << u)) == 0) continue; // Skip if u is not in the subset

                for (int v = 0; v < n; v++)
                {
                    if ((mask & (1 << v)) != 0 || graph[u, v] == int.MaxValue) continue; // Skip if v is already in the subset or edge doesn't exist

                    int newMask = mask | (1 << v);
                    dp[newMask, v] = Math.Min(dp[newMask, v], dp[mask, u] + graph[u, v]);
                }
            }
        }

        // Calculate shortest path
        int minCost = int.MaxValue;
        if (travelBackToStart)
        {
            // Find the minimum cost to complete the cycle
            for (int u = 0; u < n; u++)
            {
                if (dp[(1 << n) - 1, u] < int.MaxValue)
                {
                    for (int v = 0; v < n; v++)
                    {
                        if (u != v && graph[u, v] != int.MaxValue)
                        {
                            minCost = Math.Min(minCost, dp[(1 << n) - 1, u] + graph[u, v]);
                        }
                    }
                }
            }
        }
        else
        {
            // Find the minimum cost to visit all nodes without returning to the start
            for (int u = 0; u < n; u++)
            {
                if (dp[(1 << n) - 1, u] < int.MaxValue)
                {
                    minCost = Math.Min(minCost, dp[(1 << n) - 1, u]);
                }
            }
        }

        return minCost == int.MaxValue ? -1 : minCost; // Return -1 if no valid tour exists
    }

    private static int FindMaxDistance(int[,] graph, bool travelBackToStart)
    {
        // Initialize DP table with int.MinValue
        int n = graph.GetLength(0);
        int[,] dp = new int[1 << n, n];
        for (int i = 0; i < (1 << n); i++)
        {
            for (int j = 0; j < n; j++)
            {
                dp[i, j] = int.MinValue;
            }
        }

        // Initialize all possible starting points
        for (int i = 0; i < n; i++)
        {
            dp[1 << i, i] = 0;
        }

        // Iterate over all subsets of nodes
        for (int mask = 1; mask < (1 << n); mask++)
        {
            for (int u = 0; u < n; u++)
            {
                if ((mask & (1 << u)) == 0) continue; // Skip if u is not in the subset

                for (int v = 0; v < n; v++)
                {
                    if ((mask & (1 << v)) != 0 || graph[u, v] == int.MinValue) continue; // Skip if v is already in the subset or edge doesn't exist

                    int newMask = mask | (1 << v);
                    dp[newMask, v] = Math.Max(dp[newMask, v], dp[mask, u] + graph[u, v]);
                }
            }
        }

        // Calculate longest path
        int maxCost = int.MinValue;
        if (travelBackToStart)
        {
            // Find the maximum cost to complete the cycle
            for (int u = 0; u < n; u++)
            {
                if (dp[(1 << n) - 1, u] > int.MinValue)
                {
                    for (int v = 0; v < n; v++)
                    {
                        if (u != v && graph[u, v] != int.MinValue)
                        {
                            maxCost = Math.Max(maxCost, dp[(1 << n) - 1, u] + graph[u, v]);
                        }
                    }
                }
            }
        }
        else
        {
            // Find the maximum cost to visit all nodes without returning to the start
            for (int u = 0; u < n; u++)
            {
                if (dp[(1 << n) - 1, u] > int.MinValue)
                {
                    maxCost = Math.Max(maxCost, dp[(1 << n) - 1, u]);
                }
            }
        }

        return maxCost == int.MinValue ? -1 : maxCost; // Return -1 if no valid tour exists
    }
}