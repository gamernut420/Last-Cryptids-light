using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RescueBeacon : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Range(1, 10)][SerializeField] float interactRange = 4.0f;
    [SerializeField] Transform playerCamera;
    [SerializeField] TextMeshProUGUI interactionPromptText;
    [SerializeField] string PromptMessage = "Message";

    [Header("Wave Timer")]
    [SerializeField] float countDownDuration = 300f;

    [System.Serializable]
    public class RequiredItem
    {
        [Tooltip("Drag the item prefab here")]
        public GameObject itemPrefab;
        [Tooltip("Auto-populated from the prefab name")]
        public string itemName;

        public int requriedAmount = 1;

    }
    [Header("Required Parts List")]
    public List<RequiredItem> requiredParts = new List<RequiredItem>();
    private void OnValidate()
    {
        foreach (var part in requiredParts)
        {
            if(part.itemPrefab != null)
            {
                part.itemName = part.itemPrefab.name;
            }
        }
    }


    private bool isRequried = false;
    PlayerInventory playerInventory;
    private bool isRepaired = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // TODO: Added player inventory
        playerInventory = FindFirstObjectByType<PlayerInventory>();
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
        if(playerInventory  == null) return;

        bool hasAllParts = true;

        foreach (var requirement in requiredParts)
        {
            int playerAmount = playerInventory.GetAmount(requirement.itemName);
            if (playerAmount < requirement.requriedAmount)
            {
                hasAllParts = false; break;
            }
        }

        if (hasAllParts)
        {
            isRequried = true;
            Debug.Log("Beacon repaired successfully! Starting extraction phase..");

            if (interactionPromptText != null)
                interactionPromptText.text = "Beacon Repaired! Defend the Area!";

            //TODO: Trigger extraction countdown timer and boost emery to attack more
            FindAnyObjectByType<ExtractionCountdown>().StartExtractionTimer();
        }
        else
        {
            Debug.Log("Missing parts to pair the beacon!");
            if (interactionPromptText != null)
                interactionPromptText.text = "Missing Parts! Need more supplies.";
        }
    }

    public void CompleteRepair()
    {
        if (isRepaired) return;

        isRepaired = true;
        Debug.Log("Beacon repaired! Starting 5-minute countdown.");

        // Start the timer coroutine
        StartCoroutine(BeaconCountdownRoutine());
    }

    private IEnumerator BeaconCountdownRoutine()
    {
        float timeRemanining = countDownDuration;
        while (timeRemanining > 0f)
        {
            // Subtract time
            timeRemanining -= Time.deltaTime;
            // UpdateTimerUI(timeRemanining);
            yield return null;
        }
        // Timer has hit 0
        OnCountdownExpired();
        
    }
    private void OnCountdownExpired()
    {
        Debug.Log("Countdown finished! Trigger you win!");
        // Put the Win game logic here

    }
}
