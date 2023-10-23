using FluentAssertions;
using GeneticSimulator;

namespace Tests;

public class BeepNetworkSimulatorTests
{
    [Fact]
    public void Very_Basic_Feed_Forward_Test()
    {
        var sensor = new SampleSensor(0.7f);
        var internalNode = new InternalNode();
        var effectorStub = new EffectorNodeStub();
        var genes = new Gene[]
        {
            new () { Source = sensor, Target = internalNode, Weight = 0.3f },
            new () { Source = internalNode, Target = effectorStub, Weight = 0.5f }
        };

        var beep = new Beep(genes);
        
        beep.Simulate();
        //tanh(0.7 * 0.3) * 0.5
        Assert.Equal(0.10348325222730637, effectorStub.Input);
        beep.Simulate();
        Assert.Equal(0.10348325222730637, effectorStub.Input);
    }


    [Fact]
    public void Linear_Feed_Forward_Test()
    {
        var sensor = new SampleSensor(0.7f);
        var internalNodes = Enumerable.Range(0, 3).Select(_=> new InternalNode()).ToArray();
        var effectorMock = new EffectorNodeStub();
        
        var genes = new Gene[]
        {
            new () { Source = sensor, Target = internalNodes[0], Weight = 0.3f },
            new () { Source = internalNodes[0], Target = internalNodes[1], Weight = 0.3f },
            new () { Source = internalNodes[1], Target = internalNodes[2], Weight = 0.3f },
            new () { Source = internalNodes[2], Target = effectorMock, Weight = 0.5f }
        };

        var beep = new Beep(genes);
        beep.Simulate();
        effectorMock.Input.Should().Be(0.00930047f);
        beep.Simulate();
        effectorMock.Input.Should().Be(0.00930047f);
    }

    [Fact]
    public void Nodes_Wait_For_All_Inputs_Before_Propagating_To_Effector()
    {
        var s1 = new SampleSensor(0.1f);
        var s2 = new SampleSensor(0.2f);
        var s3 = new SampleSensor(0.3f);
        var internalNodes = Enumerable.Range(0, 3).Select(_ => new InternalNode()).ToArray();
        var e1 = new EffectorNodeStub();
        var e2 = new EffectorNodeStub();

        var genes = new Gene[]
        {
            new() { Source = s1, Target = internalNodes[0], Weight = 0.3f },
            new() { Source = s1, Target = internalNodes[1], Weight = 0.2f },
            new() { Source = s2, Target = internalNodes[1], Weight = 0.7f },
            new() { Source = s3, Target = internalNodes[2], Weight = 0.6f },
            new() { Source = internalNodes[1], Target = internalNodes[0], Weight = 0.9f },
            new() { Source = internalNodes[2], Target = internalNodes[1], Weight = 2.5f },
            new() { Source = internalNodes[0], Target = e1, Weight = 1.5f },
            new() { Source = internalNodes[1], Target = e1, Weight = 1.3f },
            new() { Source = internalNodes[2], Target = e2, Weight = 1.2f },
            new() { Source = s3, Target = e2, Weight = 1.3f },
        };

        var beep = new Beep(genes);
        beep.Simulate();

        var n3Input = 0.6f * 0.3f;
        var n3Output = MathF.Tanh(n3Input);
        var n2Input = n3Output * 2.5f + 0.2f * 0.7f + 0.2f * 0.1f;
        var n2Output = MathF.Tanh(n2Input);
        var n1Input = 0.3f * 0.1f + 0.9f * n2Output;
        var n1Output = MathF.Tanh(n1Input);
        
        e1.Input.Should().Be(1.5f * n1Output + 1.3f * n2Output);
        e2.Input.Should().Be(1.2f * n3Output + 1.3f * 0.3f);
        
        beep.Simulate();
        
        e1.Input.Should().Be(1.5f * n1Output + 1.3f * n2Output);
        e2.Input.Should().Be(1.2f * n3Output + 1.3f * 0.3f);
    }

    [Fact]
    public void Self_Referencing_Nodes_Are_Handled_Correctly()
    {
        var s = new SampleSensor(0.5f);
        var n = new InternalNode();
        var e = new EffectorNodeStub();

        var genes = new[]
        {
            new Gene { Source = s, Target = n, Weight = 0.7f },
            new Gene { Source = n, Target = n, Weight = 0.5f },
            new Gene { Source = n, Target = e, Weight = 0.3f },
        };

        var beep = new Beep(genes);
        beep.Simulate();
        e.Input.Should().Be(0.14289004F);
        beep.Simulate();
        e.Input.Should().Be(0.14289004F);
    }

    [Fact]
    public void Loops_Are_Handled()
    {
        var s = new SampleSensor(0.5f);
        var n1 = new InternalNode();
        var n2 = new InternalNode();
        var e = new EffectorNodeStub();

        var genes = new[]
        {
            new Gene { Source = s, Target = n1, Weight = 0.7f },
            new Gene { Source = n1, Target = n2, Weight = 0.6f },
            new Gene { Source = n2, Target = n1, Weight = 0.4f },
            new Gene { Source = n1, Target = e, Weight = 0.3f },
        };

        var beep = new Beep(genes);
        beep.Simulate();
    }
}