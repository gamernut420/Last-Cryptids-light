using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ExposureSystem : MonoBehaviour
{
    [Header("Exposure Settings")]
    [Range(0, 10)][SerializeField] float exposureRate;
    [SerializeField] bool isOutside;

    [Header("Visual Effects")]
    [SerializeField] private Volume exposureVolume;
    private Vignette vignette;
    private float damageAccumlator = 0f;

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
                float pulse = Mathf.Sin(Time.time * 6f) * 0.15f + 0.35f;
                vignette.intensity.value = pulse;
            }
        }
        else
        {
            // Clear the warning effect when back inside
            if (vignette != null)
                vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, 0f, Time.deltaTime * 2f);
        }
    }

    void ApplyExposureDamage(float deltaDanage)
    {
        damageAccumlator += deltaDanage;
        // Once accumulated damage hit 1 or more, deal 1 point of damage
        if (damageAccumlator >= 1f)
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
}

