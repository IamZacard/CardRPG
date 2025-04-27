using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }
    private Dictionary<Character, List<(StatusEffect effect, int value)>> effects = new Dictionary<Character, List<(StatusEffect, int)>>();
    public Dictionary<Character, List<(StatusEffect effect, int value)>> GetAllEffects() => effects;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ApplyEffect(Character target, StatusEffect effect, int value)
    {
        if (!effects.ContainsKey(target))
            effects[target] = new List<(StatusEffect, int)>();
        effects[target].Add((effect, value));
    }

    public void ProcessEffects(Character target)
    {
        if (!effects.ContainsKey(target)) return;

        var activeEffects = effects[target];
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var (effect, value) = activeEffects[i];
            switch (effect)
            {
                case StatusEffect.Poison:
                    target.TakeDamage(value);
                    activeEffects[i] = (effect, value - 1); // Decrease duration
                    if (activeEffects[i].value <= 0) activeEffects.RemoveAt(i);
                    break;
                case StatusEffect.Stun:
                    Debug.Log($"{target.name} is stunned!");
                    activeEffects.RemoveAt(i); // One-turn stun
                    break;
                case StatusEffect.Burn:
                    target.TakeDamage(value);
                    activeEffects.RemoveAt(i); // One-time burn
                    break;
            }
        }
        if (activeEffects.Count == 0) effects.Remove(target);
    }

    // New method to check if a character has a specific effect
    public bool HasEffect(Character target, StatusEffect effect)
    {
        if (!effects.ContainsKey(target)) return false;
        foreach (var (e, _) in effects[target])
        {
            if (e == effect) return true;
        }
        return false;
    }
}