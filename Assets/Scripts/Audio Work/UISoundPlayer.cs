using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class UISoundPlayer : MonoBehaviour
{
    public static UISoundPlayer Instance
    {
        get; private set;
    }
    [Header("UI Sounds")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip sliderFeedbackSound;

    [Header("Slider Feedback")]
    [Min(0f)]
    [SerializeField] private float sliderFeedbackCooldown = 0.08f;

    private AudioSource audioSource;
    private float nextSliderFeedbackTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
    }

    public void PlayHover()
    {
        PlaySound(hoverSound);
    }

    public void PlayClick()
    {
        PlaySound(clickSound);
    }

    public void PlaySliderFeedback()
    {
        if (Time.unscaledTime < nextSliderFeedbackTime)
        {
            return;
        }

        nextSliderFeedbackTime = Time.unscaledTime + sliderFeedbackCooldown;
        PlaySound(sliderFeedbackSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
