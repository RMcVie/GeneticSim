namespace GeneticSimulator;

public class World
{
    private readonly int _gridSize;
    private const int NumBeeps = 100;
    private const int NumGenes = 5;
    private const int NumNodes = 3;
    private readonly Beep[] _beeps;
    public readonly Beep?[,] Grid;
    public Dictionary<Beep, BeepMetadata> _beepMetadata = new();
    private int stepNumber;
    private int generationNumber;
    private int numSteps = 20;
    private readonly Random random = new Random();
    private readonly Node[] nodes;
    
    public World(int gridSize)
    {
        _gridSize = gridSize;
        Grid = new Beep[gridSize, gridSize];
        _beeps = new Beep[NumBeeps];
        for (var i = 0; i < NumBeeps; i++)
        {
            nodes = Enumerable.Range(0, NumNodes).Select(_ => (Node)new InternalNode()).Union(GetEffectors()).Union(GetSensors()).ToArray();
            var genes = Enumerable.Range(0, NumGenes).Select(_ => Gene.RandomGene(nodes)).ToArray();
            _beeps[i] = new Beep(genes);
            _beepMetadata[_beeps[i]] = new BeepMetadata();
        }

        InitializeGrid();
    }

    public void SimulateGenerations(int generations)
    {
        for (var i = 0; i < generations; i++)
        {
            generationNumber += i;
            SimulateSteps(numSteps + 1);
            CreateNewGeneration();
            InitializeGrid();
            stepNumber = 0;
        }
    }


    public void SimulateSteps(int steps)
    {
        if (stepNumber >= numSteps)
            return;
        for (int i = 0; i < steps; i++)
        {
            if (stepNumber >= numSteps)
                break;

            foreach (var beep in _beeps)
            {
                beep.Simulate();
                throw new Exception();
                // effector.Effect(this, beep);
            }
            stepNumber++;
        }
    }

    public (int x, int y) GetBeepPosition(Beep beep)
    {
        return (_beepMetadata[beep].XPos, _beepMetadata[beep].YPos);
    }

    public bool TryMoveBeep(Beep beep, int x, int y)
    {
        if (x > Grid.GetLength(0) - 1 || y > Grid.GetLength(1) - 1)
            return false;
        if (Grid[x, y] != null)
            return false;
        Grid[x, y] = beep;
        _beepMetadata[beep].XPos = x;
        _beepMetadata[beep].YPos = y;
        return true;
    }

    private void CreateNewGeneration()
    {
        var survivors = new List<Beep>();
        foreach (var (beep, data) in _beepMetadata)
        {
            if (data.XPos > Grid.GetLength(0) / 2)
            {
                survivors.Add(beep);
            }
        }

        var pairs = new List<(Beep, Beep)>();
        for (int i = 0; i < NumBeeps; i++)
        {
            int index1, index2;
            do
            {
                index1 = random.Next(survivors.Count);
                index2 = random.Next(survivors.Count);
            } while (index1 == index2);

            pairs.Add((survivors[index1], survivors[index2]));
        }

        for (int i = 0; i < NumBeeps; i++)
        {
            _beeps[i] = new Beep(_beeps[i].Genes.Select(gene => Gene.Mutate(gene, nodes)).ToArray());
        }

        _beepMetadata = new Dictionary<Beep, BeepMetadata>();
        foreach (var beep in _beeps)
        {
            _beepMetadata.Add(beep, new BeepMetadata()); }
    }

    private void InitializeGrid()
    {
        foreach (var beep in _beeps)
        {
            int x, y;
            do
            {
                x = random.Next(_gridSize);
                y = random.Next(_gridSize);
            } while (Grid[x, y] != null);

            Grid[x, y] = beep;
            _beepMetadata[beep].XPos = x;
            _beepMetadata[beep].YPos = y;
        }
    }
    
    private IEnumerable<EffectorNode> GetEffectors()
    {
        yield return new MovementEffector(this);
    }

    private IEnumerable<SensorNode> GetSensors()
    {
        yield return new XPositionSensor(this);
    }
}