using UnityEngine;

//This class handles all of the code all gadgets use
public abstract class GadgetBase : MonoBehaviour, IGadget, IInteract
{
    [SerializeField] ScriptableItem ItemInfo;
    [SerializeField] string GadgetName;

    public string GetGadgetName()
    {
        return GadgetName;
    }

    public ScriptableItem GetItemInfo()
    {
        return ItemInfo;
    }

    public abstract bool Interact(GameObject interactor);

    public string ScreenMessage()
    {
        return $"Pickup {GadgetName}";
    }

    public abstract bool UseGadget(GameObject player);
}
