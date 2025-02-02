using System.Numerics;

namespace AdventOfCode2019.Solver;

internal partial class Day22 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Slam Shuffle";

    private enum DealTypes
    {
        NewStack,
        Cut,
        Increment,
    }

    private readonly List<(DealTypes type, int value)> _allShuffling = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        int nbrOfCards = isChallenge ? 10007 : 10;
        int[] deck = Enumerable.Range(0, nbrOfCards).ToArray();
        foreach ((DealTypes type, int value) in _allShuffling)
        {
            deck = type switch
            {
                DealTypes.NewStack => deck.Reverse().ToArray(),
                DealTypes.Cut => CutDeck(deck, value),
                DealTypes.Increment => IncrementDeck(deck, value),
                _ => throw new InvalidOperationException(),
            };
        }
        return isChallenge ? deck.ToList().IndexOf(2019).ToString() : string.Join("", deck);
    }

    public override string GetSolution2(bool isChallenge)
    {
        // Will have to admit this solution was not found by myself... Thanks to https://www.reddit.com/user/zedrdave/
        // for his Python solution posted on https://www.reddit.com/r/adventofcode/comments/ee0rqi/2019_day_22_solutions/
        // I just made the conversion to C#

        ExtractData();

        // Puzzle explanation
        BigInteger nbrOfCards = 119315717514047;
        BigInteger nbrOfShufflingCycles = 101741582076661;
        BigInteger positionOfCardOfInterestAfterShuffling = 2020;

        // Extra infos here : https://www.reddit.com/r/adventofcode/comments/ee0rqi/comment/fbnkaju/
        BigInteger offsetIncrement = 0;
        BigInteger incrementMultiplier = 1;

        // Run the loop shuffling
        foreach ((DealTypes type, int value) in _allShuffling)
        {
            switch (type)
            {
                case DealTypes.NewStack:
                    incrementMultiplier = (-incrementMultiplier) % nbrOfCards;
                    offsetIncrement = (nbrOfCards - 1 - offsetIncrement) % nbrOfCards;
                    break;

                case DealTypes.Cut:
                    offsetIncrement = (offsetIncrement - value) % nbrOfCards;
                    break;

                case DealTypes.Increment:
                    incrementMultiplier = (incrementMultiplier * value) % nbrOfCards;
                    offsetIncrement = (offsetIncrement * value) % nbrOfCards;
                    break;

                default:
                    break;
            }
        }

        // Calculate result using mathematics beyond my knowledge...
        BigInteger modInverse = BigInteger.ModPow(1 - incrementMultiplier, nbrOfCards - 2, nbrOfCards);
        BigInteger r = (offsetIncrement * modInverse) % nbrOfCards;
        BigInteger result = ((positionOfCardOfInterestAfterShuffling - r) * BigInteger.ModPow(incrementMultiplier, nbrOfShufflingCycles * (nbrOfCards - 2), nbrOfCards) + r) % nbrOfCards;

        // Done
        return result.ToString();
    }

    private static int[] CutDeck(int[] deck, int value)
    {
        int[] newDeck = Enumerable.Repeat(-1, deck.Length).ToArray();
        if (value > 0)
        {
            Array.Copy(deck, 0, newDeck, deck.Length - value, value);
            Array.Copy(deck, value, newDeck, 0, deck.Length - value);
        }
        else
        {
            Array.Copy(deck, deck.Length + value, newDeck, 0, -value);
            Array.Copy(deck, 0, newDeck, -value, deck.Length + value);
        }
        return newDeck;
    }

    private static int[] IncrementDeck(int[] deck, int value)
    {
        int[] newDeck = Enumerable.Repeat(-1, deck.Length).ToArray();
        for (int i = 0; i < deck.Length; i++)
        {
            newDeck[i * value % deck.Length] = deck[i];
        }
        return newDeck;
    }

    private void ExtractData()
    {
        _allShuffling.Clear();
        foreach (string line in _puzzleInput)
        {
            _allShuffling.Add(line switch
            {
                string l when l.StartsWith("deal into") => (DealTypes.NewStack, 0),
                string l when l.StartsWith("cut") => (DealTypes.Cut, int.Parse(l.Split(' ')[^1])),
                string l when l.StartsWith("deal with") => (DealTypes.Increment, int.Parse(l.Split(' ')[^1])),
                _ => throw new InvalidOperationException()
            });
        }
    }
}