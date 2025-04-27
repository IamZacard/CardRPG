using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string Name;
    public int MaxHealth;
    public Sprite Sprite;
    public List<Card> Cards;
}