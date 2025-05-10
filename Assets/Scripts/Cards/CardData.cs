using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Scriptable Object containing static card information.
/// Create assets from this class to define new cards.
/// </summary>
[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Information")]
    public string cardName;
    public string description;
    public int baseCost;

    [Header("Card Type")]
    public CardType cardType;

    [Header("Card Visuals")]
    public Sprite artwork;
    public Color frameColor = Color.white;

    [Header("Card Effects")]
    public List<CardEffect> effects = new List<CardEffect>();

    [Header("Tags")]
    public List<CardTag> tags = new List<CardTag>();

    // Optional rarity for future expansion
    public CardRarity rarity = CardRarity.Common;
}

/// <summary>
/// Defines the type of a card, which affects how it can be played
/// </summary>
public enum CardType
{
    Attack,
    Skill,
    Power,
    Status,
    Curse
}

/// <summary>
/// Defines the rarity of a card, useful for deck building and rewards
/// </summary>
public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
    Special
}

/// <summary>
/// Used to categorize cards for synergy effects and filtering
/// </summary>
public enum CardTag
{
    Fire,
    Ice,
    Lightning,
    Physical,
    Poison,
    Defensive,
    Healing,
    Drawing,
    Special
}

/// <summary>
/// Defines an effect a card can have when played
/// </summary>
[Serializable]
public class CardEffect
{
    public EffectType type;
    public int value;
    public TargetType target = TargetType.Enemy;
    public bool isAreaEffect = false;

    // For special effects that need additional parameters
    public string additionalParams;
}

/// <summary>
/// Types of effects a card can have
/// </summary>
public enum EffectType
{
    Damage,
    Block,
    Draw,
    ApplyStatus,
    GainEnergy,
    Heal,
    Special
}

/// <summary>
/// Valid targets for card effects
/// </summary>
public enum TargetType
{
    Self,
    Enemy,
    AllEnemies,
    Random
}