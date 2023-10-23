namespace GeneticSimulator;

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