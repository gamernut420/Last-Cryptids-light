using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


[RequireComponent(typeof(Slider))]
public class SFXVolumeSlider : MonoBehaviour
{

    private const string SFXVolumeKey = "SFXVolume";

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParameter = "SFXVolume";
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

        volumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(SFXVolumeKey, defaultVolume);
        volumeSlider.SetValueWithoutNotify(savedVolume);
        ApplyVolumeToMixer(savedVolume);
    }

    private void OnDisable()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
        PlayerPrefs.Save();
        ApplyVolumeToMixer(volume);
        UISoundPlayer.Instance?.PlaySliderFeedback();
    }

    private void ApplyVolumeToMixer(float volume)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("SFXVolumeSlider: No Audio Mixer is assigned.", this);
            return;
        }

        float decibels = volume <= 0.0001f
            ? -80f
            : Mathf.Log10(volume) * 20f;

        audioMixer.SetFloat(exposedParameter, decibels);
    }
}
