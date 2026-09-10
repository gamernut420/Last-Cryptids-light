using UnityEngine;

public class ShopStation : MonoBehaviour, IInteract
{
    public bool Interact(GameObject interactor)
    {
        gameManager.instance.ShowShopUI(true);

        return true;
    }

    public string ScreenMessage()
    {
        return "Open Item Shop";
    }
}
