using UnityEngine;
using UnityEngine.Audio;

public class MusicVolumeInitializer : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParameter = "MusicVolume";
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   private void Start()
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("MusicVolumeInitializer: No Audio Mixer is assigned.", this);
            return;
        }

        float volume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultVolume);
        float decibels = volume <= 0.0001f
            ? -80f
            : Mathf.Log10(volume) * 20f;

        audioMixer.SetFloat(exposedParameter, decibels);
    }
}
