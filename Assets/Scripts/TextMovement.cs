using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextMovement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Text buttonText;
    public float moveDis = 10f;

    private Vector3 originalPos;

    private void Start()
    {
        if (buttonText != null)
            originalPos = buttonText.rectTransform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buttonText != null)
            buttonText.rectTransform.localPosition = originalPos + Vector3.down * moveDis;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buttonText != null)
            buttonText.rectTransform.localPosition = originalPos;
    }
}
