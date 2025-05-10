using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Core Stats")]
    public string playerName;
    public Sprite characterSprite;
    public int maxHealth = 50;
    public int baseBlock = 0;
    public int currentHealth = 50;
    public int maxEnergy = 3;
    public int currentEnergy = 3;

    [Header("Meta")]
    public int gold = 0;
    public int experience = 0;
}
