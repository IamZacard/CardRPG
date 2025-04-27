using UnityEngine;

public enum CardType { Attack, Defense, Effect }
public enum StatusEffect { None, Poison, Stun, Burn }

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class Card : ScriptableObject
{
    public string cardName;
    public int cost;
    public CardType type;
    public int damage;
    public int block;
    public StatusEffect statusEffect;
    public int statusEffectValue;
    public string effectDescription; // Optional for custom descriptions

    public string GetFormattedDescription()
    {
        if (!string.IsNullOrEmpty(effectDescription))
            return effectDescription; // Use custom description if provided

        string description = "";
        switch (type)
        {
            case CardType.Attack:
                if (damage > 0)
                    description = $"Deal {damage} damage";
                break;
            case CardType.Defense:
                if (block > 0)
                    description = $"Block {block} damage";
                break;
            case CardType.Effect:
                if (statusEffect != StatusEffect.None)
                    description = $"Apply {statusEffectValue} {statusEffect}";
                break;
        }

        // Append status effect if present (for Attack/Defense cards with effects)
        if (statusEffect != StatusEffect.None && type != CardType.Effect)
            description += $", apply {statusEffectValue} {statusEffect}";

        return description;
    }
}