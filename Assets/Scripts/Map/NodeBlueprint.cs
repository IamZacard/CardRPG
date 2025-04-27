using UnityEngine;

[CreateAssetMenu(fileName = "NodeBlueprint", menuName = "Map/NodeBlueprint")]
public class NodeBlueprint : ScriptableObject
{
    public Sprite sprite;
    public NodeType nodeType;
}