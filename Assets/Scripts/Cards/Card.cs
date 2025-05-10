using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents a runtime instance of a card during combat
/// </summary>
public class Card
{
    // Unique identifier for this card instance
    public readonly int instanceID;

    // Reference to the static data
    public readonly CardData cardData;

    // Dynamic card properties (can be modified during gameplay)
    private int currentCost;
    private string currentDescription;
    private List<StatusEffect> temporaryEffects = new List<StatusEffect>();

    // Events for when card properties change
    public event Action<int> OnCostChanged;
    public event Action<string> OnDescriptionChanged;
    public event Action<Card> OnCardModified;

    public Card(CardData data, int id)
    {
        cardData = data;
        instanceID = id;

        // Initialize dynamic properties with base values
        currentCost = data.baseCost;
        currentDescription = data.description;
    }

    // Properties for accessing and modifying dynamic state
    public int CurrentCost
    {
        get => currentCost;
        set
        {
            if (currentCost != value)
            {
                currentCost = value;
                OnCostChanged?.Invoke(currentCost);
                OnCardModified?.Invoke(this);
            }
        }
    }

    public string CurrentDescription
    {
        get => currentDescription;
        set
        {
            if (currentDescription != value)
            {
                currentDescription = value;
                OnDescriptionChanged?.Invoke(currentDescription);
                OnCardModified?.Invoke(this);
            }
        }
    }

    // Reset modifications to return card to its base state
    public void ResetModifications()
    {
        currentCost = cardData.baseCost;
        currentDescription = cardData.description;
        temporaryEffects.Clear();

        // Notify listeners of changes
        OnCostChanged?.Invoke(currentCost);
        OnDescriptionChanged?.Invoke(currentDescription);
        OnCardModified?.Invoke(this);
    }

    // Apply a temporary effect to the card
    public void ApplyTemporaryEffect(StatusEffect effect)
    {
        temporaryEffects.Add(effect);

        // Apply effect logic
        if (effect.effectType == CardStatusEffectType.CostModifier)
        {
            CurrentCost += effect.value;
        }

        OnCardModified?.Invoke(this);
    }

    // Get all temporary effects currently applied to this card
    public List<StatusEffect> GetTemporaryEffects()
    {
        return new List<StatusEffect>(temporaryEffects);
    }

    // Copy this card instance, creating a new one with same state
    public Card CreateCopy()
    {
        Card copy = new Card(cardData, CardFactory.GetNextCardID());

        // Copy dynamic state
        copy.CurrentCost = currentCost;
        copy.CurrentDescription = currentDescription;

        foreach (var effect in temporaryEffects)
        {
            copy.ApplyTemporaryEffect(effect);
        }

        return copy;
    }

    // Check if card can be played with current energy
    public bool CanPlayWithEnergy(int availableEnergy)
    {
        return currentCost <= availableEnergy;
    }
}

/// <summary>
/// Represents a temporary effect applied to a card
/// </summary>
[Serializable]
public class StatusEffect
{
    public CardStatusEffectType effectType;
    public int value;
    public int durationTurns;
    public string description;

    public StatusEffect(CardStatusEffectType type, int effectValue, int duration = 1)
    {
        effectType = type;
        value = effectValue;
        durationTurns = duration;
        description = $"{type} {value} for {duration} turn(s)";
    }
}

/// <summary>
/// Types of status effects that can be applied to cards
/// </summary>
public enum CardStatusEffectType
{
    CostModifier,
    DamageModifier,
    BlockModifier,
    EffectDoubler,
    RetainCard,
    ExhaustCard,
    EtherealCard,
    Special
}