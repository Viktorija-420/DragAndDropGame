using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text timerText;
    private float timer = 0f;
    private bool isRunning = true;

    void Update()
    {
        if (!isRunning)
            return;

        timer += Time.deltaTime;

        int hours = Mathf.FloorToInt(timer / 3600f);
        int minutes = Mathf.FloorToInt((timer % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public void ResetTimer()
    {
        timer = 0f;
        isRunning = true;
        timerText.text = "00:00:00";
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    // Get the current timer value
    public float GetCurrentTime()
    {
        return timer;
    }
}