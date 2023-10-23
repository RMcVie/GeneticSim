using GeneticSimulator;

namespace Tests;

public sealed class EffectorNodeStub : EffectorNode
{
    public float Input { get; private set; }

    public EffectorNodeStub()
    {
        Effect = _ => { Input = _receivedInputs.Sum(x=>x.Value); };
    }
}