using System.Collections.Generic;

public class MapLayer
{
    public List<Node> Nodes { get; private set; }
    public int LevelIndex { get; private set; }

    public MapLayer(int index)
    {
        Nodes = new List<Node>();
        LevelIndex = index;
    }

    public void AddNode(Node node)
    {
        Nodes.Add(node);
    }
}