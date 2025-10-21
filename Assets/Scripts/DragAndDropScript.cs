using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler,
    IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGro;
    private RectTransform rectTra;
    public ObjectScript objectScr;
    public ScreenBoundaries screenBou;
    public ObjectScript objScript;

    // Store original transform for resetting if needed
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private bool isPlacedCorrectly = false;

    // Start is called before the first frame update
    void Start()
    {
        canvasGro = GetComponent<CanvasGroup>();
        rectTra = GetComponent<RectTransform>();

        // Store original transform values
        originalPosition = rectTra.localPosition;
        originalRotation = rectTra.localRotation;
        originalScale = rectTra.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2) && !isPlacedCorrectly)
        {
            Debug.Log("OnPointerDown");
            objectScr.effects.PlayOneShot(objectScr.audioCli[0]);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2) && !isPlacedCorrectly)
        {
            ObjectScript.drag = true;
            canvasGro.blocksRaycasts = false;
            canvasGro.alpha = 0.6f;

            int positionIndex = transform.parent.childCount - 1;
            int position = Mathf.Max(0, positionIndex - 1);
            transform.SetSiblingIndex(position);

            Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBou.screenPoint.z));
            rectTra.position = cursorWorldPos;

            screenBou.screenPoint = Camera.main.WorldToScreenPoint(rectTra.localPosition);

            screenBou.offset = rectTra.localPosition -
                Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                screenBou.screenPoint.z));

            ObjectScript.lastDragged = eventData.pointerDrag;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2) && !isPlacedCorrectly)
        {
            Vector3 curSreenPoint =
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBou.screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curSreenPoint) + screenBou.offset;
            rectTra.position = screenBou.GetClampedPosition(curPosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Input.GetMouseButtonUp(0) && !isPlacedCorrectly)
        {
            ObjectScript.drag = false;
            canvasGro.alpha = 1f;

            // Only disable raycasts and count as placed if it's in the right place
            if (objectScr.rightPlace)
            {
                isPlacedCorrectly = true;
                canvasGro.blocksRaycasts = false;
                ObjectScript.lastDragged = null;
                ObjectScript.carsLeft--;
                ObjectScript.carsCorrectlyPlaced++;
                Debug.Log($"Cars correctly placed: {ObjectScript.carsCorrectlyPlaced}, cars left: {ObjectScript.carsLeft}");
            }
            else
            {
                // If not in right place, re-enable raycasts for future dragging
                canvasGro.blocksRaycasts = true;
            }

            // Reset the flag for next drag
            objectScr.rightPlace = false;
        }
    }

    // Public method to reset vehicle to original position
    public void ResetToOriginalPosition()
    {
        rectTra.localPosition = originalPosition;
        rectTra.localRotation = originalRotation;
        rectTra.localScale = originalScale;
        canvasGro.blocksRaycasts = true;
        canvasGro.alpha = 1f;
        isPlacedCorrectly = false;
    }

    // Method to mark this object as correctly placed
    public void SetPlacedCorrectly()
    {
        isPlacedCorrectly = true;
        canvasGro.blocksRaycasts = false;
        canvasGro.alpha = 1f;
    }
}