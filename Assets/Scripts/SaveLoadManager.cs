using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct SerializableVector2Int
{
    public int x;
    public int y;

    public SerializableVector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int ToVector2Int() => new Vector2Int(x, y);
    public static SerializableVector2Int FromVector2Int(Vector2Int v) => new SerializableVector2Int(v.x, v.y);
}

[System.Serializable]
public class GameState
{
    public int version = 1;
    public int playerHealth;
    public List<string> deckCardNames;
    public int playerGold;
    public List<(string character, StatusEffect effect, int value)> statusEffects;
    public MapSaveData mapData;
    public List<SerializableVector2Int> playerPath;
}

[System.Serializable]
public class MapSaveData
{
    public List<NodeSaveData> nodes;
    public List<ConnectionSaveData> connections;
    public SerializableVector2Int currentNodePoint;
    public List<NodeStateSaveData> nodeStates;
}

[System.Serializable]
public class NodeSaveData
{
    public NodeType type;
    public string blueprintName;
    public SerializableVector2Int point;
    public Vector2 position;
}

[System.Serializable]
public class ConnectionSaveData
{
    public SerializableVector2Int fromPoint;
    public SerializableVector2Int toPoint;
}

[System.Serializable]
public class NodeStateSaveData
{
    public SerializableVector2Int point;
    public NodeStates state;
}

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    private const int CurrentVersion = 1;
    private const string SaveFilePath = "/autosave.dat";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null)
            {
                Debug.LogWarning("SaveLoadManager is not a root GameObject. Moving to root to support DontDestroyOnLoad.");
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AutoSave()
    {
        GameState state = new GameState
        {
            version = CurrentVersion,
            playerHealth = CombatManager.Instance != null && CombatManager.Instance.player != null
                ? CombatManager.Instance.player.currentHealth
                : 50,
            deckCardNames = new List<string>(),
            playerGold = FindObjectOfType<ShopManager>()?.GetPlayerGold() ?? 50,
            statusEffects = new List<(string, StatusEffect, int)>(),
            mapData = SaveMapState(),
            playerPath = MapPlayerTracker.Instance.PlayerPath
                .Select(SerializableVector2Int.FromVector2Int)
                .ToList()
        };

        if (DeckManager.Instance != null)
        {
            foreach (var card in DeckManager.Instance.startingDeck)
                state.deckCardNames.Add(card.cardName);
        }

        if (StatusEffectManager.Instance != null)
        {
            foreach (var entry in StatusEffectManager.Instance.GetAllEffects())
            {
                foreach (var effectTuple in entry.Value)
                {
                    state.statusEffects.Add((entry.Key.name, effectTuple.effect, effectTuple.value));
                }
            }
        }

        string json = JsonUtility.ToJson(state, true);
        string path = Application.persistentDataPath + SaveFilePath;
        File.WriteAllText(path, json);
        Debug.Log($"Auto-saved game state to {path}");
    }

    public void LoadAutoSave()
    {
        string path = Application.persistentDataPath + SaveFilePath;
        if (!File.Exists(path))
        {
            Debug.LogWarning("No autosave file found!");
            MapManager.Instance?.GenerateMap();
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Autosave file is empty!");
                MapManager.Instance?.GenerateMap();
                return;
            }

            GameState state = JsonUtility.FromJson<GameState>(json);
            if (state == null)
            {
                Debug.LogError("Failed to deserialize autosave!");
                MapManager.Instance?.GenerateMap();
                return;
            }

            if (state.version != CurrentVersion)
            {
                Debug.LogWarning($"Save file version ({state.version}) does not match current version ({CurrentVersion}). Attempting to load anyway.");
                // Handle version migrations here if needed
            }

            if (CombatManager.Instance != null && CombatManager.Instance.player != null)
            {
                CombatManager.Instance.player.currentHealth = state.playerHealth;
                Debug.Log($"Restored player health: {state.playerHealth}");
            }

            if (DeckManager.Instance != null && state.deckCardNames != null)
            {
                Debug.Log($"Deck restoration not implemented. Saved card names: {string.Join(", ", state.deckCardNames)}");
            }

            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null)
            {
                shopManager.SetPlayerGold(state.playerGold);
                Debug.Log($"Restored player gold: {state.playerGold}");
            }

            if (StatusEffectManager.Instance != null && state.statusEffects != null)
            {
                foreach (var effect in state.statusEffects)
                {
                    Character character = FindObjectsOfType<Character>().FirstOrDefault(c => c.name == effect.character);
                    if (character != null)
                    {
                        StatusEffectManager.Instance.ApplyEffect(character, effect.effect, effect.value);
                        Debug.Log($"Restored status effect: {effect.effect} on {effect.character}");
                    }
                }
            }

            if (MapManager.Instance != null && state.mapData != null)
            {
                LoadMapState(state.mapData);
            }

            if (MapPlayerTracker.Instance != null && state.playerPath != null)
            {
                MapPlayerTracker.Instance.SetPath(state.playerPath.Select(p => p.ToVector2Int()).ToList());
                MapView.Instance.SetAttainableNodes();
                MapView.Instance.SetLineColors();
            }

            Debug.Log("Loaded autosave.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load autosave: {ex.Message}\nStackTrace: {ex.StackTrace}");
            MapManager.Instance?.GenerateMap();
        }
    }

    private MapSaveData SaveMapState()
    {
        if (MapManager.Instance == null) return null;

        MapSaveData mapData = new MapSaveData
        {
            nodes = new List<NodeSaveData>(),
            connections = new List<ConnectionSaveData>(),
            currentNodePoint = MapManager.Instance.CurrentNode != null
                ? SerializableVector2Int.FromVector2Int(MapManager.Instance.CurrentNode.point)
                : new SerializableVector2Int(-1, -1),
            nodeStates = new List<NodeStateSaveData>()
        };

        foreach (var layer in MapManager.Instance.Layers)
        {
            foreach (var node in layer.Nodes)
            {
                mapData.nodes.Add(new NodeSaveData
                {
                    type = node.nodeType,
                    blueprintName = node.blueprintName,
                    point = SerializableVector2Int.FromVector2Int(node.point),
                    position = node.position
                });

                foreach (var outgoingPoint in node.outgoing)
                {
                    if (!mapData.connections.Exists(c => c.fromPoint.Equals(SerializableVector2Int.FromVector2Int(node.point)) &&
                                                        c.toPoint.Equals(SerializableVector2Int.FromVector2Int(outgoingPoint))))
                    {
                        mapData.connections.Add(new ConnectionSaveData
                        {
                            fromPoint = SerializableVector2Int.FromVector2Int(node.point),
                            toPoint = SerializableVector2Int.FromVector2Int(outgoingPoint)
                        });
                    }
                }
            }
        }

        foreach (var state in MapManager.Instance.nodeStates)
        {
            mapData.nodeStates.Add(new NodeStateSaveData
            {
                point = SerializableVector2Int.FromVector2Int(state.Key),
                state = state.Value
            });
        }

        Debug.Log($"Saved map: {mapData.nodes.Count} nodes, {mapData.connections.Count} connections, {mapData.nodeStates.Count} states");
        return mapData;
    }

    private void LoadMapState(MapSaveData mapData)
    {
        if (MapManager.Instance == null) return;

        MapManager.Instance.Layers.Clear();
        MapManager.Instance.nodeStates.Clear();

        int maxLayerIndex = mapData.nodes.Max(n => n.point.y);
        for (int i = 0; i <= maxLayerIndex; i++)
        {
            MapLayer layer = new MapLayer(i);
            var layerNodes = mapData.nodes.Where(n => n.point.y == i).ToList();
            foreach (var nodeData in layerNodes)
            {
                Node node = new Node(nodeData.type, nodeData.blueprintName, nodeData.point.ToVector2Int())
                {
                    position = nodeData.position
                };
                layer.AddNode(node);
            }
            MapManager.Instance.Layers.Add(layer);
        }

        foreach (var conn in mapData.connections)
        {
            Node fromNode = MapManager.Instance.Layers
                .Find(l => l.Nodes.Any(n => n.point.Equals(conn.fromPoint.ToVector2Int())))
                ?.Nodes.Find(n => n.point.Equals(conn.fromPoint.ToVector2Int()));
            Node toNode = MapManager.Instance.Layers
                .Find(l => l.Nodes.Any(n => n.point.Equals(conn.toPoint.ToVector2Int())))
                ?.Nodes.Find(n => n.point.Equals(conn.toPoint.ToVector2Int()));
            if (fromNode != null && toNode != null)
            {
                fromNode.AddOutgoing(conn.toPoint.ToVector2Int());
                toNode.AddIncoming(conn.fromPoint.ToVector2Int());
            }
        }

        foreach (var state in mapData.nodeStates)
        {
            MapManager.Instance.nodeStates[state.point.ToVector2Int()] = state.state;
        }

        Node currentNode = MapManager.Instance.Layers
            .SelectMany(l => l.Nodes)
            .FirstOrDefault(n => n.point.Equals(mapData.currentNodePoint.ToVector2Int()));
        if (currentNode != null)
        {
            MapManager.Instance.SetCurrentNode(currentNode);
        }
        else
        {
            Debug.LogWarning($"Current node at point {mapData.currentNodePoint.x},{mapData.currentNodePoint.y} not found!");
        }

        // Update visuals if MapView has nodes, otherwise generate new visuals
        if (MapView.Instance != null)
        {
            // Check if MapView has any nodes (indicating existing visuals)
            if (MapView.Instance.GetNode(Vector2Int.zero) != null || mapData.nodes.Any(n => MapView.Instance.GetNode(n.point.ToVector2Int()) != null))
            {
                MapView.Instance.UpdateVisuals(MapManager.Instance.nodeStates);
                MapView.Instance.SetAttainableNodes();
                MapView.Instance.SetLineColors();
                Debug.Log("Updated existing map visuals");
            }
            else
            {
                MapManager.Instance.GenerateMapVisual();
                Debug.Log("Generated new map visuals");
            }
        }
    }

    private int GetNodeIndex(Node node)
    {
        if (node == null || MapManager.Instance == null) return -1;
        return MapManager.Instance.Layers.SelectMany(l => l.Nodes).ToList().IndexOf(node);
    }
}