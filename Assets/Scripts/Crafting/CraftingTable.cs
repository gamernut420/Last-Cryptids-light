using UnityEngine;

public class CraftingTable : MonoBehaviour, IInteract
{
    [Header("Crafting")]

    [SerializeField]
    private RecipeBookSO recipeBook;


    [Header("Crafting UI")]

    [SerializeField]
    private CraftingUIManager craftingUI;


    [Header("Interaction")]

    [SerializeField]
    private string interactionMessage =
        "Use Crafting Table";


    public RecipeBookSO RecipeBook =>
        recipeBook;


    public bool Interact(
        GameObject interactor)
    {
        if (interactor == null)
        {
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
                "CraftingTable could not find PlayerInventory."
            );

            return false;
        }


        if (craftingUI != null)
        {
            // ADDED FOR CRAFTING UI:
            craftingUI.ToggleCraftingUI();
        }
        else
        {
            Debug.LogWarning(
                "CraftingTable does not have a CraftingUIManager assigned."
            );

            return false;
        }


        return true;
    }


    // ADDED FOR CRAFTING:
    public bool CanCraft(
        CrafatbleItemRecipe recipe,
        PlayerInventory inventory)
    {
        if (recipe == null ||
            inventory == null)
        {
            return false;
        }


        return CraftingSystem.CanCraft(
            recipe,
            inventory
        );
    }


    // ADDED FOR CRAFTING:
    public bool Craft(
        CrafatbleItemRecipe recipe,
        PlayerInventory inventory)
    {
        if (recipe == null ||
            inventory == null)
        {
            return false;
        }


        return CraftingSystem.TryCraft(
            recipe,
            inventory
        );
    }


    public string ScreenMessage()
    {
        return interactionMessage;
    }
}