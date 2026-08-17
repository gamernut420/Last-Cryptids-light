using UnityEngine;

public class AmmoBox : MonoBehaviour, IInteract
{
    [SerializeField] int RefillAmmount = 5;
    [SerializeField][Min(0)] float HoldTimer = 0;

    float currentHoldTimer;
    string interactionText = "Pickup Ammo";

    private void Start()
    {
        currentHoldTimer = HoldTimer;
    }

    public bool Interact(GameObject interactor)
    {
        IPlayer player = interactor.GetComponent<IPlayer>();

        if(player != null)
        {
            if (player.PlayerRefillAmmo(RefillAmmount))
            {
                Destroy(gameObject);

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public string ScreenMessage()
    {
        return interactionText;
    }

    public void StopHold()
    {
        interactionText = "Pickup Ammo";

        currentHoldTimer = HoldTimer;
    }

    public bool DoHold()
    {
        currentHoldTimer -= Time.deltaTime;

        currentHoldTimer = Mathf.Clamp(currentHoldTimer, 0, HoldTimer);

        interactionText = currentHoldTimer.ToString("F1");

        if (currentHoldTimer == 0)
        {
            return true;
        }

        return false;
    }
}
