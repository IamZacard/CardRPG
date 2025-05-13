using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EnemyHUDController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform nameTextRT;
    [SerializeField] private RectTransform healthSliderRT;
    [SerializeField] private RectTransform healthTextRT;
    [SerializeField] private RectTransform blockTextRT;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI blockText;

    [Header("Offsets (in screen pixels)")]
    [SerializeField] private Vector2 nameOffset = new Vector2(0, 90);
    [SerializeField] private Vector2 healthSliderOffset = new Vector2(0, -100);
    [SerializeField] private Vector2 healthTextOffset = new Vector2(0, -130);
    [SerializeField] private Vector2 blockTextOffset = new Vector2(0, -160);

    private Enemy enemy;

    public void Initialize(Enemy e)
    {
        enemy = e;

        // Set static name
        if (nameText != null)
            nameText.text = enemy.entityName;

        // Set up values and listeners
        UpdateHealth(enemy.CurrentHealth);
        UpdateBlock(enemy.CurrentBlock);
        enemy.OnHealthChanged += UpdateHealth;
        enemy.OnBlockChanged += UpdateBlock;

        // Convert world to screen space
        Vector2 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);

        // Setup UI element positions
        SetupRect(nameTextRT, screenPos + (Vector2)nameOffset);
        SetupRect(healthSliderRT, screenPos + (Vector2)healthSliderOffset);
        SetupRect(healthTextRT, screenPos + (Vector2)healthTextOffset);
        SetupRect(blockTextRT, screenPos + (Vector2)blockTextOffset);
    }

    private void SetupRect(RectTransform rt, Vector3 screenPosition)
    {
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.position = screenPosition;
    }

    private void UpdateHealth(int hp)
    {
        if (healthText != null)
            healthText.text = $"HP: {hp}/{enemy.MaxHealth}";

        if (healthSlider != null)
            healthSlider.value = (float)hp / enemy.MaxHealth;

        Animate(healthText);
    }

    private void UpdateBlock(int blk)
    {
        if (blockText != null)
            blockText.text = $"Block: {blk}";

        Animate(blockText);
    }

    private void Animate(TextMeshProUGUI txt)
    {
        if (txt == null) return;

        txt.transform.DOKill();
        txt.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 8, 1)
            .SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        if (enemy != null)
        {
            enemy.OnHealthChanged -= UpdateHealth;
            enemy.OnBlockChanged -= UpdateBlock;
        }
    }
}
