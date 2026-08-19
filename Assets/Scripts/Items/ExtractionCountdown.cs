using UnityEngine;
using TMPro;

public class ExtractionCountdown : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI timerText;

    float totalTime;
    float currentTime;

    private void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    public void SetTimer(float time)
    {
        totalTime = time;
        currentTime = totalTime;
    }

    void Update()
    {
        if (gameManager.instance.isExtracting && totalTime >= 0 && !gameManager.instance.isPaused)
        {
            currentTime -= Time.deltaTime;

            currentTime = Mathf.Clamp(currentTime, 0, totalTime);

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