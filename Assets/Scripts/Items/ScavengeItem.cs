using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ScavengeItem : MonoBehaviour, IInteract
{
    [Header("Item Settings")]
    [SerializeField] string itemName = "Item Name";
    [Range(1, 10)][SerializeField] int quantity;
    [SerializeField][Min(0)] float HoldTimer;
    [Range(1f, 10f)][SerializeField] float interactRange;
    [SerializeField] float minimumLookAlignment = 0.8f;

    [Header("References")]
    [SerializeField] Transform playerCamera;
    [SerializeField] TextMeshProUGUI interactionPromptText;
    [SerializeField] string PromptMessage = "Press E to pick up";
    [SerializeField] PlayerInventory playerInventory;

    private bool collected = false;
    
    private void Start()
    {
        if (playerInventory == null) playerInventory = FindAnyObjectByType<PlayerInventory>();
        ResolvePlayerCamera();
    }

    private void Update()
    {
        //if (collected || playerCamera == null) return;
        
        //if (IsPlayerLookingAtItem())
        //{
        //    InteractionPromptUI.Show(interactionPromptText, this, $"{PromptMessage} {itemName}");

        //    if (WasInteractPressed())
        //        CollectItem();
        //}
        //else {InteractionPromptUI.Hide(interactionPromptText, this);
        //}
    }

    private bool IsPlayerLookingAtItem()
    {
        Vector3 directionToItem = transform.position - playerCamera.position;
        if (directionToItem.sqrMagnitude > interactRange * interactRange) return false;
        if (directionToItem.sqrMagnitude <= Mathf.Epsilon) return true;
        float lookAlignment = Vector3.Dot(playerCamera.forward, directionToItem.normalized);
        return lookAlignment >= minimumLookAlignment;
    }

   private bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
return Input.GetKeyDown(KeyCode.E);
#endif
    }


    void CollectItem()
    {
       if (collected) return;
        if (playerInventory == null)
        {
            Debug.LogWarning($"Cannot collect {itemName} because PlayerInventory is not assigned.", this);
            return;
        }
        collected = true;
        playerInventory.AddItem(itemName, quantity);
        Debug.Log($"Collected {quantity} x {itemName}", this);
        Destroy(gameObject);
    }

    private void ResolvePlayerCamera()
    {
        if (playerCamera != null && playerCamera.GetComponent<Camera>() ==null)
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

    public bool Interact(GameObject interactor)
    {
        if (collected) return false;
        CollectItem();
        return collected;
    }

    public string ScreenMessage()
    {
        return $"{PromptMessage} {itemName}";
    }

    public void StopHold()
    {
        throw new System.NotImplementedException();
    }
}
