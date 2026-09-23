using UnityEngine;

[DefaultExecutionOrder(50)]
[RequireComponent(typeof(playerController))]
public class PlayerStaminaAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource staminaLoopSource;
    [SerializeField] private AudioSource oneShotSource;

    [Header("Stamina Sounds")]
    [SerializeField] private AudioClip staminaDrainLoop;
    [SerializeField] private AudioClip normalRecoverySound;
    [SerializeField] private AudioClip exhaustedRecoverySound;

    [Header("Movement Sounds")]
    [SerializeField] private AudioClip dashSound;

    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float drainLoopVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float recoveryVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float dashVolume = 1f;

    private const float StaminaChangeThreshold = 0.0001f;

    private playerController player;
    private float previousStamina;
    private bool wasDraining;
    private bool reachedEmptyDuringCurrentDrain;
    private bool recoverySoundPlayed;

    private void Awake()
    {
        player = GetComponent<playerController>();

        ConfigureSource(staminaLoopSource);
        ConfigureSource(oneShotSource);

        if (staminaLoopSource != null)
        {
            staminaLoopSource.loop = true;
            staminaLoopSource.clip = staminaDrainLoop;
        }

        if (oneShotSource != null)
        {
            oneShotSource.loop = false;
        }

        if (staminaLoopSource != null && staminaLoopSource == oneShotSource)
        {
            Debug.LogWarning(
                "PlayerStaminaAudio: Assign two different AudioSources so stopping the stamina loop does not cut off dash or recovery sounds.",
                this);
        }
    }

    void Start()
    {
        previousStamina = player.GetCurrentStamina();
    }

    void Update()
    {
        float currentStamina = player.GetCurrentStamina();
        float maxStamina = player.GetMaxStamina();

        if (gameManager.instance != null && gameManager.instance.isPaused)
        {
            StopDrainLoop();
            previousStamina = currentStamina;
            return;
        }

        bool isDraining = currentStamina < previousStamina - StaminaChangeThreshold;
        bool isRecovering = currentStamina > previousStamina + StaminaChangeThreshold;

        if (isDraining)
        {
            if (!wasDraining)
            {
                reachedEmptyDuringCurrentDrain = false;
                recoverySoundPlayed = false;
            }

            if (currentStamina <= StaminaChangeThreshold)
            {
                reachedEmptyDuringCurrentDrain = true;
            }

            StartDrainLoop();
        }
        else
        {
            StopDrainLoop();
        }

        if (currentStamina <= StaminaChangeThreshold &&
            previousStamina > StaminaChangeThreshold)
        {
            PlayOneShot(exhaustedRecoverySound, recoveryVolume);
            recoverySoundPlayed = true;
        }

        if (isRecovering && !recoverySoundPlayed)
        {
            PlayOneShot(normalRecoverySound, recoveryVolume);
            recoverySoundPlayed = true;
        }

        if (currentStamina >= maxStamina - StaminaChangeThreshold && !isDraining)
        {
            reachedEmptyDuringCurrentDrain = false;
        }

        wasDraining = isDraining;
        previousStamina = currentStamina;
    }

    public void PlayDashSound()
    {
        PlayOneShot(dashSound, dashVolume);
    }

    private void StartDrainLoop()
    {
        if (staminaLoopSource == null || staminaDrainLoop == null)
        {
            return;
        }

        if (staminaLoopSource.clip != staminaDrainLoop)
        {
            staminaLoopSource.clip = staminaDrainLoop;
        }

        staminaLoopSource.volume = drainLoopVolume;

        if (!staminaLoopSource.isPlaying)
        {
            staminaLoopSource.Play();
        }
    }

    private void StopDrainLoop()
    {
        if (staminaLoopSource != null && staminaLoopSource.isPlaying)
        {
            staminaLoopSource.Stop();
        }
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (oneShotSource != null && clip != null)
        {
            oneShotSource.PlayOneShot(clip, volume);
        }
    }

    private void ConfigureSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    private void OnDisable()
    {
        StopDrainLoop();
    }
}
