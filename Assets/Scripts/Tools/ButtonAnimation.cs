using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private Button button;

    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private float pressDuration = 0.1f;
    // [SerializeField] private AudioClip hoverSound;
    // [SerializeField] private AudioClip clickSound;

    private Vector3 originalScale;

    private void Awake()
    {
        // Cache references
        button = GetComponent<Button>() ?? GetComponentInParent<Button>();
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsValid()) return;

        KillMyTweens();
        transform
          .DOScale(originalScale * hoverScale, hoverDuration)
          .SetEase(Ease.OutQuad)
          .SetId(this);
        // audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsValid()) return;

        KillMyTweens();
        transform
          .DOScale(originalScale, hoverDuration)
          .SetEase(Ease.OutQuad)
          .SetId(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsValid()) return;

        KillMyTweens();
        var seq = DOTween.Sequence()
            .SetId(this)
            .Append(transform.DOScale(originalScale * pressScale, pressDuration).SetEase(Ease.InOutQuad))
            .Append(transform.DOScale(originalScale, pressDuration).SetEase(Ease.InOutQuad));
        // audioSource.PlayOneShot(clickSound);
    }

    private void OnDisable()
    {
        KillMyTweens();
    }

    private void OnDestroy()
    {
        KillMyTweens();
    }

    /// <summary>
    /// Kills ANY tween that was given the ID `this`.
    /// </summary>
    private void KillMyTweens()
    {
        DOTween.Kill(this, complete: false);
    }

    /// <summary>
    /// Quick guard so we never start a tween on a destroyed or inactive object.
    /// </summary>
    private bool IsValid()
    {
        return
            this != null &&
            gameObject != null &&
            gameObject.activeInHierarchy &&
            button != null &&
            button.interactable;
    }
}
