using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class PauseMenuMusic : MonoBehaviour
{
    [SerializeField] private AudioClip pauseMusic;

    private AudioSource musicSource;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;

        // This lets the music continue if AudioListener.pause is used later.
        musicSource.ignoreListenerPause = true;
    }

    public void PlayPauseMusic()
    {
        if (pauseMusic == null)
        {
            Debug.LogWarning("PauseMenuMusic: No pause music clip is assigned.", this);
            return;
        }

        musicSource.clip = pauseMusic;
        musicSource.Play();
    }

    public void StopPauseMusic()
    {
        musicSource.Stop();
    }
}
