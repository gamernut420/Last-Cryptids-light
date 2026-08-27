using UnityEngine;

public class CheckpointPlate : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private AudioSource activationAudioSource;
    [SerializeField] private AudioClip activationSound;

    private bool hasBeenActivated;

    private void Awake()
    {
        if(respawnPoint == null)
        {
            respawnPoint = transform;
        } 
        if (activationAudioSource == null)
        {
            activationAudioSource = GetComponent<AudioSource>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenActivated || !other.CompareTag("Player")) return;
        if (CheckpointManager.ShouldIgnorePlateActivation())
        {
            Debug.Log("Checkpoint Plate: Ignored the activation when respawning", this); return;
        }
        if (gameManager.instance == null)
        {
            Debug.LogWarning("CheckpointPlate: game manager wasn't found", this);
            return;
        }

        hasBeenActivated = true;
        gameManager.instance.SaveCheckpoint(respawnPoint);
        if (activationAudioSource != null && activationSound != null)
        {
            activationAudioSource.PlayOneShot(activationSound);
        }
    }

}
