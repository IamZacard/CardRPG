using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "Map/MapConfig")]
public class MapConfig : ScriptableObject
{
    [System.Serializable]
    public struct NodeProbability
    {
        public NodeType nodeType;
        [Range(0f, 1f)]
        public float probability;
    }

    [Header("Layer Configuration")]
    public int minLayers = 15;
    public int maxLayers = 20;
    public int minNodesPerLayer = 2;
    public int maxNodesPerLayer = 4;

    [Header("Node Distribution")]
    public List<NodeProbability> randomNodeProbabilities = new List<NodeProbability>
    {
        new NodeProbability { nodeType = NodeType.MinorEnemy, probability = 0.4f },
        new NodeProbability { nodeType = NodeType.EliteEnemy, probability = 0.2f },
        new NodeProbability { nodeType = NodeType.RestSite, probability = 0.15f },
        new NodeProbability { nodeType = NodeType.Store, probability = 0.1f },
        new NodeProbability { nodeType = NodeType.Treasure, probability = 0.1f },
        new NodeProbability { nodeType = NodeType.Mystery, probability = 0.05f }
    };

    [Header("Connections")]
    public int extraConnectionsPerNode = 1;

    [Header("Node Templates")]
    public List<NodeBlueprint> nodeBlueprints;

    public NodeBlueprint GetBlueprint(NodeType type)
    {
        return nodeBlueprints.Find(b => b.nodeType == type);
    }
}