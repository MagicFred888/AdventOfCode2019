using System.Diagnostics;
using System.Text;

namespace AdventOfCode2019.Tools;

public static class SmallTools
{
    /// <summary>
    /// Generates all permutations of a given number of items.
    /// Example: For totalItems = 3, it generates [0,1,2], [0,2,1], [1,0,2], [1,2,0], [2,0,1], [2,1,0].
    /// </summary>
    /// <param name="totalItems">The total number of items to permute.</param>
    /// <returns>A list of all permutations.</returns>
    public static List<List<int>> GeneratePermutations(int totalItems)
    {
        var result = new List<List<int>>();
        GeneratePermutationsRecursive(0, totalItems, [], result, new bool[totalItems]);
        return result;
    }

    private static void GeneratePermutationsRecursive(int depth, int totalItems, List<int> currentPermutation, List<List<int>> result, bool[] used)
    {
        if (depth == totalItems)
        {
            result.Add(new List<int>(currentPermutation));
            return;
        }

        for (int i = 0; i < totalItems; i++)
        {
            if (!used[i])
            {
                used[i] = true;
                currentPermutation.Add(i);
                GeneratePermutationsRecursive(depth + 1, totalItems, currentPermutation, result, used);
                currentPermutation.RemoveAt(currentPermutation.Count - 1);
                used[i] = false;
            }
        }
    }

    /// <summary>
    /// Generates all combinations of a given number of items taken a specified number at a time.
    /// Example: For totalItems = 4 and itemsNeeded = 2, it generates [0,1], [0,2], [0,3], [1,2], [1,3], [2,3].
    /// </summary>
    /// <param name="totalItems">The total number of items to choose from.</param>
    /// <param name="itemsNeeded">The number of items to choose in each combination.</param>
    /// <returns>A list of all combinations.</returns>
    public static List<List<int>> GenerateCombinations(int totalItems, int itemsNeeded)
    {
        var result = new List<List<int>>();
        GenerateCombinationsRecursive(0, totalItems, itemsNeeded, [], result);
        return result;
    }

    private static void GenerateCombinationsRecursive(int start, int totalItems, int itemsNeeded, List<int> currentCombination, List<List<int>> result)
    {
        if (currentCombination.Count == itemsNeeded)
        {
            result.Add(new List<int>(currentCombination));
            return;
        }

        for (int i = start; i < totalItems; i++)
        {
            currentCombination.Add(i);
            GenerateCombinationsRecursive(i + 1, totalItems, itemsNeeded, currentCombination, result);
            currentCombination.RemoveAt(currentCombination.Count - 1);
        }
    }

    /// <summary>
    /// Generates all combinations of numbers that sum up to a target value.
    /// Example: For numbers = [2,3,6,7] and targetValue = 7, it generates [7], [2,2,3].
    /// </summary>
    /// <param name="numbers">The list of numbers to choose from.</param>
    /// <param name="targetValue">The target sum value.</param>
    /// <returns>A list of all combinations that sum up to the target value.</returns>
    public static List<List<long>> GenerateCombinationsMatchingTotal(List<long> numbers, long targetValue)
    {
        List<List<long>> result = [];
        GenerateCombinations(numbers, targetValue, [], result, 0);
        return result;
    }

