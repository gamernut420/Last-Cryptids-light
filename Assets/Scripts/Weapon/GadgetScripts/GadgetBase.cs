using UnityEngine;

//This class handles all of the code all gadgets use
public abstract class GadgetBase : MonoBehaviour, IGadget, IInteract
{
    [SerializeField] string GadgetName;

    public string GetGadgetName()
    {
        return GadgetName;
    }

    public abstract bool Interact(GameObject interactor);

    public string ScreenMessage()
    {
        return $"Pickup {GadgetName}";
    }

    public abstract bool UseGadget(IPlayer player);
}
