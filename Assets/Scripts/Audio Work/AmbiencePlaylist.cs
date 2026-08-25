using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbiencePlaylist : MonoBehaviour
{
    [SerializeField] private AudioClip[] ambienceClips;
    [Range(0f, 1f)][SerializeField] private float volume = 0.5f;

    private AudioSource ambienceSource;
    private int currentClipIndex;
    private bool playlistStarted;
    private bool playlistPaused;
    private float pausedPlaybackTime;

   private void Awake()
    {
        ambienceSource = GetComponent<AudioSource>();

        ambienceSource.playOnAwake = false; 
        ambienceSource.loop = false;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.volume = volume;
        
    }


    private void Update()
    {
        if (!playlistStarted || playlistPaused) return;
        if (!ambienceSource.isPlaying)
        {
            PlayNextClip();
        }
    }

    public void StartPlaylist()
    {
        if (ambienceClips == null || ambienceClips.Length == 0)
        {
            Debug.LogWarning("AmbiencePlaylist: No clips are assigned", this);
            return;
        }

        currentClipIndex = 0;
        playlistStarted = true;
        playlistPaused = false;
        PlayCurrentClip();
    }

    public void PausePlaylist()
    {
        if (!playlistStarted || playlistPaused) return;
        pausedPlaybackTime = ambienceSource.time;
        playlistPaused=true;
        ambienceSource.Pause();
    }
    
    public void ResumePlaylist()
    {
        if (!playlistStarted || !playlistPaused) return;
        playlistPaused=false;
        if (ambienceSource.clip != null)
        {
            float latestValidTime = Mathf.Max(0f, ambienceSource.clip.length - 0.02f);
            ambienceSource.time = Mathf.Clamp(pausedPlaybackTime, 0f, latestValidTime);
            ambienceSource.Play();
        }
        else
        {
            PlayCurrentClip();
        }
    }

    public void StopPlaylist()
    {
        playlistStarted = false;
        playlistPaused = false;
        currentClipIndex = 0;
        ambienceSource.Stop();
    }

    private void PlayCurrentClip()
    {
        AudioClip clip = ambienceClips[currentClipIndex];
        if (clip == null )
        {
            Debug.LogWarning($"AmbiencePlaylist: {currentClipIndex} has no AudioClip", this);
            PlayNextClip();
            return;
        }

        ambienceSource.clip = clip;
        ambienceSource.volume = volume;
        pausedPlaybackTime = 0f;
        ambienceSource.Play();
    }

    private void PlayNextClip()
    {
        for (int attempts = 0; attempts < ambienceClips.Length; attempts++)
        {
            currentClipIndex++;
            if (currentClipIndex >= ambienceClips.Length)
            {
                currentClipIndex = 0;
            }
            if (ambienceClips[currentClipIndex] != null)
            {
                PlayCurrentClip();
                return;
            }
        }
        playlistStarted = false;
        Debug.LogWarning("AmbiencePlaylist: All ambience clips are empty", this);

    }

}
