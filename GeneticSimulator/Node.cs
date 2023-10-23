namespace GeneticSimulator;

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