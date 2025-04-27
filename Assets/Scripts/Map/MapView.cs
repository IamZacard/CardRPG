using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapView : MonoBehaviour
{
    public enum MapOrientation
    {
        BottomToTop,
        TopToBottom,
        RightToLeft,
        LeftToRight
    }

    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private MapConfig config;
    [SerializeField] private MapOrientation orientation = MapOrientation.BottomToTop;
    [SerializeField] private float orientationOffset = 1f;
    [SerializeField] private float layerSpacing = 2f;
    [SerializeField] private float nodeSpacing = 1f;
    [Header("Background Settings")]
    [SerializeField] private Sprite background;
    [SerializeField] private Color32 backgroundColor = Color.white;
    [SerializeField] private float xSize = 10f;
    [SerializeField] private float yOffset = 1f;
    [Header("Line Settings")]
    [SerializeField, Range(3, 10)] private int linePointsCount = 10;
    [SerializeField] private float offsetFromNodes = 0.5f;

    public static Color LockedColor { get; } = Color.gray;
    public static Color VisitedColor { get; } = Color.green;
    private List<MapNode> spawnedNodes = new List<MapNode>();
    private List<LineConnection> spawnedLines = new List<LineConnection>();
    private readonly Dictionary<Vector2Int, MapNode> nodeMap = new Dictionary<Vector2Int, MapNode>();
    private GameObject firstParent;
    private GameObject mapParent;
    private Camera cam;

    public static MapView Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    public MapNode GetNode(Vector2Int point)
    {
        nodeMap.TryGetValue(point, out MapNode node);
        return node;
    }

    public void GenerateMapVisual(List<MapLayer> layers)
    {
        ClearMap();

        if (layers == null || config == null || nodePrefab == null || linePrefab == null)
        {
            Debug.LogError("MapView: Required references are missing.");
            return;
        }

        CreateMapParent();

        float maxNodeWidth = layers.Max(l => (l.Nodes.Count - 1) * nodeSpacing);
        for (int i = 0; i < layers.Count; i++)
        {
            MapLayer layer = layers[i];
            if (layer == null || layer.Nodes == null)
            {
                Debug.LogWarning($"MapView: Layer {i} is null or has no nodes.");
                continue;
            }

            float xPos = 0, yPos = 0;
            switch (orientation)
            {
                case MapOrientation.BottomToTop:
                    yPos = i * layerSpacing;
                    break;
                case MapOrientation.TopToBottom:
                    yPos = -i * layerSpacing;
                    break;
                case MapOrientation.RightToLeft:
                    xPos = -i * layerSpacing;
                    break;
                case MapOrientation.LeftToRight:
                    xPos = i * layerSpacing;
                    break;
            }

            float totalWidth = (layer.Nodes.Count - 1) * nodeSpacing;
            float xStart = -totalWidth / 2;

            for (int j = 0; j < layer.Nodes.Count; j++)
            {
                Node node = layer.Nodes[j];
                if (node == null)
                {
                    Debug.LogWarning($"MapView: Node at layer {i}, index {j} is null.");
                    continue;
                }

                NodeBlueprint blueprint = config.GetBlueprint(node.nodeType);
                if (blueprint == null)
                {
                    Debug.LogError($"MapView: No NodeBlueprint found for NodeType {node.nodeType}.");
                    continue;
                }

                GameObject nodeObj = Instantiate(nodePrefab, mapParent.transform);
                float nodeX = (orientation == MapOrientation.RightToLeft || orientation == MapOrientation.LeftToRight) ? xPos : xStart + j * nodeSpacing;
                float nodeY = (orientation == MapOrientation.BottomToTop || orientation == MapOrientation.TopToBottom) ? yPos : xStart + j * nodeSpacing;
                nodeObj.transform.localPosition = new Vector3(nodeX, nodeY, 0);
                MapNode mapNode = nodeObj.GetComponent<MapNode>();
                if (mapNode == null)
                {
                    Debug.LogError($"MapView: Node Prefab missing MapNode component.");
                    Destroy(nodeObj);
                    continue;
                }

                mapNode.SetUp(node, blueprint);
                node.position = nodeObj.transform.localPosition;
                nodeMap[node.point] = mapNode;
                spawnedNodes.Add(mapNode);
            }
        }

        foreach (var mapNode in spawnedNodes)
        {
            foreach (var outgoingPoint in mapNode.Node.outgoing)
            {
                MapNode toNode = GetNode(outgoingPoint);
                if (toNode == null || spawnedLines.Any(l => (l.FromNode == mapNode && l.ToNode == toNode) || (l.FromNode == toNode && l.ToNode == mapNode)))
                    continue;

                GameObject lineObj = Instantiate(linePrefab, mapParent.transform);
                LineConnection line = lineObj.GetComponent<LineConnection>();
                if (line == null)
                {
                    Debug.LogError($"MapView: Line Prefab missing LineConnection component.");
                    Destroy(lineObj);
                    continue;
                }

                line.Initialize(mapNode, toNode, linePointsCount, offsetFromNodes);
                spawnedLines.Add(line);
            }
        }

        SetOrientation();
        CreateMapBackground(layers, maxNodeWidth);
        SetAttainableNodes();
        SetLineColors();
    }

    public void UpdateVisuals(Dictionary<Vector2Int, NodeStates> nodeStates)
    {
        if (nodeStates == null)
        {
            Debug.LogError("MapView: NodeStates dictionary is null.");
            return;
        }

        foreach (var mapNode in spawnedNodes)
        {
            if (mapNode == null) continue;
            if (nodeStates.TryGetValue(mapNode.Node.point, out NodeStates state))
            {
                mapNode.SetState(state);
            }
        }

        foreach (var line in spawnedLines)
        {
            if (line != null)
                line.UpdateLine(offsetFromNodes);
        }
    }

    public void SetAttainableNodes()
    {
        if (MapManager.Instance == null) return;

        foreach (MapNode node in spawnedNodes)
            node.SetState(NodeStates.Locked);

        var path = MapPlayerTracker.Instance.PlayerPath;
        if (path.Count == 0)
        {
            foreach (MapNode node in spawnedNodes.Where(n => n.Node.point.y == 0))
                node.SetState(NodeStates.Attainable);
        }
        else
        {
            foreach (Vector2Int point in path)
            {
                MapNode mapNode = GetNode(point);
                if (mapNode != null)
                    mapNode.SetState(NodeStates.Visited);
            }

            Vector2Int currentPoint = path[path.Count - 1];
            Node currentNode = MapManager.Instance.Layers.SelectMany(l => l.Nodes).FirstOrDefault(n => n.point.Equals(currentPoint));
            if (currentNode != null)
            {
                foreach (Vector2Int point in currentNode.outgoing)
                {
                    MapNode mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Attainable);
                }
            }
        }
    }

    public void SetLineColors()
    {
        foreach (LineConnection connection in spawnedLines)
            connection.SetColor(LockedColor);

        var path = MapPlayerTracker.Instance.PlayerPath;
        if (path.Count == 0)
            return;

        Vector2Int currentPoint = path[path.Count - 1];
        Node currentNode = MapManager.Instance.Layers.SelectMany(l => l.Nodes).FirstOrDefault(n => n.point.Equals(currentPoint));
        if (currentNode != null)
        {
            foreach (Vector2Int point in currentNode.outgoing)
            {
                LineConnection lineConnection = spawnedLines.FirstOrDefault(conn =>
                    conn.FromNode.Node.point.Equals(currentPoint) && conn.ToNode.Node.point.Equals(point));
                if (lineConnection != null)
                    lineConnection.SetColor(VisitedColor);
            }
        }

        if (path.Count <= 1) return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2Int current = path[i];
            Vector2Int next = path[i + 1];
            LineConnection lineConnection = spawnedLines.FirstOrDefault(conn =>
                conn.FromNode.Node.point.Equals(current) && conn.ToNode.Node.point.Equals(next));
            if (lineConnection != null)
                lineConnection.SetColor(VisitedColor);
        }
    }

    private void ClearMap()
    {
        if (firstParent != null)
            Destroy(firstParent);

        foreach (var node in spawnedNodes)
        {
            if (node != null) Destroy(node.gameObject);
        }
        spawnedNodes.Clear();
        nodeMap.Clear();

        foreach (var line in spawnedLines)
        {
            if (line != null) Destroy(line.gameObject);
        }
        spawnedLines.Clear();
    }

    private void CreateMapParent()
    {
        firstParent = new GameObject("OuterMapParent");
        mapParent = new GameObject("MapParentWithAScroll");
        mapParent.transform.SetParent(firstParent.transform);
        ScrollNonUI scrollNonUi = mapParent.AddComponent<ScrollNonUI>();
        scrollNonUi.freezeX = orientation == MapOrientation.BottomToTop || orientation == MapOrientation.TopToBottom;
        scrollNonUi.freezeY = orientation == MapOrientation.LeftToRight || orientation == MapOrientation.RightToLeft;
        BoxCollider boxCollider = mapParent.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(100, 100, 1);
    }

    private void CreateMapBackground(List<MapLayer> layers, float maxNodeWidth)
    {
        if (background == null) return;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(mapParent.transform);
        float span = (layers.Count - 1) * layerSpacing;

        // Position and size based on orientation
        Vector3 bgPosition;
        Vector2 bgSize;
        switch (orientation)
        {
            case MapOrientation.BottomToTop:
                bgPosition = new Vector3(0, span / 2f, -5); // Center vertically, Z=-5
                bgSize = new Vector2(maxNodeWidth + xSize, span + yOffset * 2f);
                break;
            case MapOrientation.TopToBottom:
                bgPosition = new Vector3(0, -span / 2f, -5); // Center vertically, Z=-5
                bgSize = new Vector2(maxNodeWidth + xSize, span + yOffset * 2f);
                break;
            case MapOrientation.RightToLeft:
                bgPosition = new Vector3(-span / 2f, 0, -5); // Center horizontally, Z=-5
                bgSize = new Vector2(span + yOffset * 2f, maxNodeWidth + xSize);
                break;
            case MapOrientation.LeftToRight:
                bgPosition = new Vector3(span / 2f, 0, -5); // Center horizontally, Z=-5
                bgSize = new Vector2(span + yOffset * 2f, maxNodeWidth + xSize);
                break;
            default:
                bgPosition = Vector3.zero;
                bgSize = Vector2.zero;
                break;
        }

        backgroundObject.transform.localPosition = bgPosition;
        backgroundObject.transform.localRotation = Quaternion.identity;
        SpriteRenderer sr = backgroundObject.AddComponent<SpriteRenderer>();
        sr.color = backgroundColor;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.sprite = background;
        sr.size = bgSize;
        sr.sortingOrder = -5; // Ensure background is behind everything else
    }

    private void SetOrientation()
    {
        if (mapParent == null) return;

        ScrollNonUI scrollNonUi = mapParent.GetComponent<ScrollNonUI>();
        float span = (MapManager.Instance.Layers.Count - 1) * layerSpacing;
        MapNode bossNode = spawnedNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);
        float offset = orientationOffset;

        // Position firstParent in front of camera
        firstParent.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);

        switch (orientation)
        {
            case MapOrientation.BottomToTop:
                if (scrollNonUi != null)
                {
                    scrollNonUi.yConstraints.max = 0; // Top of map (start)
                    scrollNonUi.yConstraints.min = -(span + 2f * offset); // Bottom of map (boss)
                }
                firstParent.transform.localPosition += new Vector3(0, offset, 0);
                mapParent.transform.localRotation = Quaternion.identity;
                break;
            case MapOrientation.TopToBottom:
                if (scrollNonUi != null)
                {
                    scrollNonUi.yConstraints.min = 0; // Bottom of map (start)
                    scrollNonUi.yConstraints.max = span + 2f * offset; // Top of map (boss)
                }
                firstParent.transform.localPosition += new Vector3(0, -offset, 0);
                mapParent.transform.localRotation = Quaternion.Euler(0, 0, 180); // Match sample
                break;
            case MapOrientation.RightToLeft:
                offset *= cam.aspect;
                if (scrollNonUi != null)
                {
                    scrollNonUi.xConstraints.max = span + 2f * offset; // Left of map (boss)
                    scrollNonUi.xConstraints.min = 0; // Right of map (start)
                }
                firstParent.transform.localPosition += new Vector3(-offset, -(bossNode?.transform.localPosition.y ?? 0), 0);
                mapParent.transform.localRotation = Quaternion.Euler(0, 0, 90); // Match sample
                break;
            case MapOrientation.LeftToRight:
                offset *= cam.aspect;
                if (scrollNonUi != null)
                {
                    scrollNonUi.xConstraints.max = 0; // Right of map (start)
                    scrollNonUi.xConstraints.min = -(span + 2f * offset); // Left of map (boss)
                }
                firstParent.transform.localPosition += new Vector3(offset, -(bossNode?.transform.localPosition.y ?? 0), 0);
                mapParent.transform.localRotation = Quaternion.Euler(0, 0, -90); // Match sample
                break;
        }

        foreach (MapNode node in spawnedNodes)
            node.transform.rotation = Quaternion.identity;
    }
}