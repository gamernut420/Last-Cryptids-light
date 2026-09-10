using UnityEngine;

public class ShopStation : MonoBehaviour, IInteract
{
    [SerializeField] GameObject SpawnLocation;

    public bool Interact(GameObject interactor)
    {
        gameManager.instance.ShowShopUI(true, SpawnLocation.transform.position);

        return true;
    }

    public string ScreenMessage()
    {
        return "Open Item Shop";
    }
}
