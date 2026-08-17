using UnityEngine;
using TMPro;

public class ExtractionCountdown : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("This is in seconds")]
    [SerializeField] float timeRemaining = 10f;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI timerText;

    float currentTime;

    private void Start()
    {
        currentTime = timeRemaining;

        timerText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (gameManager.instance.isExtracting && timeRemaining >= 0)
        {
            currentTime -= Time.deltaTime;

            currentTime = Mathf.Clamp(currentTime, 0, timeRemaining);

            UpdateTimerDisplay(currentTime);

            if (currentTime == 0)
            {
                gameManager.instance.isExtracting = false;
                gameManager.instance.extractionWin();
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
}