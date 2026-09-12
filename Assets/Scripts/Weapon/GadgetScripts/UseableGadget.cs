using UnityEngine;

public abstract class UseableGadget : GadgetBase
{
    public override bool Interact(GameObject interactor)
    {
        IPlayer player = interactor.GetComponent<IPlayer>();

        if (player != null)
        {
            player.PlayerAddItem(gameObject);

            gameObject.GetComponent<Collider>().enabled = false;

            return true;
        }

        return false;
    }
}
