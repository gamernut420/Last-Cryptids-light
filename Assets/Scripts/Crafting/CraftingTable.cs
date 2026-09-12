using UnityEngine;

public class CraftingTable : MonoBehaviour, IInteract
{
    [Header("Crafting")]

    // ADDED:
    // Recipe book containing all recipes available
    // at this crafting table.
    [SerializeField]
    private RecipeBookSO recipeBook;


    [Header("Crafting UI")]

    // ADDED:
    // Reference to the crafting UI that should be opened
    // when the player interacts with this table.
    [SerializeField]
    private CraftingUIManager craftingUI;


    [Header("Interaction")]

    [SerializeField]
    private string interactionMessage =
        "Use Crafting Table";


    // ADDED:
    // Allows CraftingUIManager to read the recipes
    // assigned to this crafting table.
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


        // ADDED:
        // Open the crafting UI when the player
        // interacts with the table.
        if (craftingUI != null)
        {
            craftingUI.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "CraftingTable does not have a CraftingUIManager assigned."
            );
        }


        return true;
    }


    // ADDED:
    // Checks whether the player has enough materials
    // to craft the selected teammate recipe.
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


    // ADDED:
    // Attempts to craft the selected recipe.
    //
    // CraftingSystem handles:
    // - checking ingredients
    // - removing ingredients
    // - adding the finished ScriptableItem
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