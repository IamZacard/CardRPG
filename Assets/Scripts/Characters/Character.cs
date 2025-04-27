using UnityEngine;
using TMPro;
using System;

public class Character : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public int block;
    public TMP_Text healthText;
    public TMP_Text blockText;

    public Action<Character> OnDeath;

    protected virtual void Start()
    {
        if (maxHealth > 0) // Only set if maxHealth is initialized
            currentHealth = maxHealth;
        UpdateUI();
    }

    public virtual void TakeDamage(int damage)
    {
        if (block >= damage) block -= damage;
        else
        {
            int remainingDamage = damage - block;
            block = 0;
            currentHealth -= remainingDamage;
            if (currentHealth <= 0) Die();
        }
        UpdateUI();
    }

    public virtual void AddBlock(int amount)
    {
        block += amount;
        UpdateUI();
    }

    protected virtual void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        if (blockText != null)
            blockText.text = $"Block: {block}";
    }

    protected virtual void Die()
    {
        Debug.Log($"{name} died!");
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}