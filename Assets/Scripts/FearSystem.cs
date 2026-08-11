using System.ComponentModel.Design;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class FearSystem : MonoBehaviour
{

    [Header("Fear Sysstem")]
    [Range(1, 100)][SerializeField] float maxFear;
    [SerializeField] float currentFear;
    [SerializeField] float fearAccumulationRate;
    [SerializeField] float calmRate;

    [Header("UI Reference")]
    [SerializeField] TextMeshProUGUI fearText;
   


    [Header("Reference")]
    [SerializeField] FlashlightController flashlightController;

    [Header("State")]
    public bool isInsane = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentFear = 0f;
        isInsane = false;
        UpdateFearUI();
       
    }

    // Update is called once per frame
    void Update()
    {
        if (isInsane) return;

        Light playerLight = flashlightController != null ? flashlightController.GetComponentInChildren<Light>() : null;
        bool isLightActive = playerLight != null && playerLight.enabled;

        if (isLightActive) DecreaseFear(calmRate * Time.deltaTime);
        else IncreaseFear(fearAccumulationRate * Time.deltaTime);

        if (currentFear >= maxFear && !isInsane) TriggerPanicState();

        // Check if player has completely lost sanity
        if (currentFear >= maxFear && !isInsane) TriggerPanicState();
    }
    void IncreaseFear(float amount)
    {
        currentFear += amount;
        currentFear = Mathf.Clamp(currentFear, 0f, maxFear);
        UpdateFearUI();
    }
    void DecreaseFear(float amount)
    {
        currentFear -= amount;
        currentFear = Mathf.Clamp(currentFear, 0f, maxFear);
        UpdateFearUI();
    }
    void UpdateFearUI()
    {
        if (fearText != null) fearText.text = "Fear: " + Mathf.Round(currentFear) + "%";
    }
    void TriggerPanicState()
    {
        isInsane = true;
        Debug.Log("Player has reached maximun psycholgical fear/insanity!");

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        // Change the game over message text
       // if (gameManager != null) gameManager.TriggerGameOver("You Went Insane!");

    }
}
