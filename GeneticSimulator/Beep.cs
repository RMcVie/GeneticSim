namespace GeneticSimulator;

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