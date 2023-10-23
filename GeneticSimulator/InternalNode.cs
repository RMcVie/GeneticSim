namespace GeneticSimulator;

public class InternalNode : Node, INodeWithOutput
{
    public float GetOutput()
    {
        return MathF.Tanh(_receivedInputs.Sum(x => x.Value));
    }
}