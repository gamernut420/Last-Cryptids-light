using JetBrains.Annotations;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] footstepClips;

    public void PlayFootstepSound()
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        audioSource.PlayOneShot(footstepClips[index]);
    }
}
