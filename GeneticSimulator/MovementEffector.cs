namespace GeneticSimulator;

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