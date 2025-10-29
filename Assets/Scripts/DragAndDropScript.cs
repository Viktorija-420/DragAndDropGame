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
    private Vector2 dragOffset;
    private Camera uiCamera;
    private Canvas canva;
    private int originalSiblingIndex;

    // Start is called before the first frame update
    void Start()
    {
        canvasGro = GetComponent<CanvasGroup>();
        rectTra = GetComponent<RectTransform>();

        if (objectScr == null)
        {
            objectScr = Object.FindFirstObjectByType<ObjectScript>();
        }
        if (screenBou == null)
        {
            screenBou = Object.FindFirstObjectByType<ScreenBoundaries>();
        }
        
        canva = GetComponentInParent<Canvas>();
        if (canva != null)
        {
            uiCamera = canva.worldCamera;
            if (uiCamera == null)
            {
                uiCamera = Camera.main;
            }
        }
        else
        {
            uiCamera = Camera.main;
            Debug.LogWarning("Canvas not found, using main camera");
        }

        // Store original transform values
        originalPosition = rectTra.localPosition;
        originalRotation = rectTra.localRotation;
        originalScale = rectTra.localScale;
        originalSiblingIndex = transform.GetSiblingIndex();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPlacedCorrectly)
        {
            Debug.Log("OnPointerDown");
            if (objectScr != null && objectScr.audioCli != null && objectScr.audioCli.Length > 0)
            {
                objectScr.effects.PlayOneShot(objectScr.audioCli[0]);
            }
            
            // Calculate the offset between pointer position and object position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTra.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointerPosition);
            
            dragOffset = (Vector2)rectTra.localPosition - localPointerPosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isPlacedCorrectly)
        {
            ObjectScript.drag = true;
            canvasGro.blocksRaycasts = false;
            canvasGro.alpha = 0.6f;

            // Move to top of hierarchy for dragging
            transform.SetAsLastSibling();

            ObjectScript.lastDragged = eventData.pointerDrag;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPlacedCorrectly)
        {
            // Convert screen position to local position within the parent
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTra.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointerPosition))
            {
                // Apply the offset to keep the object under the cursor/finger
                Vector2 newPosition = localPointerPosition + dragOffset;
                
                if (screenBou != null)
                {
                    screenBou.RecalculateBounds();
                    Vector2 clamped = screenBou.GetClampedPosition(newPosition);
                    rectTra.localPosition = new Vector3(clamped.x, clamped.y, rectTra.localPosition.z);
                }
                else
                {
                    rectTra.localPosition = new Vector3(newPosition.x, newPosition.y, rectTra.localPosition.z);
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isPlacedCorrectly)
        {
            ObjectScript.drag = false;
            canvasGro.alpha = 1f;

            // Only disable raycasts and count as placed if it's in the right place
            if (objectScr != null && objectScr.rightPlace)
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
                // Reset to original hierarchy position
                transform.SetSiblingIndex(originalSiblingIndex);
            }

            // Reset the flag for next drag
            if (objectScr != null)
            {
                objectScr.rightPlace = false;
            }
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
        transform.SetSiblingIndex(originalSiblingIndex);
    }

    // Method to mark this object as correctly placed
    public void SetPlacedCorrectly()
    {
        isPlacedCorrectly = true;
        canvasGro.blocksRaycasts = false;
        canvasGro.alpha = 1f;
    }
}