using UnityEngine;
using UnityEngine.EventSystems;


public class SnapToTarget : MonoBehaviour, IEndDragHandler
{
public RectTransform targetRect; // tiks iestatīts no PlacementRandomizer
public float snapDistance = 50f; // pikseļos (UI coordinates) - pielāgo pēc vajadzības


private RectTransform rectTra;
private ObjectScript objScr;
private Canvas parentCanvas;


void Awake()
{
rectTra = GetComponent<RectTransform>();
objScr = GetComponent<ObjectScript>();
parentCanvas = GetComponentInParent<Canvas>();
}


public void OnEndDrag(PointerEventData eventData)
{
if (targetRect == null || rectTra == null) return;


// salīdzinām atstarpes izvērtējot anchoredPosition (Canvas viet coordinate sistēma)
float dist = Vector2.Distance(rectTra.anchoredPosition, targetRect.anchoredPosition);
if (dist <= snapDistance)
{
// snap
rectTra.anchoredPosition = targetRect.anchoredPosition;
if (objScr != null)
{
objScr.rightPlace = true;
}


// opcija: padarīt objektu nespēlējošu (nevar vilkt)
var cg = GetComponent<CanvasGroup>();
if (cg != null)
{
cg.blocksRaycasts = false;
}
}
else
{
if (objScr != null)
objScr.rightPlace = false;
}
}
}