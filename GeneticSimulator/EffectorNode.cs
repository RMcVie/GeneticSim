namespace GeneticSimulator;

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