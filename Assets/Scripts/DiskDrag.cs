using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class DiskDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("1 = smallest, larger number = bigger disk")]
    public int diskSize = 1;

    [Tooltip("Set false if this disk should not be draggable")]
    public bool draggable = true;

    [HideInInspector]
    public int currentTowerIndex = -1;

    private RectTransform rectTransform;
    private Image image;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private int startTower;
    private Transform startParent;

    // Visual feedback
    private Color originalColor;
    public Color dragColor = new Color(1f, 1f, 1f, 0.7f);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        
        // Get or add CanvasGroup
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalColor = image.color;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        if (!GameManager2.Instance.IsTopDisk(this)) return;

        startPosition = rectTransform.anchoredPosition;
        startTower = currentTowerIndex;
        startParent = transform.parent;

        // Visual feedback
        image.color = dragColor;
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        // Bring to front while dragging and detach from parent temporarily
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        if (!GameManager2.Instance.IsTopDisk(this)) return;

        // Convert screen position to canvas position
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
        // Always restore visual properties
        image.color = originalColor;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (!draggable)
        {
            ReturnToStartPosition();
            return;
        }

        // Find which tower we're over
        int targetTowerIndex = FindTowerUnderPointer(eventData.position);
        
        if (targetTowerIndex >= 0 && targetTowerIndex != currentTowerIndex)
        {
            // Try to move to new tower
            Transform targetTower = GameManager2.Instance.towers[targetTowerIndex];
            if (GameManager2.Instance.TryMove(this, targetTower))
            {
                // Success - position will be updated by GameManager
                return;
            }
        }

        // Invalid move or same tower: return to original position
        ReturnToStartPosition();
        GameManager2.Instance.RegisterDiskAtTower(this, startTower);
    }

    private int FindTowerUnderPointer(Vector2 screenPosition)
    {
        // Check which tower rect contains the pointer
        for (int i = 0; i < GameManager2.Instance.towers.Length; i++)
        {
            if (GameManager2.Instance.towers[i] == null) continue;

            RectTransform towerRect = GameManager2.Instance.towers[i].GetComponent<RectTransform>();
            if (towerRect == null) continue;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                towerRect,
                screenPosition,
                canvas.worldCamera,
                out localPoint
            );

            if (towerRect.rect.Contains(localPoint))
            {
                return i;
            }
        }
        return -1;
    }

    private void ReturnToStartPosition()
    {
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startPosition;
    }

    public void SetPosition(Vector2 newPosition)
    {
        rectTransform.anchoredPosition = newPosition;
    }
}