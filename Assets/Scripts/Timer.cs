using UnityEngine;
using UnityEngine.UI;

public class AutoStartTimer : MonoBehaviour
{
    public Text timerText;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        int hours = Mathf.FloorToInt(timer / 3600f);
        int minutes = Mathf.FloorToInt((timer % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
}

