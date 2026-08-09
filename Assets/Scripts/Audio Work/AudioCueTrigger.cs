using UnityEngine;

public class AudioCueTrigger : MonoBehaviour
{
    public enum CueType
    {
        BranchSnap, WindWhisper
    }

    [Header("Cue Settings")]
    [SerializeField] CueType cueType;

    [SerializeField] Transform soundOrigin;
    [SerializeField] float playChance;
    [SerializeField] bool oneShot = true;

    bool hasPlayed = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return; //Makes sure that the player is the only one that can trigger the audio cue 
        if (oneShot && hasPlayed) return;
        if (Random.value > playChance) return;

        Vector3 position;

        if (soundOrigin != null) position = soundOrigin.position; 
        else position = transform.position;

        switch (cueType)
        {
            case CueType.BranchSnap:
                AudioCueManager.instance.PlayBranchSnap(position);
                break;
            case CueType.WindWhisper:
                AudioCueManager.instance.PlayWindWhisper(position);
                break;
        }
        hasPlayed = true;
    }



}
