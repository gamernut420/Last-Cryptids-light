using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class RescueBeacon : MonoBehaviour, IInteract
{
    [Header("Interaction Settings")]
    [Range(1f, 10f)][SerializeField] float interactRange = 4.0f;
    [SerializeField] float holdDuration = 3f;
    [SerializeField] string PromptMessage = "Hold E to repair beacon!";
    string currentPrompt;

    [System.Serializable]
    public class RequiredItem
    {
        [Tooltip("Drag the item prefab here")]
        public GameObject itemPrefab;

        [Tooltip("Set ammount needed here")]
        public int requiredAmount = 1;

        [Tooltip("Auto-populated from the prefab name")]
        public string itemName;
    }

    [Header("Required Parts List")]
    [SerializeField] public RequiredItem[] requiredParts = new RequiredItem[0];
    float currentHoldTime = 0f;
    bool isRepaired = false;

    private void OnValidate()
    {
        holdDuration = Mathf.Max(0.1f, holdDuration);

        foreach (RequiredItem part in requiredParts)
        {
            if (part == null) continue;
            if (part.itemPrefab != null) part.itemName = part.itemPrefab.name;
            part.requiredAmount = Mathf.Max(1, part.requiredAmount); 
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPrompt = PromptMessage;
        currentHoldTime = holdDuration;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private bool HasAllRequiredParts()
    {
        PlayerInventory inventory = gameManager.instance.playerInventory;

        bool hasEnough = true;

        currentPrompt = string.Empty;

        foreach (RequiredItem requirement in requiredParts)
        {
            if (requirement == null || string.IsNullOrWhiteSpace(requirement.itemName)) continue;

            int playerAmount = inventory.GetAmount(requirement.itemName);

            if (playerAmount < requirement.requiredAmount)
            {
                int stillNeeded = requirement.requiredAmount - playerAmount;
                currentPrompt += $"Missing {stillNeeded} x {requirement.itemName}\n";

                hasEnough = false;
            }
        }

        return hasEnough;
    }

    public void CompleteRepair()
    {
        if (isRepaired) return;

        isRepaired = true;
        Debug.Log("Beacon repaired successfully! Starting countdown timer.", this);

        gameManager.instance.isExtracting = true;
    }

    public bool DoHold()
    {
        if (HasAllRequiredParts())
        {
            currentHoldTime -= Time.deltaTime;

            currentHoldTime = Mathf.Clamp(currentHoldTime, 0, holdDuration);

            currentPrompt = currentHoldTime.ToString("F1");

            if (currentHoldTime == 0)
            {
                return true;
            }
        }

        return false;
    }

    public void StopHold()
    {
        currentPrompt = PromptMessage;
    }

    public bool Interact(GameObject interactor)
    {
        CompleteRepair();

        return true;
    }

    public string ScreenMessage()
    {
        return currentPrompt;
    }
}
