using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MapNode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Image image;
    [SerializeField] private SpriteRenderer visitedCircle;
    [SerializeField] private Image circleImage;
    [SerializeField] private Image visitedCircleImage;

    public Node Node { get; private set; }
    public NodeBlueprint Blueprint { get; private set; }
    private NodeStates state;
    private float initialScale;
    private float mouseDownTime;
    private const float HoverScaleFactor = 1.2f;
    private const float MaxClickDuration = 0.5f;

    public void SetUp(Node node, NodeBlueprint blueprint)
    {
        Node = node;
        Blueprint = blueprint;
        if (sr != null) sr.sprite = blueprint.sprite;
        if (image != null) image.sprite = blueprint.sprite;
        if (node.nodeType == NodeType.Boss) transform.localScale *= 1.5f;
        initialScale = (sr != null) ? sr.transform.localScale.x : image.transform.localScale.x;

        if (visitedCircle != null)
        {
            visitedCircle.color = MapView.VisitedColor;
            visitedCircle.gameObject.SetActive(false);
        }
        if (circleImage != null)
        {
            circleImage.color = MapView.VisitedColor;
            circleImage.gameObject.SetActive(false);
        }
        if (visitedCircleImage != null)
        {
            visitedCircleImage.color = MapView.VisitedColor;
            visitedCircleImage.gameObject.SetActive(false);
        }

        SetState(NodeStates.Locked);
    }

    public void SetState(NodeStates newState)
    {
        state = newState;
        if (visitedCircle != null) visitedCircle.gameObject.SetActive(false);
        if (circleImage != null) circleImage.gameObject.SetActive(false);
        if (visitedCircleImage != null) visitedCircleImage.gameObject.SetActive(false);

        switch (state)
        {
            case NodeStates.Locked:
                if (sr != null)
                {
                    sr.DOKill();
                    sr.color = MapView.LockedColor;
                }
                if (image != null)
                {
                    image.DOKill();
                    image.color = MapView.LockedColor;
                }
                break;
            case NodeStates.Visited:
                if (sr != null)
                {
                    sr.DOKill();
                    sr.color = MapView.VisitedColor;
                }
                if (image != null)
                {
                    image.DOKill();
                    image.color = MapView.VisitedColor;
                }
                if (visitedCircle != null) visitedCircle.gameObject.SetActive(true);
                if (circleImage != null) circleImage.gameObject.SetActive(true);
                if (visitedCircleImage != null) visitedCircleImage.gameObject.SetActive(true);
                break;
            case NodeStates.Attainable:
                if (sr != null)
                {
                    sr.DOKill();
                    sr.color = MapView.LockedColor;
                    sr.DOColor(MapView.VisitedColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
                if (image != null)
                {
                    image.DOKill();
                    image.color = MapView.LockedColor;
                    image.DOColor(MapView.VisitedColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
                break;
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (state != NodeStates.Attainable) return;
        Transform target = (sr != null) ? sr.transform : image.transform;
        target.DOKill();
        target.DOScale(initialScale * HoverScaleFactor, 0.3f);
    }

    public void OnPointerExit(PointerEventData data)
    {
        Transform target = (sr != null) ? sr.transform : image.transform;
        target.DOKill();
        target.DOScale(initialScale, 0.3f);
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (state != NodeStates.Attainable) return;
        mouseDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (state != NodeStates.Attainable) return;
        if (Time.time - mouseDownTime < MaxClickDuration)
        {
            MapPlayerTracker.Instance.SelectNode(this);
        }
    }

    public void ShowSwirlAnimation()
    {
        if (visitedCircleImage == null) return;
        const float fillDuration = 0.3f;
        visitedCircleImage.fillAmount = 0;
        DOTween.To(() => visitedCircleImage.fillAmount, x => visitedCircleImage.fillAmount = x, 1f, fillDuration);
    }

    private void OnDestroy()
    {
        if (image != null)
        {
            image.transform.DOKill();
            image.DOKill();
        }
        if (sr != null)
        {
            sr.transform.DOKill();
            sr.DOKill();
        }
    }
}