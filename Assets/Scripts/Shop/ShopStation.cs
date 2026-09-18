using System.Collections.Generic;
using UnityEngine;

public class ShopStation : MonoBehaviour, IInteract
{
    [SerializeField] GameObject SpawnLocation;
    [SerializeField] List<ShopItem> Items;

    public bool Interact(GameObject interactor)
    {
        gameManager.instance.ShowShopUI(true, Items, SpawnLocation.transform.position);

        return true;
    }

    public string ScreenMessage()
    {
        return "Open Item Shop";
    }
}