    private static void GenerateCombinations(List<long> numbers, long targetValue, List<long> currentCombination, List<List<long>> results, int startIndex)
    {
        if (targetValue == 0)
        {
            results.Add(new List<long>(currentCombination));
            return;
        }

        for (int i = startIndex; i < numbers.Count; i++)
        {
            if (numbers[i] <= targetValue)
            {
                currentCombination.Add(numbers[i]);
                GenerateCombinations(numbers, targetValue - numbers[i], currentCombination, results, i + 1);
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }
    }

    /// <summary>
    /// Calculates the Least Common Multiple (LCM) of an array of numbers.
    /// </summary>
    /// <param name="numbers">An array of long integers.</param>
    /// <returns>The LCM of the input numbers.</returns>
    public static long LCM(long[] numbers)
    {
        return numbers.Aggregate((long x, long y) => x * y / GCD(x, y));
    }

    /// <summary>
    /// Calculates the Greatest Common Divisor (GCD) of two numbers using the Euclidean algorithm.
    /// </summary>
    /// <param name="a">The first long integer.</param>
    /// <param name="b">The second long integer.</param>
    /// <returns>The GCD of the two input numbers.</returns>
    public static long GCD(long a, long b)
    {
        if (b == 0) return a;
        return GCD(b, a % b);
    }

    /// <summary>
    /// Determines if a given number is a prime number.
    /// </summary>
    /// <param name="number">The number to check for primality.</param>
    /// <returns>True if the number is prime, otherwise false.</returns>
    public static bool IsPrime(long number)
    {
        return PrimeDecomposition(number).Count == 1;
    }

    /// <summary>
    /// Decomposes a given integer into its prime factors.
    /// </summary>
    /// <param name="number">The integer to decompose.</param>
    /// <returns>A list of prime factors of the given integer.</returns>
    public static List<long> PrimeDecomposition(long number)
    {
        List<long> factors = [];
        long n = number;
        for (long i = 2; i * i <= n; i++)
        {
            while (n % i == 0)
            {
                factors.Add(i);
                n /= i;
            }
        }
        if (n > 1)
        {
            factors.Add(n);
        }
        return factors;
    }

    /// <summary>
    /// Calculates the sum of factors of a given number.
    /// Reference: https://en.wikipedia.org/wiki/Divisor_function
    /// </summary>
    /// <param name="number">The number to calculate the sum of factors for.</param>
    /// <returns>The sum of factors of the given number.</returns>
    public static long SumOfFactors(long number)
    {
        List<long> primeFactor = SmallTools.PrimeDecomposition(number);
        return primeFactor.Aggregate(1L, (acc, val) => acc * (long)((Math.Pow(val, 2) - 1) / (val - 1)));
    }

    /// <summary>
    /// Prints a 2D array of objects to the debug output, converting values using a dictionary if provided.
    /// </summary>
    /// <param name="tmpTable">The 2D array of objects to print.</param>
    /// <param name="convDic">A dictionary for converting object values to strings.</param>
    /// <param name="missing">The string to use if a value is missing in the dictionary.</param>
    public static void DebugPrint(object[,] tmpTable, Dictionary<string, string> convDic, string missing)
    {
        Debug.WriteLine("");
        for (long y = 0; y <= tmpTable.GetUpperBound(1); y++)
        {
            StringBuilder line = new();
            for (long x = 0; x <= tmpTable.GetUpperBound(0); x++)
            {
                string? tmpVal = tmpTable[x, y] == null ? "." : tmpTable[x, y].ToString();

                if (tmpVal == null)
                {
                    line.Append("NULL");
                }
                else if (convDic == null)
                {
                    line.Append(tmpVal);
                }
                else
                {
                    if (convDic.TryGetValue(tmpVal, out string? value))
                    {
                        line.Append(value);
                    }
                    else
                    {
                        line.Append(missing);
                    }
                }
            }
            Debug.WriteLine(line.ToString());
        }
    }

    /// <summary>
    /// Prints a 2D array of characters to the debug output.
    /// </summary>
    /// <param name="tmpTable">The 2D array of characters to print.</param>
    public static void DebugPrint(char[,] tmpTable)
    {
        Debug.WriteLine("");
        for (long y = 0; y <= tmpTable.GetUpperBound(1); y++)
        {
            StringBuilder line = new();
            for (long x = 0; x <= tmpTable.GetUpperBound(0); x++)
            {
                line.Append(tmpTable[x, y]);
            }
            Debug.WriteLine(line.ToString());
        }
    }
}