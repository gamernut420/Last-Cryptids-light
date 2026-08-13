using UnityEngine;
using TMPro;

public class ExtractionCountdown : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] float timeRemaining = 300f;
    bool timerIsRunning = false;

    [Header("UI References")]
    [SerializeField]TextMeshProUGUI timerText;


    [Header("References")]
    [SerializeField] gameManager gameManager;
    [SerializeField] EnemyDirector enemyDirector;

    // Update is called once per frame
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

                // Trigger the win condition throught thr game manager
                if (gameManager != null)
                {
                    // gameManager.TriggerWin();
                }
            }
        }
    }

    public void StartExtractionTimer()
    {
        timerIsRunning = true;
        if(timerText != null) timerText.gameObject.SetActive(true);

        //if (enemyDirector != null) enemyDirector.StartDefendPhase();
        
    }

    void UpdateTimerDisplay(float currentTime)
    {
        currentTime += 1;
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        if (timerText != null) timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

    }



}
