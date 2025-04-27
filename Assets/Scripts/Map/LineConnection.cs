using UnityEngine;

public class LineConnection : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    private MapNode fromNode;
    private MapNode toNode;

    public MapNode FromNode => fromNode;
    public MapNode ToNode => toNode;

    public void Initialize(MapNode from, MapNode to, int pointCount, float offsetFromNodes)
    {
        fromNode = from;
        toNode = to;
        lineRenderer.positionCount = pointCount;
        UpdateLine(offsetFromNodes);
    }

    public void UpdateLine(float offsetFromNodes)
    {
        if (lineRenderer != null && fromNode != null && toNode != null)
        {
            Vector3 fromPoint = fromNode.transform.position +
                                (toNode.transform.position - fromNode.transform.position).normalized * offsetFromNodes;
            Vector3 toPoint = toNode.transform.position +
                              (fromNode.transform.position - toNode.transform.position).normalized * offsetFromNodes;

            transform.position = fromPoint;
            lineRenderer.useWorldSpace = false;

            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, Vector3.Lerp(Vector3.zero, toPoint - fromPoint, (float)i / (lineRenderer.positionCount - 1)));
            }

            bool isVisited = MapManager.Instance.nodeStates.ContainsKey(fromNode.Node.point) &&
                             MapManager.Instance.nodeStates[fromNode.Node.point] == NodeStates.Visited ||
                             MapManager.Instance.nodeStates.ContainsKey(toNode.Node.point) &&
                             MapManager.Instance.nodeStates[toNode.Node.point] == NodeStates.Visited;
            Color color = isVisited ? MapView.VisitedColor : MapView.LockedColor;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }

    public void SetColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }
}