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
        currentHoldTimer = 0;
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

    public float DoHold()
    {
        currentHoldTimer += Time.deltaTime;

        currentHoldTimer = Mathf.Clamp(currentHoldTimer, 0, HoldTimer);

        if (HoldTimer == 0)
        {
            return 1;
        }

        return currentHoldTimer / HoldTimer;
    }

    public void StopHold()
    {
        currentPropmt = PromptMessage;

        currentHoldTimer = 0;
    }
}
