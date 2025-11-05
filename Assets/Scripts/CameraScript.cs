using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraScript : MonoBehaviour
{
    private float maxZoom;
    public float minZoom = 150f;

    private float startZoom;
    public Camera cam;
    public float pinchZoomSpeed = 0.9f;
    public float mouseZoomSpeed = 150f;
    public float mouseFollowSpeed = 1f;
    public float touchPanSpeed = 1f;
    public ScreenBoundaries screenBoundries;

    private Vector2 lastTouchPos;
    private int panFingerId = -1;
    private bool isTouchPaning = false;
    private float lastTapTime = 0f;

    public float doubleTapMaxDelay = 0.4f;
    public float doubleTapMaxDistance = 100f;

    private void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        // Ensure we always have a valid ScreenBoundaries reference
#if UNITY_2023_1_OR_NEWER
        if (screenBoundries == null)
            screenBoundries = FindFirstObjectByType<ScreenBoundaries>();
#else
        if (screenBoundries == null)
            screenBoundries = FindObjectOfType<ScreenBoundaries>();
#endif

        if (screenBoundries == null)
            Debug.LogWarning("⚠️ CameraScript: No ScreenBoundaries object found in scene!");
    }

    private void Start()
    {
        startZoom = cam.orthographicSize;

        if (screenBoundries != null)
        {
            screenBoundries.RecalculateBounds();
            transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
        }
    }

    private void Update()
    {
        // Skip updates if transforming (if TransformationScript exists)
        if (typeof(TransformationScript).GetField("isTransforming") != null &&
            (bool)typeof(TransformationScript).GetField("isTransforming").GetValue(null))
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        DesktopFollowCursor();
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
            cam.orthographicSize -= scroll * mouseZoomSpeed;
#else
        HandleTouch();
#endif

        UpdateMaxZoom();
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

        // ✅ Safe boundary recalculation
        if (screenBoundries == null)
        {
#if UNITY_2023_1_OR_NEWER
            screenBoundries = FindFirstObjectByType<ScreenBoundaries>();
#else
            screenBoundries = FindObjectOfType<ScreenBoundaries>();
#endif
            if (screenBoundries == null)
                return; // skip frame if still missing
        }

        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    private void DesktopFollowCursor()
    {
        Vector3 mouse = Input.mousePosition;
        if (mouse.x < 0 || mouse.x > Screen.width || mouse.y < 0 || mouse.y > Screen.height)
            return;

        Vector3 screenPoint = new Vector3(mouse.x, mouse.y, cam.nearClipPlane);
        Vector3 targetWorld = cam.ScreenToWorldPoint(screenPoint);
        Vector3 desired = new Vector3(targetWorld.x, targetWorld.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, mouseFollowSpeed * Time.deltaTime);
    }

    private void HandleTouch()
    {
        if (Input.touchCount == 0)
            return;

        Touch t = Input.GetTouch(0);

        if (IsTouchingPaningOverUIButton(t.position))
            return;

        if (t.phase == TouchPhase.Began)
        {
            float dt = Time.time - lastTapTime;
            if (dt <= doubleTapMaxDelay && Vector2.Distance(t.position, lastTouchPos) <= doubleTapMaxDistance)
            {
                StartCoroutine(ResetZoomSmooth());
                lastTapTime = 0f;
            }
            else
            {
                lastTapTime = Time.time;
            }

            lastTouchPos = t.position;
            panFingerId = t.fingerId;
            isTouchPaning = true;
        }
        else if (t.phase == TouchPhase.Moved && isTouchPaning && t.fingerId == panFingerId)
        {
            Vector2 delta = t.deltaPosition;
            transform.Translate(ScreenDeltaToWorldDelta(delta) * touchPanSpeed, Space.World);
            lastTouchPos = t.position;
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            isTouchPaning = false;
            panFingerId = -1;
        }

        if (Input.touchCount == 2)
        {
            HandlePinch();
        }
    }

    private bool IsTouchingPaningOverUIButton(Vector2 touchPos)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = touchPos
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<Button>() != null)
                return true;
        }

        return false;
    }

    private void HandlePinch()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 t0PrevPos = t0.position - t0.deltaPosition;
        Vector2 t1PrevPos = t1.position - t1.deltaPosition;

        float prevDist = Vector2.Distance(t0PrevPos, t1PrevPos);
        float currDist = Vector2.Distance(t0.position, t1.position);
        cam.orthographicSize -= (prevDist - currDist) * pinchZoomSpeed;
    }

    private Vector3 ScreenDeltaToWorldDelta(Vector2 screenDelta)
    {
        float worldPerPixel = (2f * cam.orthographicSize) / Screen.height;
        return new Vector3(-screenDelta.x * worldPerPixel, -screenDelta.y * worldPerPixel, 0f);
    }

    private void UpdateMaxZoom()
    {
        if (screenBoundries == null || cam == null)
            return;

        Rect wb = screenBoundries.worldBounds;
        float maxZoomHeight = wb.height / 2f;
        float maxZoomWidth = (wb.width / 2f) / cam.aspect;
        maxZoom = Mathf.Min(maxZoomHeight, maxZoomWidth);
    }

    private System.Collections.IEnumerator ResetZoomSmooth()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        float initialZoom = cam.orthographicSize;

        float targetZoom = maxZoom;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.orthographicSize = Mathf.Lerp(initialZoom, targetZoom, elapsed / duration);

            if (screenBoundries != null)
            {
                screenBoundries.RecalculateBounds();
                transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
            }

            yield return null;
        }

        cam.orthographicSize = targetZoom;

        if (screenBoundries != null)
        {
            screenBoundries.RecalculateBounds();
            transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
        }
    }

    // ✅ Added this so GameManager can call ResetCamera()
    public void ResetCamera()
    {
        StopAllCoroutines();
        StartCoroutine(ResetZoomSmooth());

        if (screenBoundries != null)
        {
            transform.position = screenBoundries.GetClampedCameraPosition(Vector3.zero);
        }
    }
}