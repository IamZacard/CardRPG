using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class EnemyAction
{
    public string actionName;
    public int damage;
    public int block;
    public StatusEffectType? statusEffectType;
    public int statusEffectAmount;
    public bool targetsAll;
    public bool selfEffect;
    public bool isTelegraphed;
}

public class Enemy : Entity
{
    [Header("Enemy Stats")]
    public int goldReward;
    public int experienceReward;
    public bool isElite;
    public bool isBoss;

    [Header("Actions")]
    public List<EnemyAction> possibleActions = new List<EnemyAction>();
    public EnemyAction nextAction;

    public event Action<EnemyAction> OnActionSelected;
    public event Action OnTurnStarted;
    public event Action OnTurnEnded;

    protected override void Awake()
    {
        base.Awake(); // Only handle ID assignment from Entity**
    }

    public override void Initialize()
    {
        base.Initialize();
        SelectNextAction(); // Select initial action after data is set
    }

    public override void OnStartTurn()
    {
        base.OnStartTurn();
        OnTurnStarted?.Invoke();
    }

    public override void OnEndTurn()
    {
        base.OnEndTurn();
        OnTurnEnded?.Invoke();
        SelectNextAction();
    }

    public virtual void SelectNextAction()
    {
        if (possibleActions.Count == 0)
        {
            Debug.LogWarning($"Enemy {entityName} has no actions defined!");
            return;
        }

        int idx = UnityEngine.Random.Range(0, possibleActions.Count);
        nextAction = possibleActions[idx];
        OnActionSelected?.Invoke(nextAction);
    }

    public void SetNextAction(EnemyAction action)
    {
        nextAction = action;
        OnActionSelected?.Invoke(nextAction);
    }

    protected override void Die()
    {
        base.Die();
        // Add logic for dropping gold/XP if needed
    }
}