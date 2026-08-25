using UnityEngine;

public class AmmoBox : MonoBehaviour, IInteract
{
    [SerializeField] int RefillAmmount = 5;
    [SerializeField][Min(0)] float HoldTimer = 0;

    float currentHoldTimer;
    string interactionText = "Pickup Ammo";

    private void Start()
    {
        currentHoldTimer = 0;
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
                currentHoldTimer = 0;

                return false;
            }
        }
        else
        {
            currentHoldTimer = 0;

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

        currentHoldTimer = 0;
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
}
