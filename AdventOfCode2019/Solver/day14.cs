namespace AdventOfCode2019.Solver;

internal partial class Day14 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Space Stoichiometry";

    private sealed class Element(string name, int quantity)
    {
        public string Name { get; init; } = name;
        public int Quantity { get; set; } = quantity;
    }

    private sealed class Nanofactory
    {
        public List<Element> Inputs { get; init; }
        public Element Output { get; init; }

        public Nanofactory(string reactionInfo)
        {
            List<string> inOut = reactionInfo.Split("=>").ToList().ConvertAll(t => t.Trim());
            if (inOut.Contains(","))
            {
                throw new InvalidDataException("Nanofactory can only have one output element");
            }
            Inputs = inOut[0].Split(",").ToList().ConvertAll(t => t.Trim()).ConvertAll(t => new Element(t.Split(" ")[1], int.Parse(t.Split(" ")[0])));
            Output = inOut[1].Split(",").ToList().ConvertAll(t => t.Trim()).ConvertAll(t => new Element(t.Split(" ")[1], int.Parse(t.Split(" ")[0])))[0];
        }
    }

    private readonly Dictionary<string, Nanofactory> _nanofactories = [];

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        return ComputeOre("FUEL", 1).ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();

        // Search quantity with binary search
        long maxOre = 1000000000000;
        long minFuel = 0;
        long maxFuel = 1000000000;
        do
        {
            long testFuelQuantity = (minFuel + maxFuel) / 2;
            long ore = ComputeOre("FUEL", testFuelQuantity);
            if (ore > maxOre)
            {
                maxFuel = testFuelQuantity;
            }
            else
            {
                minFuel = testFuelQuantity;
            }
        } while (maxFuel - minFuel > 1);
        return ((minFuel + maxFuel) / 2).ToString();
    }

    private long ComputeOre(string elementName, long elementQuantity)
    {
        // Initialization
        long totalOre = 0;
        Dictionary<string, long> stock = [];
        Queue<(string, long)> toCalculate = new();
        toCalculate.Enqueue((elementName, elementQuantity));

        // Using BFS, compute amount of ORE for required element quantity
        while (toCalculate.Count > 0)
        {
            (string element, long quantity) = toCalculate.Dequeue();
            Nanofactory factory = _nanofactories[element];

            // Compute how many time the factory must be run to produce what we need
            long nbrRun = ComputeNbrOfRunAndQuantityWithStockUpdate(ref stock, factory.Output.Name, factory.Output.Quantity, quantity).NbrOfRun;
            if (nbrRun == 0)
            {
                continue;
            }

            // Update quantity of ORE if needed
            totalOre += factory.Inputs[0].Name == "ORE" ? factory.Inputs[0].Quantity * nbrRun : 0;

            // Treat each source element
            foreach (Element sourceElement in factory.Inputs.FindAll(e => e.Name != "ORE"))
            {
                long elementCorrectedQuantity = ComputeNbrOfRunAndQuantityWithStockUpdate(ref stock,
                    sourceElement.Name, _nanofactories[sourceElement.Name].Output.Quantity, sourceElement.Quantity * nbrRun).Quantity;
                if (elementCorrectedQuantity == 0)
                {
                    continue;
                }

                toCalculate.Enqueue((sourceElement.Name, elementCorrectedQuantity));
            }
        }

        // Done
        return totalOre;
    }

    private static (long NbrOfRun, long Quantity) ComputeNbrOfRunAndQuantityWithStockUpdate(ref Dictionary<string, long> stock, string elementName, int elementFactoryQuantity, long quantityNeeded)
    {
        // Compute number of run to produce the required quantity
        long nbrOfRun = (long)Math.Ceiling((double)quantityNeeded / (double)elementFactoryQuantity);

        // Check if we can reduce this quantity based on what we have in stock
        long nbrOfRunStockEquivalency = stock.TryGetValue(elementName, out long value) ? value / elementFactoryQuantity : 0;
        if (nbrOfRunStockEquivalency > 0)
        {
            nbrOfRun -= nbrOfRunStockEquivalency;
            stock[elementName] -= nbrOfRunStockEquivalency * elementFactoryQuantity;
            if (nbrOfRun < 0)
            {
                throw new InvalidDataException();
            }
        }

        // Save in stock extra quantity that will be produced (we anticipate the run)
        long extraProduction = ((nbrOfRun + nbrOfRunStockEquivalency) * elementFactoryQuantity) - quantityNeeded;
        if (extraProduction > 0 && !stock.TryAdd(elementName, extraProduction))
        {
            stock[elementName] += extraProduction;
        }

        // Done
        return (nbrOfRun, nbrOfRun * elementFactoryQuantity);
    }

    private void ExtractData()
    {
        _nanofactories.Clear();
        foreach (string line in _puzzleInput)
        {
            Nanofactory nanofactory = new(line);
            _nanofactories[nanofactory.Output.Name] = nanofactory;
        }
    }
}