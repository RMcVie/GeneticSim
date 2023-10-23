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

public class BeepMetadata
{
    public int XPos { get; set; }
    public int YPos { get; set; }
}

public interface INodeWithOutput
{
    float GetOutput();
    Dictionary<Node, float> NextLinkedNodes { get; set; }
    bool HasReceivedAllInputs();
}

public abstract class Node
{
    public Dictionary<Node, float> PreviousLinkedNodes { get; set; } = new();
    public Dictionary<Node, float> NextLinkedNodes { get; set; } = new();
    protected Dictionary<Node, float> _receivedInputs = new();
    
    public void InputValue(Node from, float value)
    {
        _receivedInputs[from] = value;
    }
    
    public bool HasReceivedAllInputs() => _receivedInputs.Count == PreviousLinkedNodes.Count;

    protected internal virtual void Reset()
    {
        _receivedInputs = new();
    }
}

public class InternalNode : Node, INodeWithOutput
{
    public float GetOutput()
    {
        return MathF.Tanh(_receivedInputs.Sum(x => x.Value));
    }
}

public abstract class SensorNode : Node, INodeWithOutput
{
    internal readonly World World;
    private float? _value;

    protected SensorNode(World world)
    {
        World = world;
    }

    protected internal override void Reset()
    {
        _value = null;
        base.Reset();
    }

    protected abstract Func<Beep, float> ReadSensorImplementation();

    public void ReadSensor(Beep beep)
    {
        _value = ReadSensorImplementation().Invoke(beep);
    }

    public float GetOutput()
    {
        return _value ?? throw new Exception("Trying to read sensor value, but sensor has not been invoked yet.");
    }
}

public abstract class EffectorNode : Node
{
    public virtual Action<Beep> Effect { get; set; }

    internal float GetValue()
    {
        if (!HasReceivedAllInputs())
            throw new Exception("Trying to get effector value without having all inputs");
        return _receivedInputs.Sum(x => x.Value);
    }
}

public class XPositionSensor : SensorNode
{
    public XPositionSensor(World world) : base(world)
    {
    }
    
    protected override Func<Beep, float> ReadSensorImplementation()
    {
        return beep => World.GetBeepPosition(beep).x / (float)World.Grid.GetLength(0);
    }
}

public class MovementEffector: EffectorNode
{
    public MovementEffector(World world)
    {
        Effect = beep =>
        {
            if (GetValue() > -0.5 && GetValue() < 0.5)
                return;
            var beepPosition = world.GetBeepPosition(beep);
            world.TryMoveBeep(beep, beepPosition.x + 1, beepPosition.y);
        };
    }

    public sealed override Action<Beep> Effect { get; set; }
}

public class Gene
{
    public required Node Source { get; set; }
    public required Node Target { get; set; }
    public required float Weight { get; set; }
    private static readonly Random Random = new();

    public static Gene RandomGene(Node[] nodes)
    {
        var gene = new Gene
        {
            Source = nodes.Where(n => n is not EffectorNode).ToArray()[Random.Next(nodes.Count(n => n is not EffectorNode))],
            Target = nodes.Where(n => n is not SensorNode).ToArray()[Random.Next(nodes.Count(n => n is not SensorNode))],
            Weight = Random.NextSingle() * 2 - 1
        };
        return gene;
    }

    public static Gene Mutate(Gene gene, Node[] nodes)
    {
        var newGene = new Gene{Source = gene.Source, Target = gene.Target, Weight = gene.Weight};
        if (Random.NextDouble() < 0.1)
            newGene.Weight = gene.Weight + (float)Random.NextDouble() - 0.5f;
        if (Random.NextDouble() < 0.1)
            newGene.Source = nodes.Where(n => n is not EffectorNode).ToArray()[Random.Next(nodes.Count(n => n is not EffectorNode))];
        if (Random.NextDouble() < 0.1)
            newGene.Target = nodes.Where(n => n is not SensorNode).ToArray()[Random.Next(nodes.Count(n => n is not SensorNode))];
        return newGene;
    }
}

public class Beep
{
    public readonly Gene[] Genes;
    private readonly HashSet<Node> _nodes = new();
    private readonly HashSet<EffectorNode> _effectorNodes = new();
    private readonly HashSet<SensorNode> _sensorNodes = new();

    public Beep(Gene[] genes)
    {
        Genes = genes;
        //Use genes to set up links and weights between nodes
        foreach (var gene in genes)
        {
            gene.Source.NextLinkedNodes.Add(gene.Target, gene.Weight);
            gene.Target.PreviousLinkedNodes.Add(gene.Source, gene.Weight);
        }

        foreach (var gene in genes)
        {
            _nodes.Add(gene.Source);
            _nodes.Add(gene.Target);
        }

        foreach (var node in _nodes)
        {
            if (node is EffectorNode effectorNode)
                _effectorNodes.Add(effectorNode);
            else if (node is SensorNode sensorNode)
                _sensorNodes.Add(sensorNode);
        }
        
    }

    public void Simulate()
    {
        /*  Let the genes decide the order.
         Order all genes, so that Source = Sensor are first and Target=Effector are last.
         Every inner node -> inner node will remain in the order of the gene.
        */
        //reset all values
        foreach (var node in _nodes)
            node.Reset();
        PropagateValues(_sensorNodes);
        foreach (var effectorNode in _effectorNodes)
        {
            effectorNode.Effect.Invoke(this);
        }
    }

    private void PropagateValues(HashSet<SensorNode> startingNodes)
    {
        // Read all sensor values
        foreach (var node in startingNodes)
        {
            node.ReadSensor(this);
        }

        // Create a queue for BFS and enqueue starting nodes
        var queue = new Queue<INodeWithOutput>(startingNodes);
        
        while (queue.Count > 0)
        {
            if (_effectorNodes.All(e => e.HasReceivedAllInputs()))
                return;
            var currentNode = queue.Dequeue();

            foreach (var nextNode in currentNode.NextLinkedNodes.Keys)
            {
                if (currentNode is SensorNode sensor)
                {
                    nextNode.InputValue(sensor, sensor.GetOutput() * currentNode.NextLinkedNodes[nextNode]);
                    if (nextNode is INodeWithOutput nodeWithOutput)
                        queue.Enqueue(nodeWithOutput);
                    continue;
                }
                if (nextNode is EffectorNode)
                {
                    //Only output to effector node if all current inputs are received. 
                    if (currentNode.HasReceivedAllInputs())
                        nextNode.InputValue((Node)currentNode, currentNode.GetOutput() * currentNode.NextLinkedNodes[nextNode]);
                    else
                        queue.Enqueue(currentNode);
                    continue;
                }

                if (currentNode is InternalNode c && nextNode is InternalNode n)
                {
                    nextNode.InputValue(c, c.GetOutput() * currentNode.NextLinkedNodes[nextNode]);
                    queue.Enqueue(n);
                }
                else
                    throw new Exception("Invalid Gene Setup");
            }
        }
    }
}