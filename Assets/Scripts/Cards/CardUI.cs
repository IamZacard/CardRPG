using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

/// <summary>
/// Handles the visual representation and interaction for a card in the UI
/// </summary>
public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Card References")]
    private Card card;

    [Header("UI Components")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image artworkImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image cardTypeIcon;

    [Header("Card Animation")]
    [SerializeField] private float hoverScaleFactor = 1.3f;
    [SerializeField] private float hoverElevation = 30f;
    [SerializeField] private float animationDuration = 0.3f; // Duration in seconds for DOTween animations

    // Original position, scale, and rotation
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    // Card state
    private bool isHovering = false;
    private bool isDragging = false;
    private bool isPlayable = true;

    // Events
    public event Action<CardUI> OnCardClicked;
    public event Action<CardUI> OnCardHoverStart;
    public event Action<CardUI> OnCardHoverEnd;
    public event Action<CardUI> OnCardDragStart;
    public event Action<CardUI> OnCardDragEnd;

    private void Awake()
    {
        // Store original transform information
        originalPosition = transform.localPosition; // Use localPosition for canvas compatibility
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    /// <summary>
    /// Initialize with a card instance
    /// </summary>
    public void Initialize(Card cardInstance)
    {
        card = cardInstance;

        // Set up event listeners for the card
        card.OnCostChanged += UpdateCostText;
        card.OnDescriptionChanged += UpdateDescriptionText;
        card.OnCardModified += _ => RefreshVisuals();

        // Initial visual update
        RefreshVisuals();
    }

    /// <summary>
    /// Get the card instance this UI represents
    /// </summary>
    public Card GetCard()
    {
        return card;
    }

    /// <summary>
    /// Refresh all visual elements based on card data
    /// </summary>
    public void RefreshVisuals()
    {
        if (card == null) return;

        // Update text elements
        nameText.text = card.cardData.cardName;
        costText.text = card.CurrentCost.ToString();
        descriptionText.text = card.CurrentDescription;

        // Update images
        artworkImage.sprite = card.cardData.artwork;
        frameImage.color = card.cardData.frameColor;

        // Update card type icon if available
        if (cardTypeIcon != null)
        {
            // Placeholder for card type sprite
            //cardTypeIcon.sprite = GetCardTypeSprite(card.cardData.cardType);
        }

        // Apply visual effects for playability
        ApplyPlayabilityVisuals();
    }

    /// <summary>
    /// Update only the cost text
    /// </summary>
    private void UpdateCostText(int newCost)
    {
        costText.text = newCost.ToString();
    }

    /// <summary>
    /// Update only the description text
    /// </summary>
    private void UpdateDescriptionText(string newDescription)
    {
        descriptionText.text = newDescription;
    }

    /// <summary>
    /// Set whether the card is currently playable and update visuals
    /// </summary>
    public void SetPlayable(bool playable)
    {
        isPlayable = playable;
        ApplyPlayabilityVisuals();
    }

    /// <summary>
    /// Apply visual effects based on playability state
    /// </summary>
    private void ApplyPlayabilityVisuals()
    {
        if (!isPlayable)
        {
            // Apply "unplayable" visual effect (grayscale)
            frameImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            nameText.color = Color.gray;
            costText.color = Color.gray;
            descriptionText.color = Color.gray;
        }
        else
        {
            // Restore normal visuals
            frameImage.color = card.cardData.frameColor;
            nameText.color = Color.white;
            costText.color = Color.white;
            descriptionText.color = Color.white;
        }
    }

    /// <summary>
    /// Animate to a specified position (used when arranging cards in hand)
    /// </summary>
    public void AnimateToPosition(Vector3 targetPosition, Quaternion targetRotation, float duration = 0.3f)
    {
        if (isHovering || isDragging) return;

        // Update original values for future animations
        originalPosition = targetPosition;
        originalRotation = targetRotation;

        // Kill any existing tweens to prevent conflicts
        transform.DOKill();

        // Animate position, rotation, and scale using DOTween
        transform.DOLocalMove(targetPosition, duration).SetEase(Ease.OutQuad);
        transform.DORotate(targetRotation.eulerAngles, duration).SetEase(Ease.OutQuad);
        transform.DOScale(originalScale, duration).SetEase(Ease.OutQuad);
    }

    #region Event System Handlers

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;

        isHovering = true;
        OnCardHoverStart?.Invoke(this);

        // Calculate hover position and scale
        Vector3 hoverPosition = originalPosition + new Vector3(0, hoverElevation, 0);
        Vector3 hoverScale = originalScale * hoverScaleFactor;

        // Kill existing tweens
        transform.DOKill();

        // Animate to hover state
        transform.DOLocalMove(hoverPosition, animationDuration).SetEase(Ease.OutQuad);
        transform.DORotate(Vector3.zero, animationDuration).SetEase(Ease.OutQuad); // Reset rotation
        transform.DOScale(hoverScale, animationDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;

        isHovering = false;
        OnCardHoverEnd?.Invoke(this);

        // Kill existing tweens
        transform.DOKill();

        // Animate back to original state
        transform.DOLocalMove(originalPosition, animationDuration).SetEase(Ease.InQuad);
        transform.DORotate(originalRotation.eulerAngles, animationDuration).SetEase(Ease.InQuad);
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.InQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;

        OnCardClicked?.Invoke(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isPlayable) return;

        isDragging = true;
        OnCardDragStart?.Invoke(this);

        // Kill existing tweens
        transform.DOKill();

        // Instantly scale up for drag (matches original behavior)
        transform.localScale = originalScale * 1.1f;

        // Optional: Animate scale for smoother effect
        transform.DOScale(originalScale * 1.1f, 0.1f).SetEase(Ease.OutQuad);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Move card with cursor (instant, as in original script)
        Vector3 screenPoint = new Vector3(
            eventData.position.x,
            eventData.position.y,
            Camera.main.WorldToScreenPoint(transform.position).z
        );
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPoint);
        transform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        isHovering = false;
        OnCardDragEnd?.Invoke(this);

        // Kill existing tweens
        transform.DOKill();

        // Animate back to original position, rotation, and scale
        transform.DOLocalMove(originalPosition, animationDuration).SetEase(Ease.InQuad);
        transform.DORotate(originalRotation.eulerAngles, animationDuration).SetEase(Ease.InQuad);
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.InQuad);
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up tweens to prevent memory leaks
        transform.DOKill();

        // Clean up event listeners
        if (card != null)
        {
            card.OnCostChanged -= UpdateCostText;
            card.OnDescriptionChanged -= UpdateDescriptionText;
            card.OnCardModified -= _ => RefreshVisuals();
        }
    }
}