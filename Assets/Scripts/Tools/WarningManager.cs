using DG.Tweening;
using TMPro;
using UnityEngine;

public class WarningManager : MonoBehaviour
{
    public static WarningManager Instance;

    public GameObject warningPanelPrefab;
    private GameObject warningPanel;
    private RectTransform panelRect;
    private CanvasGroup canvasGroup;
    private float cooldownTimer;
    private bool isAnimating;

    void Awake()
    {
        if (Instance == null) Instance = this;

        Canvas mainCanvas = null;
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.name == "MainCanvas")
            {
                mainCanvas = canvas;
                break;
            }
        }

        if (mainCanvas == null) return;

        if (warningPanelPrefab != null)
        {
            warningPanel = Instantiate(warningPanelPrefab, mainCanvas.transform);
            warningPanel.name = "WarningPanel";
        }
        else return;

        panelRect = warningPanel.GetComponent<RectTransform>();
        canvasGroup = warningPanel.GetComponent<CanvasGroup>() ?? warningPanel.AddComponent<CanvasGroup>();
        warningPanel.SetActive(false);
        canvasGroup.alpha = 0f;
        panelRect.anchoredPosition = new Vector2(0, -Screen.height);
    }

    public void ShowWarning(string message)
    {
        if (isAnimating || cooldownTimer > 0 || warningPanel == null) return;

        isAnimating = true;
        warningPanel.SetActive(true);
        TMP_Text warningText = warningPanel.GetComponentInChildren<TMP_Text>();
        if (warningText != null) warningText.text = message;

        panelRect.anchoredPosition = new Vector2(0, -Screen.height);
        panelRect.localRotation = Quaternion.Euler(0, 0, -90);
        canvasGroup.alpha = 0f;

        Sequence rollSequence = DOTween.Sequence();
        rollSequence.Append(panelRect.DOAnchorPos(new Vector2(0, -160), 1f).SetEase(Ease.OutQuad)); // Target y = -160
        rollSequence.Join(panelRect.DORotate(new Vector3(0, 0, 0), 0.8f).SetEase(Ease.OutBack));
        rollSequence.Join(canvasGroup.DOFade(1f, 0.5f));
        rollSequence.OnComplete(() => Invoke(nameof(HideWarning), 1.5f));
    }

    private void HideWarning()
    {
        Sequence rollOutSequence = DOTween.Sequence();
        rollOutSequence.Append(panelRect.DOAnchorPos(new Vector2(0, -Screen.height), 1f).SetEase(Ease.InQuad));
        rollOutSequence.Join(panelRect.DORotate(new Vector3(0, 0, 90), 0.8f).SetEase(Ease.InBack));
        rollOutSequence.Join(canvasGroup.DOFade(0f, 0.5f));
        rollOutSequence.OnComplete(() =>
        {
            warningPanel.SetActive(false);
            isAnimating = false;
            cooldownTimer = 3f;
        });
    }

    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
}