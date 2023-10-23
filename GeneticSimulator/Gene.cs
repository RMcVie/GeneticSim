namespace GeneticSimulator;

public class Gene
{
    public required Node Source { get; set; }
    public required Node Target { get; set; }
    public required float Weight { get; set; }
    private static readonly Random Random = new();

    public static Gene RandomGene(Node[] nodes)
    {
        var gene = new Gene
        {
            Source = nodes.Where(n => n is not EffectorNode).ToArray()[Random.Next(nodes.Count(n => n is not EffectorNode))],
            Target = nodes.Where(n => n is not SensorNode).ToArray()[Random.Next(nodes.Count(n => n is not SensorNode))],
            Weight = Random.NextSingle() * 2 - 1
        };
        return gene;
    }

    public static Gene Mutate(Gene gene, Node[] nodes)
    {
        var newGene = new Gene{Source = gene.Source, Target = gene.Target, Weight = gene.Weight};
        if (Random.NextDouble() < 0.1)
            newGene.Weight = gene.Weight + (float)Random.NextDouble() - 0.5f;
        if (Random.NextDouble() < 0.1)
            newGene.Source = nodes.Where(n => n is not EffectorNode).ToArray()[Random.Next(nodes.Count(n => n is not EffectorNode))];
        if (Random.NextDouble() < 0.1)
            newGene.Target = nodes.Where(n => n is not SensorNode).ToArray()[Random.Next(nodes.Count(n => n is not SensorNode))];
        return newGene;
    }
}