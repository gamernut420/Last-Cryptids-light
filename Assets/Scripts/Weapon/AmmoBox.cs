using UnityEngine;

public class AmmoBox : MonoBehaviour, IInteract
{
    [SerializeField] int RefillAmmount = 5;

    public bool Interact(GameObject interactor)
    {
        IPlayer player = interactor.GetComponent<IPlayer>();

        if(player != null)
        {
            Debug.Log("Interacted with ammo");

            player.PlayerRefillAmmo(RefillAmmount);

            return true;
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
