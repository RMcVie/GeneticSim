namespace GeneticSimulator;

public interface INodeWithOutput
{
    float GetOutput();
    Dictionary<Node, float> NextLinkedNodes { get; set; }
    bool HasReceivedAllInputs();
}