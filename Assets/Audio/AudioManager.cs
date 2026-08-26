using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource ==  null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySound(float stepVol)
    {
        if (audioClips.Length == 0) return;

        int index = Random.Range(0, audioClips.Length);
        audioSource.PlayOneShot(audioClips[index], stepVol);
    }
}
