using TMPro;
using UnityEngine;

public class RescueBeacon : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Range(1, 10)][SerializeField] float interactRange = 4.0f;
    [SerializeField] Transform playerCamera;
    [SerializeField] TextMeshProUGUI interactionPromptText;
    [SerializeField] string PromptMessage = "Message";

    [Header("Required Parts")]
    [Range(1, 10)][SerializeField] int requiredBatteries = 1;
    [Range(1, 10)][SerializeField] int requiredRadioTubes = 1;
    [Range(1, 10)][SerializeField] int requiredFuel = 1;


    private bool isRequried = false;

    // TODO: Added player inventory Reference 



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // TODO: Added player inventory

        if (interactionPromptText != null) interactionPromptText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isRequried || playerCamera == null) return;

        float distance = Vector3.Distance(transform.position, playerCamera.position);

            // Show when close
        if(distance <= interactRange)
        {
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(true);
                interactionPromptText.text = PromptMessage;
            }
            if (Input.GetKeyDown(KeyCode.E)) TryRepairBeacon();
        }

        else
        {
            // Hide when away
            if(interactionPromptText != null)
                interactionPromptText.gameObject.SetActive(false);
        }
    }

    void TryRepairBeacon()
    {
        //TODO: Added player inventory if = to null return

        //TODO: Added inventery check counts

        int batteries = requiredBatteries;
        int radio = requiredRadioTubes;
        int fuel = 0;

        if(batteries >= requiredBatteries && radio >= requiredRadioTubes && fuel >= requiredFuel)
        {
            isRequried = true;
            Debug.Log("Beacon repaired successfully! Starting extraction phase..");

            if (interactionPromptText != null)
                interactionPromptText.text = "Beacon Repaired! Defend the Area!";

            //TODO: Trigger extraction countdown timer and boost emery to attack more
        }
        else
        {
            Debug.Log("Missing parts to pair the beacon!");
            if (interactionPromptText != null)
                interactionPromptText.text = "Missing Parts! Need more supplies.";
        }
    }
}
