using UnityEngine;

public class AudioCueManager : MonoBehaviour
{
    public static AudioCueManager instance;
    [Header("Branch Snaps")]
    [SerializeField] AudioClip[] branchSnapClips;
    [Header("Wind Whispers")]
    [SerializeField] AudioClip[] windWhisperClips;
    [Header("3D Audio Settings")]
    [SerializeField] float minDistance = 2f;
    [SerializeField] float maxDistance = 25f; //making the sound quiter if far and louder if close to the player

    private void Awake()
    {
        instance = this;
    }

    public void PlayBranchSnap(Vector3 position)
    {
        PlayRandomClip(branchSnapClips, position);
    }

    public void PlayWindWhisper(Vector3 position)
    {
        PlayRandomClip(windWhisperClips, position);
    }

    private void PlayRandomClip(AudioClip[] clips, Vector3 position)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip selectedClip = clips[Random.Range(0, clips.Length)];
        GameObject audioObject = new GameObject("TempAudioCue");
        audioObject.transform.position = position;

        AudioSource source = audioObject.AddComponent<AudioSource>();

        source.clip = selectedClip;
        source.spatialBlend = 1f; // 3D sound
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;

        source.pitch = Random.Range(0.1f, 1.1f); // Slight pitch variation

        source.Play();

        Destroy(audioObject, selectedClip.length / Mathf.Abs(source.pitch)+0.1f);
    }

}
