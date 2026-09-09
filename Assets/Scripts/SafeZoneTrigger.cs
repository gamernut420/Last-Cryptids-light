using UnityEngine;

public class SafeZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player enters the safe zone
        ExposureSystem playerExposure = other.GetComponent<ExposureSystem>();
        if (playerExposure != null)
            playerExposure.SetOutsideStatus(false);

    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player exits the safe zone
        ExposureSystem playerExposure = other.GetComponent<ExposureSystem>();
        if (playerExposure != null)
            playerExposure.SetOutsideStatus(true);
    }
}
