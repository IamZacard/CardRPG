using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;

public enum StatusEffectType
{
    Strength,
    Dexterity,
    Poison,
    Vulnerable,
    Weak,
    Burn,
    Stun,
    Frail,
    Thorns
}

public abstract class Entity : MonoBehaviour
{
    public int entityID; // Unique identifier for each entity**
    private static int nextID = 0; // Static counter for generating IDs**

    [Header("Base Stats")]
    public string entityName;
    public int maxHealth;
    public int currentHealth;
    public int baseBlock;
    public int maxEnergy;

    [Header("Status")]
    protected Dictionary<StatusEffectType, int> statusEffects = new Dictionary<StatusEffectType, int>();
    protected int currentBlock;
    public int currentEnergy;

    //public PlayerClass playerClass;

    private Vector3 originalScale;

    // Events
    public event Action<int> OnHealthChanged;
    public event Action<int> OnBlockChanged;
    public event Action<StatusEffectType, int> OnStatusEffectApplied;
    public event Action<StatusEffectType, int> OnStatusEffectRemoved;
    public event Action OnDeath;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentBlock => currentBlock;

    public int CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxEnergy;

    protected virtual void Awake()
    {
        entityID = nextID++; // Assign unique ID**
    }

    public virtual void Initialize()
    {
        currentHealth = maxHealth; // Set initial health
        currentBlock = 0;
        statusEffects.Clear();
    }

    public virtual void TakeDamage(int amount, Entity source)
    {
        if (HasStatusEffect(StatusEffectType.Vulnerable))
        {
            amount = Mathf.FloorToInt(amount * 1.5f);
        }

        if (currentBlock > 0)
        {
            int blockDamage = Mathf.Min(amount, currentBlock);
            amount -= blockDamage;
            SetBlock(currentBlock - blockDamage);
        }

        if (amount > 0)
        {
            SetHealth(currentHealth - amount);
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    protected virtual void Start()
    {
        originalScale = transform.localScale;
    }

    public virtual void Heal(int amount)
    {
        SetHealth(Mathf.Min(currentHealth + amount, maxHealth));
    }

    public virtual void AddBlock(int amount)
    {
        if (HasStatusEffect(StatusEffectType.Frail))
        {
            amount = Mathf.FloorToInt(amount * 0.75f);
        }

        SetBlock(currentBlock + amount);
    }

    public virtual void ApplyStatusEffect(StatusEffectType type, int amount)
    {
        if (amount <= 0) return;

        if (!statusEffects.ContainsKey(type))
        {
            statusEffects[type] = 0;
        }

        statusEffects[type] += amount;
        OnStatusEffectApplied?.Invoke(type, statusEffects[type]);
    }

    public virtual void RemoveStatusEffect(StatusEffectType type, int amount = 0)
    {
        if (!statusEffects.ContainsKey(type)) return;

        if (amount <= 0 || amount >= statusEffects[type])
        {
            int previousAmount = statusEffects[type];
            statusEffects.Remove(type);
            OnStatusEffectRemoved?.Invoke(type, previousAmount);
        }
        else
        {
            statusEffects[type] -= amount;
            OnStatusEffectRemoved?.Invoke(type, amount);
        }
    }

    public bool HasStatusEffect(StatusEffectType type)
    {
        return statusEffects.ContainsKey(type) && statusEffects[type] > 0;
    }

    public int GetStatusEffectAmount(StatusEffectType type)
    {
        return statusEffects.ContainsKey(type) ? statusEffects[type] : 0;
    }

    public virtual void OnStartTurn()
    {
        ProcessStatusEffects(true);
    }

    public virtual void OnEndTurn()
    {
        ProcessStatusEffects(false);
        SetBlock(0);
    }

    protected virtual void ProcessStatusEffects(bool isStartOfTurn)
    {
        List<StatusEffectType> effectsToRemove = new List<StatusEffectType>();

        foreach (var effect in statusEffects)
        {
            StatusEffectType type = effect.Key;
            int value = effect.Value;

            if (isStartOfTurn)
            {
                switch (type)
                {
                    case StatusEffectType.Poison:
                        TakeDamage(value, this);
                        statusEffects[type]--;
                        break;
                    case StatusEffectType.Burn:
                        TakeDamage(value, this);
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case StatusEffectType.Vulnerable:
                    case StatusEffectType.Weak:
                    case StatusEffectType.Frail:
                    case StatusEffectType.Stun:
                        statusEffects[type]--;
                        break;
                }
            }

            if (statusEffects[type] <= 0)
            {
                effectsToRemove.Add(type);
            }
        }

        foreach (var type in effectsToRemove)
        {
            RemoveStatusEffect(type);
        }
    }

    public int GetDamageModifier()
    {
        int modifier = 0;
        if (HasStatusEffect(StatusEffectType.Strength))
        {
            modifier += GetStatusEffectAmount(StatusEffectType.Strength);
        }
        return modifier;
    }

    public int GetBlockModifier()
    {
        int modifier = 0;
        if (HasStatusEffect(StatusEffectType.Dexterity))
        {
            modifier += GetStatusEffectAmount(StatusEffectType.Dexterity);
        }
        return modifier;
    }

    protected virtual void SetHealth(int newHealth)
    {
        int oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        if (oldHealth != currentHealth)
        {
            OnHealthChanged?.Invoke(currentHealth);
        }
    }

    protected virtual void SetBlock(int newBlock)
    {
        int oldBlock = currentBlock;
        currentBlock = Mathf.Max(0, newBlock);
        if (oldBlock != currentBlock)
        {
            OnBlockChanged?.Invoke(currentBlock);
        }
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();
    }

    public virtual void Reset()
    {
        SetHealth(maxHealth);
        SetBlock(0);
        statusEffects.Clear();
    }

    public Dictionary<StatusEffectType, int> GetAllStatusEffects()
    {
        return new Dictionary<StatusEffectType, int>(statusEffects);
    }

    public void SetStatusEffects(Dictionary<StatusEffectType, int> effects)
    {
        statusEffects.Clear();
        foreach (var effect in effects)
        {
            statusEffects[effect.Key] = effect.Value;
            OnStatusEffectApplied?.Invoke(effect.Key, effect.Value);
        }
    }

    private void OnMouseEnter()
    {
        transform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack);
    }

    private void OnMouseExit()
    {
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack);
    }
}