using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class MapPlayerTracker : MonoBehaviour
{
    [SerializeField] private bool lockAfterSelecting = false;
    [SerializeField] private float enterNodeDelay = 1f;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private MapView view;

    public static MapPlayerTracker Instance { get; private set; }
    public bool Locked { get; set; }
    public List<Vector2Int> PlayerPath { get; private set; } = new List<Vector2Int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null)
            {
                Debug.LogWarning("MapPlayerTracker is not a root GameObject. Moving to root to support DontDestroyOnLoad.");
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectNode(MapNode mapNode)
    {
        if (Locked || mapNode == null) return;

        if (PlayerPath.Count == 0)
        {
            if (mapNode.Node.point.y == 0)
                SendPlayerToNode(mapNode);
            else
                PlayWarningThatNodeCannotBeAccessed();
        }
        else
        {
            Vector2Int currentPoint = PlayerPath[PlayerPath.Count - 1];
            Node currentNode = mapManager.Layers.SelectMany(l => l.Nodes).FirstOrDefault(n => n.point.Equals(currentPoint));

            if (currentNode != null && currentNode.outgoing.Any(point => point.Equals(mapNode.Node.point)))
                SendPlayerToNode(mapNode);
            else
                PlayWarningThatNodeCannotBeAccessed();
        }
    }

    private void SendPlayerToNode(MapNode mapNode)
    {
        Locked = lockAfterSelecting;
        PlayerPath.Add(mapNode.Node.point);
        mapManager.nodeStates[mapNode.Node.point] = NodeStates.Visited;
        mapManager.SetCurrentNode(mapNode.Node);
        view.SetAttainableNodes();
        view.SetLineColors();
        mapNode.ShowSwirlAnimation();

        DOTween.Sequence().AppendInterval(enterNodeDelay).OnComplete(() => EnterNode(mapNode));
    }

    private void EnterNode(MapNode mapNode)
    {
        Debug.Log($"Entering node: {mapNode.Node.blueprintName} of type: {mapNode.Node.nodeType}");

        switch (mapNode.Node.nodeType)
        {
            case NodeType.MinorEnemy:
            case NodeType.EliteEnemy:
            case NodeType.Boss:
                SceneManager.LoadScene("Combat");
                break;
            case NodeType.RestSite:
                if (CombatManager.Instance != null && CombatManager.Instance.player != null)
                {
                    CombatManager.Instance.player.currentHealth = Mathf.Min(
                        CombatManager.Instance.player.currentHealth + 10,
                        CombatManager.Instance.player.maxHealth);
                    Debug.Log($"Player healed to {CombatManager.Instance.player.currentHealth}");
                }
                Locked = false; // Unlock for GUI
                break;
            case NodeType.Treasure:
            case NodeType.Mystery:
                Debug.Log($"{mapNode.Node.nodeType} node triggered!");
                Locked = false; // Unlock for GUI
                break;
            case NodeType.Store:
                SceneManager.LoadScene("Shop");
                break;
            case NodeType.StartingNode:
                Locked = false; // Unlock for starting node
                break;
        }

        SaveLoadManager.Instance.AutoSave();
    }

    private void PlayWarningThatNodeCannotBeAccessed()
    {
        Debug.Log("Selected node cannot be accessed");
    }

    public void ClearPath()
    {
        PlayerPath.Clear();
        Locked = false;
    }

    public void SetPath(List<Vector2Int> path)
    {
        PlayerPath = new List<Vector2Int>(path);
    }
}