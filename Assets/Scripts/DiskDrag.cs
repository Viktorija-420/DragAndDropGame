using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class DiskDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("1 = smallest, larger number = bigger disk")]
    public int diskSize = 1;

    [Tooltip("Set false if this disk should not be draggable")]
    public bool draggable = true;

    [HideInInspector]
    public int currentTowerIndex = -1;

    private Rigidbody2D rb;
    private Collider2D col;
    private Camera cam;

    private Vector2 pointerOffset;
    private Vector2 startPosition;
    private int startTower;

    // Smooth snapping
    private Vector3 targetPosition;
    private bool snapping = false;
    public float snapSpeed = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        cam = Camera.main;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        targetPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (snapping)
        {
            rb.MovePosition(Vector3.MoveTowards(rb.position, targetPosition, snapSpeed * Time.fixedDeltaTime));
            if ((Vector2)rb.position == (Vector2)targetPosition)
                snapping = false;
        }
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
        snapping = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        if (!GameManager2.Instance.IsTopDisk(this)) return;

        startPosition = rb.position;
        startTower = currentTowerIndex;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.isKinematic = true;
        col.enabled = false;

        Vector2 pointerWorld = cam.ScreenToWorldPoint(eventData.position);
        pointerOffset = (Vector2)rb.position - pointerWorld;

        snapping = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        if (!GameManager2.Instance.IsTopDisk(this)) return;

        Vector2 pointerWorld = cam.ScreenToWorldPoint(eventData.position);
        Vector2 targetPos = pointerWorld + pointerOffset;
        rb.MovePosition(targetPos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!draggable)
        {
            rb.position = startPosition;
            return;
        }

        Transform closest = GameManager2.Instance.FindClosestTower(rb.position);
        if (closest != null && GameManager2.Instance.TryMove(this, closest))
        {
            // Snap handled in TryMove
        }
        else
        {
            // Invalid move: revert smoothly
            SetTargetPosition(startPosition);
            GameManager2.Instance.RegisterDiskAtTower(this, startTower);
        }

        rb.isKinematic = false;
        col.enabled = true;
    }
}
