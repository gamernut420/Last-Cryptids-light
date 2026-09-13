using UnityEngine;

public class MaterialPickup : MonoBehaviour, IInteract
{
    [Header("Material Pickup")]

    [SerializeField]
    private CraftingMaterialSO material;

    [SerializeField]
    [Min(1)]
    private int amount = 1;

    [Header("Interaction")]

    [SerializeField]
    private string interactionMessage = "Pick Up";


    // ADDED FOR CRAFTING RECIPES:
    // Allows CraftingIngredient to reference the physical
    // pickup prefab and retrieve the material assigned to it.
    public CraftingMaterialSO Material => material;


    public bool Interact(GameObject interactor)
    {
        if (interactor == null)
        {
            return false;
        }

        if (material == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " does not have a CraftingMaterialSO assigned."
            );

            return false;
        }


        // ADDED FOR NEW PLAYER INVENTORY:
        // PlayerInventory stores ScriptableItem references.
        // The crafting material therefore needs a linked
        // ScriptableItem before it can be collected.
        if (material.InventoryItem == null)
        {
            Debug.LogWarning(
                material.MaterialName +
                " does not have an Inventory Item assigned."
            );

            return false;
        }


        PlayerInventory inventory =
            interactor.GetComponent<PlayerInventory>();

        if (inventory == null)
        {
            inventory =
                interactor.GetComponentInParent<PlayerInventory>();
        }

        if (inventory == null)
        {
            Debug.LogWarning(
                "MaterialPickup could not find PlayerInventory."
            );

            return false;
        }


        // CHANGED FOR NEW PLAYER INVENTORY:
        // PlayerInventory now expects a ScriptableItem
        // instead of a string item name.
        inventory.AddItem(
            material.InventoryItem,
            amount
        );


        Debug.Log(
            "Picked up " +
            amount +
            " x " +
            material.MaterialName
        );

        Destroy(gameObject);

        return true;
    }


    public string ScreenMessage()
    {
        if (material == null)
        {
            return interactionMessage;
        }

        return interactionMessage +
               " " +
               material.MaterialName;
    }
}