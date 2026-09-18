using UnityEngine;

public class DistressBeaconInteract : MonoBehaviour
{
    [SerializeField] private GameObject beaconBubble;

    private bool playerNearby;

    private void Start()
    {
        beaconBubble.SetActive(false);
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            beaconBubble.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
