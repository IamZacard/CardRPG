using UnityEngine;
using TMPro;
using DG.Tweening;

public class PlayerHUDController : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI blockText;
    [SerializeField] private TextMeshProUGUI goldText;

    private int lastGold = -1;
    private Player player;

    public void Initialize(Player p)
    {
        player = p;

        // Subscribe to player events
        player.OnHealthChanged += SetHealth;
        player.OnBlockChanged += SetBlock;

        // Set initial values
        SetHealth(player.CurrentHealth);
        SetBlock(player.CurrentBlock);
        SetGold(player.playerData.gold);
        SetEnergy(player.CurrentEnergy);
    }

    public void SetHealth(int value)
    {
        AnimateStat(healthText, $"HP: {value}/{player.MaxHealth}");
    }

    public void SetBlock(int value)
    {
        AnimateStat(blockText, $"Block: {value}");
    }

    public void SetEnergy(int value)
    {
        AnimateStat(energyText, $"{value}/{player.MaxEnergy}");
    }

    public void SetGold(int value)
    {
        if (value != lastGold)
        {
            lastGold = value;
            AnimateStat(goldText, $"Gold: {value}");
        }
    }

    private void AnimateStat(TextMeshProUGUI textElement, string newText)
    {
        textElement.text = newText;

        textElement.transform.DOKill(); // Stop any ongoing animation
        textElement.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 10, 1);
    }
}
