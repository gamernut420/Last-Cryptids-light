using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class RescueBeacon : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Range(1f, 10f)][SerializeField] float interactRange = 4.0f;
    [SerializeField] float holdDuration = 3f;
    [SerializeField] Transform playerCamera;
    [SerializeField] TextMeshProUGUI interactionPromptText;
    [SerializeField] string PromptMessage = "Hold E to repair beacon!";

    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] ExtractionCountdown extractionCountdown;

    [System.Serializable]


    public class RequiredItem
    {
        [Tooltip("Drag the item prefab here")]
        public GameObject itemPrefab;
        [Tooltip("Auto-populated from the prefab name")]
        public string itemName;
        public int requiredAmount = 1;
    }
    [Header("Required Parts List")]
    [SerializeField] RequiredItem[] requiredParts = new RequiredItem[0];
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
        if (playerInventory == null) playerInventory = FindAnyObjectByType<PlayerInventory>();
        if (extractionCountdown == null) extractionCountdown = FindAnyObjectByType<ExtractionCountdown>();

        ResolvePlayerCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRepaired || playerCamera == null)
        { ResetHold(); return;  }
        if (!IsPlayerLookingAtBeacon()) { ResetHold(); return; }
        if (!HasAllRequiredParts(out string missingPartsMessage))
        {
            currentHoldTime = 0f;
            InteractionPromptUI.Show(interactionPromptText, this, missingPartsMessage);
            return;
        }
       if (IsInteractHeld())
        {
            currentHoldTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentHoldTime / holdDuration);
            int percent = Mathf.RoundToInt(progress * 100f);
            InteractionPromptUI.Show(interactionPromptText, this, $"Repairing... {percent}%");
            if (currentHoldTime >= holdDuration) CompleteRepair();
        }

        else
        {
            currentHoldTime = 0f;
            InteractionPromptUI.Show(interactionPromptText, this, PromptMessage);
        }

    }




    private bool IsPlayerLookingAtBeacon()
    {
        float distance = Vector3.Distance(transform.position, playerCamera.position);
        if (distance > interactRange) return false;
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange))
            return false;
        return hit.transform == transform || hit.transform.IsChildOf(transform);
    }
    private bool IsInteractHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.isPressed;
#else
        return Input.GetKey(KeyCode.E);
#endif
    }

    private bool HasAllRequiredParts(out string message)
    {
       if (playerInventory == null)
        {
            message = "Player inventory was not found"; return false;
        }
       foreach (RequiredItem requirement in requiredParts)
        {
            if (requirement == null || string.IsNullOrWhiteSpace(requirement.itemName)) continue;
            int playerAmount = playerInventory.GetAmount(requirement.itemName);
            if (playerAmount < requirement.requiredAmount)
            {
                int stillNeeded = requirement.requiredAmount - playerAmount;
                message = $"Missing {stillNeeded} x {requirement.itemName}";
                return false;
            }
        }
        message = string.Empty;
        return true;
    }


   

    public void CompleteRepair()
    {
        if (isRepaired) return;

        isRepaired = true;
        currentHoldTime = holdDuration;
        Debug.Log("Beacon repaired successfully! Starting countdown timer.", this);
        InteractionPromptUI.Show(interactionPromptText, this, "Beacon Repaired! Defend it!");

        if (extractionCountdown != null)
        {
            extractionCountdown.StartExtractionTimer();
        }
        else
        {
            Debug.LogWarning("Beacon repaired but no countdown", this);
        }

    
       
    }

   private void ResetHold()
    {
        currentHoldTime = 0f;
        InteractionPromptUI.Hide(interactionPromptText, this);
    }

    private void ResolvePlayerCamera()
    {
        if (playerCamera != null && playerCamera.GetComponent<Camera>() == null)
        {
            Camera childCamera = playerCamera.GetComponentInChildren<Camera>();
            if (childCamera != null) playerCamera = childCamera.transform;
            else if (Camera.main != null) playerCamera = Camera.main.transform;
        }

        if (playerCamera == null && Camera.main != null) playerCamera = Camera.main.transform;
    }

    private void OnDisable()
    {
        InteractionPromptUI.Hide(interactionPromptText, this);
    }
}
