using UnityEngine;
using TMPro;
using JetBrains.Annotations;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] AudioSource flashlightAudio;
    [SerializeField] AudioClip soundOn;
    [SerializeField] AudioClip soundOff;
    [SerializeField] Light flashlightLight;
    [Range(1f, 100f)][SerializeField] float maxBattery;
    [Range(1f, 100f)][SerializeField] float currentBattery;
    [Range(1f, 10f)][SerializeField] float drainRate;
    [SerializeField] float raycastRange;
    [SerializeField] LayerMask enemyLayer;

    [Header("UI Reference")]
    [SerializeField] TextMeshProUGUI batteryText;

    private bool isOn = false;
    private bool isLockedOut = false;
    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentBattery = maxBattery;
        if (flashlightLight != null) flashlightLight.enabled = false;

        if(batteryText == null)
        {
            batteryText = GameObject.Find("BatteryData").GetComponent<TextMeshProUGUI>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        // Toggle input (Press 'F' 
        if (Input.GetKeyDown(KeyCode.F)) ToggleFlashlight();
        // Handle battery drian when active
        if(isOn)
        {
            CheckForEnemy();

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
        // If it's locked out because battery is dead, prevent turning it back on
        if (isLockedOut && currentBattery <= 0f)
        {
            Debug.Log("Bettery is dead! Find a battery to recharge.");
            return;

        }
        isOn = !isOn;
        if (flashlightLight != null) flashlightLight.enabled = isOn;

        if (flashlightAudio != null)
        {
            if (isOn)
            {
                flashlightAudio.PlayOneShot(soundOn);
            }
            else
            {
                flashlightAudio.PlayOneShot(soundOff);
            }
        }
    }


    void TurnOffFlashLigh()
    {
        isOn = false;
        if (flashlightLight != null) flashlightLight.enabled = false; 
        if (flashlightAudio != null && soundOff != null)
            flashlightAudio.PlayOneShot(soundOff);
    }

    void UpdateBatteryUI()
    {
        if(batteryText != null)
        {
            // Displays rounded battery percentage on screen
           batteryText.text = Mathf.Round(currentBattery) + "%";
        }
    }

    public void RechargeBattery(float amount)
    {
        currentBattery += amount;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);
        if (currentBattery > 0f) isLockedOut = false; // Remove lockout once recharged
        UpdateBatteryUI(); 
    }

    void CheckForEnemy()
    {
        RaycastHit hit;
        Vector3 forward = transform.forward;

        if (Physics.Raycast(transform.position, forward, out hit, raycastRange, enemyLayer))
        {
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            EnemyAI_EyesOnly eyesEnemy = hit.collider.GetComponent<EnemyAI_EyesOnly>();
            
            if (enemy != null)
            {
                enemy.stun = 0;
                enemy.afterAttack = 0;
                enemy.attackTimer = 0;
                enemy.attack = false;
                enemy.stalk = false;
                enemy.flee = false;
                return;
            }

            if (eyesEnemy != null)
            {
                if (eyesEnemy.isWaiting)
                {
                    eyesEnemy.ApplyFlashlightStun(5f);
                }
                return;
            }

        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawRay(transform.position, transform.forward * raycastRange);
    }
}
