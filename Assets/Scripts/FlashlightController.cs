using UnityEngine;
using TMPro;
using UnityEngine.UI; // ADDED - needed for the new flashlight meter Images

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] AudioSource flashlightAudio;
    [SerializeField] AudioClip soundOn;
    [SerializeField] AudioClip soundOff;
    public float toggleHearingRadius = 2f;
    [SerializeField] Light flashlightLight;
    [Range(1f, 100f)][SerializeField] float maxBattery;
    [Range(1f, 100f)][SerializeField] float currentBattery;
    [Range(0f, 10f)][SerializeField] float drainRate;
    [SerializeField] float raycastRange;
    public float stunTime;
    public float requiredColliderTime = 1.0f;
    private float hitTimer = 0.0f;
    private Transform currentEnemy = null;
    [SerializeField] LayerMask enemyLayer;

    [Header("UI Reference")]
    [SerializeField] TextMeshProUGUI batteryText;

    // ADDED - four sections of the new vertical flashlight meter
    [Header("Flashlight HUD Meter")]
    [SerializeField] private Image segment1; // Bottom
    [SerializeField] private Image segment2;
    [SerializeField] private Image segment3;
    [SerializeField] private Image segment4; // Top

    // ADDED - meter colors
    [SerializeField] private Color normalColor = new Color32(244, 251, 255, 255);
    [SerializeField] private Color warningColor = new Color32(255, 200, 61, 255);
    [SerializeField] private Color criticalColor = new Color32(255, 59, 48, 255);
    [SerializeField] private Color emptyColor = new Color32(8, 11, 13, 255);

    private bool isOn = false;
    private bool isLockedOut = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentBattery = maxBattery;

        if (flashlightLight != null)
            flashlightLight.enabled = false;

        if (batteryText == null)
        {
            GameObject batteryObject = GameObject.Find("BatteryData");

            if (batteryObject != null)
            {
                batteryText = batteryObject.GetComponent<TextMeshProUGUI>();
            }
        }

        // ADDED - make sure both battery displays start correctly
        UpdateBatteryUI();
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle input (Press 'F')
        if (Input.GetKeyDown(KeyCode.F))
            ToggleFlashlight();

        // Handle battery drain when active
        if (isOn)
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

        ////////////// Testing RechargeBattery(); Press key 'B' to test
        if (Input.GetKeyDown(KeyCode.B))
            RechargeBattery(maxBattery);
    }

    void ToggleFlashlight()
    {
        NoiseManager.MakeNoise(transform.position, toggleHearingRadius);

        // If it's locked out because battery is dead,
        // prevent turning it back on
        if (isLockedOut && currentBattery <= 0f)
        {
            Debug.Log("Battery is dead! Find a battery to recharge.");
            return;
        }

        isOn = !isOn;

        if (flashlightLight != null)
            flashlightLight.enabled = isOn;

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

        if (flashlightLight != null)
            flashlightLight.enabled = false;

        if (flashlightAudio != null && soundOff != null)
            flashlightAudio.PlayOneShot(soundOff);
    }

    void UpdateBatteryUI()
    {
        if (batteryText != null)
        {
            // Displays rounded battery percentage on screen
            batteryText.text = Mathf.Round(currentBattery) + "%";
        }

        // ADDED - updates the new four-section flashlight HUD meter
        UpdateFlashlightMeter();
    }

    // ADDED
    void UpdateFlashlightMeter()
    {
        if (maxBattery <= 0f)
            return;

        float batteryPercent = currentBattery / maxBattery;

        int activeSegments;
        Color activeColor;

        // 76% - 100%
        if (batteryPercent > 0.75f)
        {
            activeSegments = 4;
            activeColor = normalColor;
        }
        // 51% - 75%
        else if (batteryPercent > 0.50f)
        {
            activeSegments = 3;
            activeColor = normalColor;
        }
        // 26% - 50%
        else if (batteryPercent > 0.25f)
        {
            activeSegments = 2;
            activeColor = warningColor;
        }
        // 1% - 25%
        else if (batteryPercent > 0f)
        {
            activeSegments = 1;
            activeColor = criticalColor;
        }
        // 0%
        else
        {
            activeSegments = 0;
            activeColor = emptyColor;
        }

        SetSegment(segment1, activeSegments >= 1, activeColor);
        SetSegment(segment2, activeSegments >= 2, activeColor);
        SetSegment(segment3, activeSegments >= 3, activeColor);
        SetSegment(segment4, activeSegments >= 4, activeColor);
    }

    // ADDED
    void SetSegment(Image segment, bool active, Color activeColor)
    {
        if (segment == null)
            return;

        segment.color = active ? activeColor : emptyColor;
    }

    public void RechargeBattery(float amount)
    {
        currentBattery += amount;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        if (currentBattery > 0f)
            isLockedOut = false;

        UpdateBatteryUI();
    }

    void CheckForEnemy()
    {
        RaycastHit hit;
        Vector3 forward = transform.forward;

        if (Physics.Raycast(
            transform.position,
            forward,
            out hit,
            raycastRange,
            enemyLayer))
        {
            EnemyAI_WaveType waveEnemy =
                hit.collider.GetComponent<EnemyAI_WaveType>();

            if (waveEnemy != null)
            {
                Transform hitEnemy = hit.transform;

                if (hitEnemy == currentEnemy)
                {
                    hitTimer += Time.deltaTime;
                }
                else
                {
                    currentEnemy = hitEnemy;
                    hitTimer = 0.0f;
                }

                if (hitTimer >= requiredColliderTime)
                {
                    waveEnemy.ApplyFlashLightStun(stunTime);
                }
            }
            else
            {
                ResetTimer();
            }
        }
        else
        {
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        hitTimer = 0.0f;
        currentEnemy = null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawRay(
            transform.position,
            transform.forward * raycastRange);
    }
}