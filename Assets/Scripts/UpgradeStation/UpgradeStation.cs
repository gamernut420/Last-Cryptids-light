using UnityEngine;

public class UpgradeStation : MonoBehaviour, IInteract
{
    public bool Interact(GameObject interactor)
    {
        gameManager.instance.ShowUpgradeUI(true);

        return true;
    }

    public string ScreenMessage()
    {
        return "Upgrades";
    }
}
