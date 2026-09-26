using UnityEngine;

public class OutOfBoundsTrigger : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();
            if(controller != null)
                controller.enabled = false;

            if(respawnPoint != null)
            {
                other.transform.position = respawnPoint.position;
                other.transform.rotation = respawnPoint.rotation;
            }

            if(controller != null)
                controller.enabled = true;
        }
    }
}
