// EnemyAI.cs
using UnityEngine;
using System.Collections.Generic;

public enum EnemyAIState
{
    Aggressive,
    Defensive,
    Buff,
    Heal,
    Special
}

[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public EnemyAIState defaultState = EnemyAIState.Aggressive;
    public float aggressiveThreshold = 0.8f;   // <=40% HP ? defensive
    public float defensiveThreshold = 0.4f;   // <=40% HP ? defensive
    public float healThreshold = 0.3f;   // <=30% HP ? heal
    public int buffInterval = 3;     // every 3 turns

    [Header("Actions by State")]
    public List<EnemyAction> aggressiveActions = new List<EnemyAction>();
    public List<EnemyAction> defensiveActions = new List<EnemyAction>();
    public List<EnemyAction> buffActions = new List<EnemyAction>();
    public List<EnemyAction> healActions = new List<EnemyAction>();
    public List<EnemyAction> specialActions = new List<EnemyAction>();

    private Enemy enemy;
    private EnemyAIState currentState;
    private int turnCounter = 0;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        currentState = defaultState;

        // Subscribe to our new turn-ended event
        enemy.OnTurnEnded += OnEnemyEndTurn;

        // Build the pool of possible actions (so Enemy.SelectNextAction never picks something outside these)
        ConfigureEnemyActions();

        // Immediately pick the very first action
        SelectActionBasedOnState();
    }

    private void OnDestroy()
    {
        if (enemy != null)
            enemy.OnTurnEnded -= OnEnemyEndTurn;
    }

    private void ConfigureEnemyActions()
    {
        enemy.possibleActions.Clear();
        enemy.possibleActions.AddRange(aggressiveActions);
        enemy.possibleActions.AddRange(defensiveActions);
        enemy.possibleActions.AddRange(buffActions);
        enemy.possibleActions.AddRange(healActions);
        enemy.possibleActions.AddRange(specialActions);
    }

    private void OnEnemyEndTurn()
    {
        turnCounter++;
        UpdateAIState();
        SelectActionBasedOnState();
    }

    private void UpdateAIState()
    {
        float hpPct = (float)enemy.CurrentHealth / enemy.MaxHealth;

        if (hpPct <= healThreshold && healActions.Count > 0)
            currentState = EnemyAIState.Heal;
        else if (hpPct <= defensiveThreshold && defensiveActions.Count > 0)
            currentState = EnemyAIState.Defensive;
        else if (turnCounter % buffInterval == 0 && buffActions.Count > 0)
            currentState = EnemyAIState.Buff;
        else if (SpecialConditionsMet() && specialActions.Count > 0)
            currentState = EnemyAIState.Special;
        else
            currentState = EnemyAIState.Aggressive;
    }

    // Override this in subclasses if you need boss-phase logic, etc.
    protected virtual bool SpecialConditionsMet() => false;

    private void SelectActionBasedOnState()
    {
        List<EnemyAction> pool = currentState switch
        {
            EnemyAIState.Aggressive => aggressiveActions.Count > 0 ? aggressiveActions : enemy.possibleActions,
            EnemyAIState.Defensive => defensiveActions,
            EnemyAIState.Buff => buffActions,
            EnemyAIState.Heal => healActions,
            EnemyAIState.Special => specialActions,
            _ => enemy.possibleActions
        };

        if (pool.Count == 0)
        {
            // fallback to whatever Enemy.SelectNextAction does
            enemy.SelectNextAction();
            return;
        }

        int idx = UnityEngine.Random.Range(0, pool.Count);
        enemy.SetNextAction(pool[idx]);
    }
}