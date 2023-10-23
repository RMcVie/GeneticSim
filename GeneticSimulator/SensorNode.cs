namespace GeneticSimulator;

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