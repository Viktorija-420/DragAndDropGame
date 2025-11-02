using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class TransformationScript : MonoBehaviour
{
    // Scale limits that match the ObjectScript ranges
    public float rotationSpeed = 90f;
    public float scaleSpeed = .5f;
    private bool rotateCW, rotateCCW, scaleUpY, scaleDownY, scaleUpX, scaleDownX;
    public static bool isTransforming = false;
    private float minScale = 0.5f;
    private float maxScale = 1.2f;

    void Update()
    {
        if (ObjectScript.lastDragged == null)
            return;
        RectTransform rt = ObjectScript.lastDragged.GetComponent<RectTransform>();

        if (rotateCW)
            rt.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        if (rotateCCW)
            rt.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        if (scaleUpY && rt.localScale.y < maxScale)
            rt.localScale += new Vector3(0, scaleSpeed * Time.deltaTime, 0);
        if (scaleDownY && rt.localScale.y > minScale)
            rt.localScale -= new Vector3(0, scaleSpeed * Time.deltaTime, 0);

        if (scaleUpX && rt.localScale.x < maxScale)
            rt.localScale += new Vector3(scaleSpeed * Time.deltaTime, 0, 0);
        if (scaleDownX && rt.localScale.x > minScale)
            rt.localScale -= new Vector3(scaleSpeed * Time.deltaTime, 0, 0);

        isTransforming = rotateCW || rotateCCW || scaleUpY || scaleDownY || scaleUpX || scaleDownX;
    }
    public void StartRotateCW(BaseEventData data) { rotateCW = true; }
    public void StopRotateCW(BaseEventData data) { rotateCW = false; }
    public void StartRotateCCW(BaseEventData data) { rotateCCW = true; }
    public void StopRotateCCW(BaseEventData data) { rotateCCW = false; }

    public void StartScaleUpY(BaseEventData data) { scaleUpY = true; }
    public void StopScaleUpY(BaseEventData data) { scaleUpY = false; }

    public void StartScaleDownY(BaseEventData data) { scaleDownY = true; }
    public void StopScaleDownY(BaseEventData data) { scaleDownY = false; }

    public void StartScaleUpX(BaseEventData data) { scaleUpX = true; }
    public void StopScaleUpX(BaseEventData data) { scaleUpX = false; }

    public void StartScaleDownX(BaseEventData data) { scaleDownX = true; }
    public void StopScaleDownX(BaseEventData data) { scaleDownX = false; }
}