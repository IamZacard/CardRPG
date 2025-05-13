// ================== CardUI.cs ==================
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
    [SerializeField] private CardTypeIconSet cardTypeIcons;

    [Header("Card Animation")]
    [SerializeField] private float hoverScaleFactor = 1.2f;
    [SerializeField] private float hoverElevation = 20f;
    [SerializeField] private float animationDuration = 0.2f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private bool isHovering;
    private bool isDragging;
    private bool isPlayable = true;

    public event Action<CardUI> OnCardClicked;
    public event Action<CardUI> OnCardHoverStart;
    public event Action<CardUI> OnCardHoverEnd;
    public event Action<CardUI> OnCardDragStart;
    public event Action<CardUI> OnCardDragEnd;

    private void Awake()
    {
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    public void Initialize(Card cardInstance)
    {
        card = cardInstance;

        card.OnCostChanged += UpdateCostText;
        card.OnDescriptionChanged += UpdateDescriptionText;
        card.OnCardModified += HandleCardModified;

        RefreshVisuals();
    }

    private void HandleCardModified(Card _)
    {
        RefreshVisuals();
    }

    public Card GetCard() => card;

    public void RefreshVisuals()
    {
        if (card == null) return;

        nameText.text = card.cardData.cardName;
        costText.text = card.CurrentCost.ToString();
        descriptionText.text = card.CurrentDescription;

        artworkImage.sprite = card.cardData.artwork;
        frameImage.color = card.cardData.frameColor;

        if (cardTypeIcon != null && cardTypeIcons != null)
            cardTypeIcon.sprite = cardTypeIcons.GetIcon(card.cardData.cardType);

        ApplyPlayabilityVisuals();
    }

    private void UpdateCostText(int newCost)
    {
        costText.text = newCost.ToString();
    }

    private void UpdateDescriptionText(string newDescription)
    {
        descriptionText.text = newDescription;
    }

    public void SetPlayable(bool playable)
    {
        isPlayable = playable;
        ApplyPlayabilityVisuals();
    }

    private void ApplyPlayabilityVisuals()
    {
        Color gray = Color.gray;
        Color white = Color.white;

        bool isUnplayable = !isPlayable;

        frameImage.color = isUnplayable ? new Color(0.5f, 0.5f, 0.5f, 1f) : card.cardData.frameColor;
        nameText.color = isUnplayable ? gray : white;
        costText.color = isUnplayable ? gray : white;
        descriptionText.color = isUnplayable ? gray : white;
    }

    public void AnimateToPosition(Vector3 targetPosition, Quaternion targetRotation, float duration = 0.3f)
    {
        if (isHovering || isDragging) return;

        originalPosition = targetPosition;
        originalRotation = targetRotation;

        transform.DOKill();

        transform.DOLocalMove(targetPosition, duration).SetEase(Ease.OutQuad);
        transform.DOLocalRotateQuaternion(targetRotation, duration).SetEase(Ease.OutQuad);
        transform.DOScale(originalScale, duration).SetEase(Ease.OutQuad);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;

        isHovering = true;
        OnCardHoverStart?.Invoke(this);

        Vector3 hoverPosition = originalPosition + Vector3.up * hoverElevation;
        Vector3 hoverScale = originalScale * hoverScaleFactor;

        transform.DOKill();
        transform.DOLocalMove(hoverPosition, animationDuration).SetEase(Ease.OutQuad);
        transform.DOLocalRotate(Vector3.zero, animationDuration).SetEase(Ease.OutQuad);
        transform.DOScale(hoverScale, animationDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;

        isHovering = false;
        OnCardHoverEnd?.Invoke(this);

        transform.DOKill();
        transform.DOLocalMove(originalPosition, animationDuration).SetEase(Ease.InQuad);
        transform.DOLocalRotateQuaternion(originalRotation, animationDuration).SetEase(Ease.InQuad);
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

        transform.DOKill();
        transform.DOScale(originalScale * 1.1f, 0.1f).SetEase(Ease.OutQuad);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 screenPoint = new Vector3(eventData.position.x, eventData.position.y, Camera.main.WorldToScreenPoint(transform.position).z);
        transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        isHovering = false;
        OnCardDragEnd?.Invoke(this);

        transform.DOKill();
        transform.DOLocalMove(originalPosition, animationDuration).SetEase(Ease.InQuad);
        transform.DOLocalRotateQuaternion(originalRotation, animationDuration).SetEase(Ease.InQuad);
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.InQuad);
    }

    private void OnDestroy()
    {
        transform.DOKill();

        if (card != null)
        {
            card.OnCostChanged -= UpdateCostText;
            card.OnDescriptionChanged -= UpdateDescriptionText;
            card.OnCardModified -= HandleCardModified;
        }
    }
}