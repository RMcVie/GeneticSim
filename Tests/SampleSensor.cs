using GeneticSimulator;

namespace Tests;

public class SampleSensor : SensorNode
{
    private readonly float _value;

    public SampleSensor(float value) : base(null!)
    {
        _value = value;
    }

    protected override Func<Beep, float> ReadSensorImplementation()
    {
        return _ => _value;
    }
}