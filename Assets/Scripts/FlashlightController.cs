using UnityEngine;
using TMPro;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] Light flashlightLight;
    [Range(1f, 100f)][SerializeField] float maxBattery;
    [Range(1f, 100f)][SerializeField] float currentBattery;
    [Range(1f, 10f)][SerializeField] float drainRate;

    private bool isOn = false;
    private bool isLockedOut = false;

    [Header("UI Reference")]
    [SerializeField] TextMeshProUGUI batteryText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentBattery = maxBattery;
        if (flashlightLight != null) flashlightLight.enabled = false;

        if(batteryText == null)
        {
            batteryText = GameObject.Find("BatteryText").GetComponent<TextMeshProUGUI>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        // Toggle input (Press 'F' 
        if (Input.GetKeyDown(KeyCode.F)) ToggleFlashlight();
        // Hanle battery drian when active
        if(isOn)
        {
            currentBattery -= drainRate * Time.deltaTime;
            currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

            UpdateBatteryUI();

            Debug.Log("Battery: " + currentBattery);

            // Auto-shutdown if battery hits 0

            if (currentBattery <= 0f)
            {
                TurnOffFlashLigh();
                isLockedOut = true;
                Debug.Log("Flashlight dead and lock out!");
            }
        }

        ////////////// Testing RechargeBattery(); Pree key 'B' to test

        if (Input.GetKeyDown(KeyCode.B)) RechargeBattery(maxBattery);

    }
    void ToggleFlashlight()
    {
        // If it's locked out because battery is dead, prevent turnig it back on
        if (isLockedOut && currentBattery <= 0f)
        {
            Debug.Log("Bettery is dead! Find a battery to recharge.");
            return;

        }
        isOn = !isOn;
        if (flashlightLight != null) flashlightLight.enabled = isOn;
    }
    void TurnOffFlashLigh()
    {
        isOn = false;
        if (flashlightLight != null) flashlightLight.enabled = false; 
    }

    void UpdateBatteryUI()
    {
        if(batteryText != null)
        {
            // Displays rounded battery percentage on screen
           batteryText.text = "Battery: " + Mathf.Round(currentBattery) + "%";
        }
    }

    public void RechargeBattery(float amount)
    {
        currentBattery += amount;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);
        if (currentBattery > 0f) isLockedOut = false; // Remove lockout once recharged
        UpdateBatteryUI(); 
    }

}
