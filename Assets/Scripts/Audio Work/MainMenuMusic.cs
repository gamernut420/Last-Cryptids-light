using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class MainMenuMusic : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;

    private AudioSource musicSource;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (menuMusic == null)
            {
                Debug.LogWarning("MainMenuMusic: No menu music clip is assigned.", this);
                return;
            }

            musicSource.clip = menuMusic;
            musicSource.Play();
    }
  
}
