using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [SerializeField] private MapConfig config;
    [SerializeField] private MapView view;
    private List<MapLayer> layers = new List<MapLayer>();
    private Node currentNode;
    public Dictionary<Vector2Int, NodeStates> nodeStates = new Dictionary<Vector2Int, NodeStates>();

    public Node CurrentNode => currentNode;
    public List<MapLayer> Layers => layers;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null)
            {
                Debug.LogWarning("MapManager is not a root GameObject. Moving to root to support DontDestroyOnLoad.");
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!System.IO.File.Exists(Application.persistentDataPath + "/autosave.dat"))
        {
            GenerateMap();
        }
        else
        {
            SaveLoadManager.Instance.LoadAutoSave();
        }
    }

    public void GenerateMap()
    {
        if (config == null)
        {
            Debug.LogError("MapManager: MapConfig is not assigned.");
            return;
        }

        if (view == null)
        {
            Debug.LogError("MapManager: MapView is not assigned.");
            return;
        }

        layers.Clear();
        nodeStates.Clear();
        MapPlayerTracker.Instance.ClearPath();

        int totalLayers = Random.Range(config.minLayers, config.maxLayers + 1);

        // Starting layer
        MapLayer startLayer = new MapLayer(0);
        Node startNode = new Node(NodeType.StartingNode, config.GetBlueprint(NodeType.StartingNode)?.name ?? "StartingNode", new Vector2Int(0, 0));
        startLayer.AddNode(startNode);
        layers.Add(startLayer);
        currentNode = startNode;
        nodeStates[startNode.point] = NodeStates.Attainable;

        // Intermediate layers
        for (int i = 1; i < totalLayers - 1; i++)
        {
            MapLayer layer = new MapLayer(i);
            int nodeCount = Random.Range(config.minNodesPerLayer, config.maxNodesPerLayer + 1);
            for (int j = 0; j < nodeCount; j++)
            {
                NodeType type = GetRandomNodeType();
                Node node = new Node(type, config.GetBlueprint(type)?.name ?? type.ToString(), new Vector2Int(j, i));
                layer.AddNode(node);
                nodeStates[node.point] = NodeStates.Locked;
            }
            layers.Add(layer);
        }

        // Boss layer
        MapLayer bossLayer = new MapLayer(totalLayers - 1);
        Node bossNode = new Node(NodeType.Boss, config.GetBlueprint(NodeType.Boss)?.name ?? "Boss", new Vector2Int(0, totalLayers - 1));
        bossLayer.AddNode(bossNode);
        layers.Add(bossLayer);
        nodeStates[bossNode.point] = NodeStates.Locked;

        // Connect nodes
        ConnectLayers();
        EnsurePathToBoss();

        view.GenerateMapVisual(layers);
        view.UpdateVisuals(nodeStates);
        SaveLoadManager.Instance.AutoSave();
    }

    public void SetCurrentNode(Node node)
    {
        currentNode = node;
        if (view != null)
            view.UpdateVisuals(nodeStates);
    }

    public void GenerateMapVisual()
    {
        if (view != null)
            view.GenerateMapVisual(layers);
    }

    private void ConnectLayers()
    {
        for (int i = 0; i < layers.Count - 1; i++)
        {
            var currentLayer = layers[i];
            var nextLayer = layers[i + 1];

            foreach (var fromNode in currentLayer.Nodes)
            {
                int connections = Random.Range(1, Mathf.Min(config.extraConnectionsPerNode + 1, nextLayer.Nodes.Count + 1));
                var shuffledNextNodes = nextLayer.Nodes.OrderBy(x => Random.value).Take(connections).ToList();
                foreach (var toNode in shuffledNextNodes)
                {
                    fromNode.AddOutgoing(toNode.point);
                    toNode.AddIncoming(fromNode.point);
                }
            }
        }
    }

    private void EnsurePathToBoss()
    {
        HashSet<Vector2Int> reachable = new HashSet<Vector2Int> { layers[layers.Count - 1].Nodes[0].point };
        for (int i = layers.Count - 2; i >= 0; i--)
        {
            foreach (var node in layers[i].Nodes)
            {
                bool hasPath = node.outgoing.Any(p => reachable.Contains(p));
                if (!hasPath && node.outgoing.Count < 3)
                {
                    var nextLayerNodes = layers[i + 1].Nodes.Where(n => reachable.Contains(n.point)).ToList();
                    if (nextLayerNodes.Count > 0)
                    {
                        var nextNode = nextLayerNodes[Random.Range(0, nextLayerNodes.Count)];
                        node.AddOutgoing(nextNode.point);
                        nextNode.AddIncoming(node.point);
                        reachable.Add(node.point);
                    }
                }
                else if (node.outgoing.Count > 0)
                {
                    reachable.Add(node.point);
                }
            }
        }
    }

    private NodeType GetRandomNodeType()
    {
        if (config == null || config.randomNodeProbabilities == null)
        {
            Debug.LogWarning("MapManager: MapConfig or RandomNodeProbabilities is null. Returning MinorEnemy.");
            return NodeType.MinorEnemy;
        }

        float total = config.randomNodeProbabilities.Sum(p => p.probability);
        float roll = Random.value * total;
        float current = 0f;
        foreach (var prob in config.randomNodeProbabilities)
        {
            current += prob.probability;
            if (roll <= current)
                return prob.nodeType;
        }
        return NodeType.MinorEnemy;
    }
}