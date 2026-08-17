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

    [Header("References")]
    [SerializeField] string PromptMessage = "Press E to pick up";

    float currentHoldTimer;
    string currentPropmt;
    
    private void Start()
    {
        currentHoldTimer = HoldTimer;
        currentPropmt = PromptMessage;
    }

    public bool Interact(GameObject interactor)
    {
        IPlayer player = interactor.GetComponent<IPlayer>();

        if (player != null)
        {
            player.PlayerAddItem(itemName, quantity);

            Destroy(gameObject);

            return true;
        }

        return false;
    }

    public string ScreenMessage()
    {
        return currentPropmt;
    }

    bool DoHold()
    {
        currentHoldTimer -= Time.deltaTime;

        currentHoldTimer = Mathf.Clamp(currentHoldTimer, 0, HoldTimer);

        currentPropmt = currentHoldTimer.ToString("F1");

        if (currentHoldTimer == 0)
        {
            return true;
        }

        return false;
    }

    public void StopHold()
    {
        currentPropmt = PromptMessage;

        currentHoldTimer = HoldTimer;
    }
}
