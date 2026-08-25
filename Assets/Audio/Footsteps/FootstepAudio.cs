using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] footstepClips;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource ==  null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayFootstepSound(float stepVol)
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        audioSource.PlayOneShot(footstepClips[index], stepVol);
    }
}
