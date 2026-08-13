using UnityEngine;
using TMPro;

public class ExtractionCountdown : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] float timeRemaining = 120f;
    private bool timerIsRunning = false;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI timerText;

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
    }

    void UpdateTimerDisplay(float currentTime)
    {
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void StartExtractionTimer()
    {
        timerIsRunning = true;
    }
}