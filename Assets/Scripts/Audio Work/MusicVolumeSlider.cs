using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MusicVolumeSlider : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParameter = "MusicVolume";
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 0.5f;

    private Slider volumeSlider;

    private void Awake()
    {
        volumeSlider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (volumeSlider == null)
        {
            volumeSlider = GetComponent<Slider>();
        }

        volumeSlider.onValueChanged.AddListener(SetMusicVolume);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultVolume);
        volumeSlider.SetValueWithoutNotify(savedVolume);
        ApplyVolumeToMixer(savedVolume);
    }

    private void OnDisable()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        PlayerPrefs.Save();
        ApplyVolumeToMixer(volume);
    }

    private void ApplyVolumeToMixer(float volume)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("MusicVolumeSlider: No Audio Mixer is assigned.", this);
            return;
        }

        // A mixer uses decibels. -80 dB is effectively silent.
        float decibels = volume <= 0.0001f
            ? -80f
            : Mathf.Log10(volume) * 20f;

        audioMixer.SetFloat(exposedParameter, decibels);
    }
}
