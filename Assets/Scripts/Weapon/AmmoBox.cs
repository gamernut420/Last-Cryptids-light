using UnityEngine;

public class AmmoBox : MonoBehaviour, IInteract
{
    [SerializeField] int RefillAmmount = 5;

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
        return "Pickup Ammo";
    }
}
