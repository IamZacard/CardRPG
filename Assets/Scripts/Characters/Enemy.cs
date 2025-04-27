using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Enemy : Character
{
    public EnemyData enemyData;
    public SpriteRenderer enemySprite;
    private int currentCardIndex = 0;

    public void Initialize()
    {
        if (enemyData != null)
        {
            maxHealth = enemyData.MaxHealth;
            currentHealth = maxHealth;
            if (enemySprite != null)
            {
                if (enemyData.Sprite != null)
                {
                    enemySprite.sprite = enemyData.Sprite;
                    Debug.Log($"Assigned sprite {enemyData.Sprite.name} to enemy {enemyData.Name}");
                }
                else
                {
                    Debug.LogWarning($"No sprite assigned in EnemyData for {enemyData.Name}");
                }
            }
            else
            {
                Debug.LogWarning("enemySprite is not assigned in Enemy component! Please assign the SpriteRenderer in the Inspector.");
            }
            if (healthText != null)
            {
                healthText.text = $"Health: {currentHealth}/{maxHealth}";
            }
            else
            {
                Debug.LogWarning("healthText is not assigned in Enemy component!");
            }
        }
        else
        {
            Debug.LogWarning("enemyData is not assigned in Enemy component!");
        }
    }

    private void Awake()
    {
        // Empty, as initialization is handled by Initialize()
    }

    protected override void Start()
    {
        base.Start();
        OnDeath += (Character enemy) => CombatManager.Instance?.OnEnemyDeath(this);
    }

    public void PerformAction(Player player)
    {
        if (StatusEffectManager.Instance.HasEffect(this, StatusEffect.Stun))
        {
            Debug.Log($"{name} is stunned and skips turn!");
            return;
        }

        if (enemyData != null && enemyData.Cards != null && enemyData.Cards.Count > 0)
        {
            // Cycle through cards
            Card cardToPlay = enemyData.Cards[currentCardIndex];
            currentCardIndex = (currentCardIndex + 1) % enemyData.Cards.Count;

            // Play card
            if (cardToPlay.damage > 0) player.TakeDamage(cardToPlay.damage);
            if (cardToPlay.block > 0) AddBlock(cardToPlay.block);
            if (cardToPlay.statusEffect != StatusEffect.None)
                StatusEffectManager.Instance.ApplyEffect(player, cardToPlay.statusEffect, cardToPlay.statusEffectValue);

            Debug.Log($"{enemyData.Name} played {cardToPlay.cardName}: {cardToPlay.GetFormattedDescription()}");
        }
        else
        {
            Debug.LogWarning($"{name} has no cards to play!");
        }
    }

    protected override void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        if (blockText != null)
            blockText.text = $"Block: {block}";
    }
}