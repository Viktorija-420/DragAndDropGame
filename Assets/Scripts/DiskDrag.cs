using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class DiskDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("1 = smallest, larger number = bigger disk")]
    public int diskSize = 1;

    [HideInInspector]
    public bool draggable = true; // controlled by GameManager

    [HideInInspector]
    public int currentTowerIndex = -1;

    private RectTransform rectTransform;
    private Image image;
    private Canvas canvas;
    [HideInInspector]
    public CanvasGroup canvasGroup;

    [HideInInspector]
    public Vector2 originalAnchoredPosition;

    private Vector2 startPosition;
    private int startTower;
    private Transform startParent;

    private Color originalColor;
    public Color dragColor = new Color(1f, 1f, 1f, 0.7f);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalColor = image.color;
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!draggable || !GameManager2.Instance.IsTopDisk(this)) return;

        startPosition = rectTransform.anchoredPosition;
        startTower = currentTowerIndex;
        startParent = transform.parent;

        image.color = dragColor;
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!draggable || !GameManager2.Instance.IsTopDisk(this)) return;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector3 worldPoint
        );
        rectTransform.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.color = originalColor;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (!draggable)
        {
            ReturnToStartPosition();
            return;
        }

        int targetTowerIndex = GameManager2.Instance.FindTowerUnderPointer(eventData.position);

        if (targetTowerIndex >= 0 && targetTowerIndex != currentTowerIndex)
        {
            Transform targetTower = GameManager2.Instance.towers[targetTowerIndex];
            if (GameManager2.Instance.TryMove(this, targetTower)) return;
        }

        ReturnToStartPosition();
        GameManager2.Instance.RegisterDiskAtTower(this, startTower);
    }

    private void ReturnToStartPosition()
    {
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }

    public void SetPosition(Vector2 newPosition)
    {
        rectTransform.anchoredPosition = newPosition;
    }
}
