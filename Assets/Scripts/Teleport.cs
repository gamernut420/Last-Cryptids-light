using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        CharacterController controller =
            other.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        other.transform.position = destination.position;
        other.transform.rotation = destination.rotation;

        if (controller != null)
            controller.enabled = true;
    }
}
