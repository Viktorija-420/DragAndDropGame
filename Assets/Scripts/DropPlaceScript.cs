using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, vehicleZRot, rotDiff;
    private Vector3 placeSiz, vehicleSiz;
    private float xSizeDiff, ySizeDiff;
    public ObjectScript objScript;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || !Input.GetMouseButtonUp(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            return;

        // Check if tags match
        if (eventData.pointerDrag.tag.Equals(this.tag))
        {
            // Get the dragged vehicle's rotation and the target place's rotation
            vehicleZRot = eventData.pointerDrag.GetComponent<RectTransform>().eulerAngles.z;
            placeZRot = GetComponent<RectTransform>().eulerAngles.z;

            // Normalize rotation to 0-360 range and calculate difference
            rotDiff = Mathf.Abs(vehicleZRot - placeZRot);
            rotDiff = Mathf.Min(rotDiff, 360 - rotDiff); // Get the smallest angle difference

            Debug.Log("Rotation difference: " + rotDiff);

            // Get the dragged vehicle's scale and the target place's scale
            vehicleSiz = eventData.pointerDrag.GetComponent<RectTransform>().localScale;
            placeSiz = GetComponent<RectTransform>().localScale;
            xSizeDiff = Mathf.Abs(vehicleSiz.x - placeSiz.x);
            ySizeDiff = Mathf.Abs(vehicleSiz.y - placeSiz.y);

            Debug.Log("X size difference: " + xSizeDiff);
            Debug.Log("Y size difference: " + ySizeDiff);

            // Check if close enough to snap (more generous thresholds)
            if ((rotDiff <= 25f) && (xSizeDiff <= 0.4f && ySizeDiff <= 0.4f))
            {
                Debug.Log("Correct place - auto-snapping");
                ObjectScript.carsCorrectlyPlaced++;
                ObjectScript.carsLeft--;
                // Get the ObjectScript from the dragged object, not this drop place
                ObjectScript draggedObjScript = eventData.pointerDrag.GetComponent<ObjectScript>();
                if (draggedObjScript != null)
                {
                    draggedObjScript.rightPlace = true;
                }

                // Snap to exact position, rotation and scale of the target
                RectTransform vehicleRect = eventData.pointerDrag.GetComponent<RectTransform>();
                RectTransform targetRect = GetComponent<RectTransform>();

                // Use Transform methods for more reliable snapping
                vehicleRect.position = targetRect.position;
                vehicleRect.rotation = targetRect.rotation;
                vehicleRect.localScale = targetRect.localScale;

                // Also set anchored position for UI elements
                vehicleRect.anchoredPosition = targetRect.anchoredPosition;
                vehicleRect.localRotation = targetRect.localRotation;

                // Play correct sound based on tag
                PlayCorrectSound(eventData.pointerDrag.tag);
                ObjectScript.lastDragged = null;
                ObjectScript.drag = false;
                // Disable dragging for this object since it's correctly placed
                DragAndDropScript dragScript = eventData.pointerDrag.GetComponent<DragAndDropScript>();
                if (dragScript != null)
                {
                    dragScript.SetPlacedCorrectly();
                }
            }
            else
            {
                // Close but not close enough - don't snap yet
                Debug.Log("Close but not close enough to snap");

                // Get the ObjectScript from the dragged object
                ObjectScript draggedObjScript = eventData.pointerDrag.GetComponent<ObjectScript>();
                if (draggedObjScript != null)
                {
                    draggedObjScript.rightPlace = false;
                    // objScript.effects.PlayOneShot(objScript.audioCli[1]); // Play error sound
                }

            }
        }
        else
        {
            // Wrong drop target - play error sound and reset position
            Debug.Log("Incorrect place");

            // Get the ObjectScript from the dragged object
            objScript.effects.PlayOneShot(objScript.audioCli[1]);
            ObjectScript draggedObjScript = eventData.pointerDrag.GetComponent<ObjectScript>();
            if (draggedObjScript != null)
            {
                draggedObjScript.rightPlace = false;
            }

            ResetVehiclePosition(eventData.pointerDrag);
        }
    }

    private void PlayCorrectSound(string tag)
    {
        switch (tag)
        {
            case "Garbage":
                objScript.effects.PlayOneShot(objScript.audioCli[7]);
                break;
            case "Medicine":
                objScript.effects.PlayOneShot(objScript.audioCli[3]);
                break;
            case "Fire":
                objScript.effects.PlayOneShot(objScript.audioCli[4]);
                break;
            case "School":
                objScript.effects.PlayOneShot(objScript.audioCli[10]); //
                break;
            case "Policija":
                objScript.effects.PlayOneShot(objScript.audioCli[11]);
                break;
            case "B2":
                objScript.effects.PlayOneShot(objScript.audioCli[5]);
                break;
            case "Cements":
                objScript.effects.PlayOneShot(objScript.audioCli[12]);
                break;
            case "e46":
                objScript.effects.PlayOneShot(objScript.audioCli[6]);
                break;
            case "e61":
                objScript.effects.PlayOneShot(objScript.audioCli[9]);
                break;
            case "Eskavator":
                objScript.effects.PlayOneShot(objScript.audioCli[13]);
                break;
            case "Traktor":
                objScript.effects.PlayOneShot(objScript.audioCli[7]);
                break;
            case "Traktor2":
                objScript.effects.PlayOneShot(objScript.audioCli[10]);
                break;
            default:
                Debug.Log("Unknown tag: " + tag);
                break;
        }
    }

    private void ResetVehiclePosition(GameObject vehicle)
    {
        // Try to use the new ResetToOriginalPosition method first
        DragAndDropScript dragScript = vehicle.GetComponent<DragAndDropScript>();
        if (dragScript != null)
        {
            dragScript.ResetToOriginalPosition();
        }
        else
        {
            // Fallback to original method
            for (int i = 0; i < objScript.vehicles.Length; i++)
            {
                if (objScript.vehicles[i].tag == vehicle.tag)
                {
                    objScript.vehicles[i].GetComponent<RectTransform>().localPosition = ObjectScript.startCoordinates[i];
                    break;
                }
            }
        }
    }
}
// switch (eventData.pointerDrag.tag)
//                 {
//                     case "Garbage":
//                         objScript.vehicles[0].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[0];
//                         break;

//                     case "Medicine":
//                         objScript.vehicles[1].GetComponent<RectTransform>().localPosition =
//                            ObjectScript.startCoordinates[1];
//                         break;

//                     case "Fire":
//                         objScript.vehicles[2].GetComponent<RectTransform>().localPosition =
//                            ObjectScript.startCoordinates[2];
//                         break;

//                     case "School":
//                         objScript.vehicles[3].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[3];
//                         break;

//                     case "B2":
//                         objScript.vehicles[4].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[4];
//                         break;

//                     case "Cements":
//                         objScript.vehicles[5].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[5];
//                         break;

//                     case "e46":
//                         objScript.vehicles[6].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[6];
//                         break;

//                     case "e61":
//                         objScript.vehicles[7].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[7];
//                         break;

//                     case "Eskavator":
//                         objScript.vehicles[8].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[8];
//                         break;

//                     case "Policija":
//                         objScript.vehicles[9].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[9];
//                         break;

//                     case "Traktor":
//                         objScript.vehicles[10].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[10];
//                         break;

//                     case "Traktor2":
//                         objScript.vehicles[11].GetComponent<RectTransform>().localPosition =
//                             ObjectScript.startCoordinates[11];
//                         break;

//                     default:
//                         Debug.Log("Error: #67. Unknown tag detected! ");
//                         break;
//                 }