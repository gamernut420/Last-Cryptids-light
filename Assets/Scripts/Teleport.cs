using UnityEngine;

public class TeleportWall : MonoBehaviour
{
    [SerializeField] private Transform teleportDestination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController playerController = other.GetComponent<CharacterController>();

            if (playerController != null)
            {
                playerController.enabled = false;
                other.transform.position = teleportDestination.position;
                playerController.enabled = true;
            }
        }
    }
}