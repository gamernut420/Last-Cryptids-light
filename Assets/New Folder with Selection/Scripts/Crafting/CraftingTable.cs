using UnityEngine;

public class CraftingTable : MonoBehaviour, IInteract
{
    [Header("Crafting")]

    [SerializeField]
    private RecipeBookSO recipeBook;

    [Header("Interaction")]

    [SerializeField]
    private string interactionMessage =
        "Use Crafting Table";


    public RecipeBookSO RecipeBook =>
        recipeBook;


    public bool Interact(GameObject interactor)
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

        if (recipeBook == null)
        {
            Debug.LogWarning(
                "CraftingTable does not have a RecipeBookSO assigned."
            );

            return false;
        }

        // Temporary test until the crafting UI is connected.
        Debug.Log(
            "Crafting Table opened. Recipes available: " +
            recipeBook.Recipes.Count
        );

        return true;
    }


    public bool CanCraft(
        CraftingRecipeSO recipe,
        PlayerInventory inventory)
    {
        return CraftingSystem.CanCraft(
            recipe,
            inventory
        );
    }


    public bool Craft(
        CraftingRecipeSO recipe,
        PlayerInventory inventory)
    {
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