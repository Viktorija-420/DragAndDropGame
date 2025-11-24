using UnityEngine;

public class WinCheck : MonoBehaviour
{
    public Transform tower3;

    void Update()
    {
        if (tower3.childCount == 6)
        {
            bool correctOrder = true;
            for (int i = 0; i < tower3.childCount - 1; i++)
            {
                if (tower3.GetChild(i).GetComponent<RectTransform>().sizeDelta.x < tower3.GetChild(i + 1).GetComponent<RectTransform>().sizeDelta.x)
                {
                    correctOrder = false;
                    break;
                }
            }

            if (correctOrder)
            {
                Debug.Log("You won!");
            }
        }
    }
}
