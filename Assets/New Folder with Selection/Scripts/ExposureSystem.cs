using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ExposureSystem : MonoBehaviour
{
    [Header("Exposure Settings")]
    [SerializeField] float exposureRate = 0.2f;
    [SerializeField] bool isOutside;

    [Header("Exposure Settings")]
    [Tooltip("Amonut of health restored per second while inside the home base.")]
    [SerializeField] private float healRate = 0.05f;
    private float healAccumulator = 0f;
    private float damageAccumlator = 0f;


    [Header("Visual Pulse Settings")]
    [SerializeField] private float pulseSpeed;
    [SerializeField] private float minIntensity;
    [SerializeField] private float maxIntensity;



    [Header("Visual Effects")]
    [SerializeField] private Volume exposureVolume;
    private Vignette vignette;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(exposureVolume != null && exposureVolume.profile != null)
            exposureVolume.profile.TryGet(out vignette);
        SetOutsideStatus(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(isOutside)
        {
            // Drain player health over time
            ApplyExposureDamage(exposureRate * Time.deltaTime);

            // Create a pulsing warning effect 
            if(vignette != null)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * (maxIntensity - minIntensity) * 0.5f + (minIntensity + maxIntensity) * 0.5f;
                vignette.intensity.value = pulse;
            }
        }
        else
        {
            // Heal Player when in safe zone
            HandleHealing(healRate);

            // Clear the warning effect when back inside
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, 0f, Time.deltaTime * 2f);
            }
        }
    }

    void ApplyExposureDamage(float deltaDanage)
    {
        damageAccumlator += deltaDanage;
        // Once accumulated damage hit 1 or more, deal 1 point of damage
        if (damageAccumlator >= 0.25f)
        {
            int damageToDeal = Mathf.FloorToInt(damageAccumlator); 
            damageAccumlator -= damageToDeal;

            // Find the player controller and deal damage without flashing the screen
            playerController player = GetComponent<playerController>();
            if (player != null)
            {
                player.takeDamage(damageToDeal, false);
            }

        }
    }
   public void SetOutsideStatus(bool other)
    {
        isOutside = other;
    }

    void HandleHealing(float daltaTime)
    {
        healAccumulator += healRate * daltaTime;

        if (healAccumulator >= 1f)
        {
            int healthToRestore = Mathf.FloorToInt(healAccumulator);
            healAccumulator -= healthToRestore;

            IPlayer player = GetComponent<IPlayer>();
            if (player != null)
            {
                player.HealPlayer(healthToRestore, false);
            }
        }
    }
}

