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

        inventory.AddItem(
            material.MaterialName,
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