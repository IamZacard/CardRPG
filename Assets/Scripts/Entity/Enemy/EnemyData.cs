using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public string description;
    public Sprite artwork;
    public int maxHealth;
    public int baseBlock;
    public bool isElite;
    public bool isBoss;

    [Header("Rewards")]
    public int goldReward;
    public int experienceReward;
    public int cardRewardCount;

    [Header("Actions")]
    public List<EnemyAction> possibleActions = new List<EnemyAction>();

    [Header("AI Settings")]
    public float aggressiveThreshold = 0.8f;
    public float defensiveThreshold = 0.4f;
    public float healThreshold = 0.3f;

    [Header("Boss Settings")]
    public List<BossPhaseData> bossPhases = new List<BossPhaseData>();

    [System.Serializable]
    public class BossPhaseData
    {
        public float healthPercentageThreshold;
        public List<EnemyAction> phaseActions = new List<EnemyAction>();
        public EnemyAction phaseTransitionAction;
    }
}