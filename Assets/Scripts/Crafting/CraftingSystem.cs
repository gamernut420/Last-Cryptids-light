using UnityEngine;

/// <summary>
/// Handles crafting logic.
///
/// Uses the team's existing CrafatbleItemRecipe assets
/// and PlayerInventory system.
/// </summary>
public static class CraftingSystem
{
    /// <summary>
    /// Checks whether the player has every item required
    /// by the selected recipe.
    /// </summary>
    public static bool CanCraft(
        CrafatbleItemRecipe recipe,
        PlayerInventory playerInventory)
    {
        if (recipe == null ||
            playerInventory == null)
        {
            return false;
        }


        if (recipe.ItemsNeeded == null)
        {
            return false;
        }


        for (int i = 0;
             i < recipe.ItemsNeeded.Count;
             i++)
        {
            CrafatbleItemRecipe.Item ingredient =
                recipe.ItemsNeeded[i];


            // ADDED:
            // New recipes should use the direct ScriptableItem
            // reference instead of relying on string IDs.
            if (ingredient.itemData == null)
            {
                Debug.LogWarning(
                    recipe.name +
                    " has an ingredient with no ScriptableItem assigned."
                );

                return false;
            }


            // ADDED:
            // Check the exact quantity required by this recipe.
            if (!playerInventory.HasItem(
                    ingredient.itemData,
                    ingredient.Quantity))
            {
                return false;
            }
        }


        return true;
    }


    /// <summary>
    /// Removes all recipe ingredients.
    ///
    /// Only call this after CanCraft succeeds.
    /// </summary>
    private static void ConsumeIngredients(
        CrafatbleItemRecipe recipe,
        PlayerInventory playerInventory)
    {
        for (int i = 0;
             i < recipe.ItemsNeeded.Count;
             i++)
        {
            CrafatbleItemRecipe.Item ingredient =
                recipe.ItemsNeeded[i];


            // ADDED:
            // Consume the ScriptableItem directly from
            // the team's PlayerInventory.
            playerInventory.RemoveItem(
                ingredient.itemData,
                ingredient.Quantity
            );
        }
    }


    /// <summary>
    /// Attempts to craft the selected recipe.
    ///
    /// Returns true if successful.
    /// </summary>
    public static bool TryCraft(
        CrafatbleItemRecipe recipe,
        PlayerInventory playerInventory)
    {
        if (recipe == null ||
            playerInventory == null)
        {
            return false;
        }


        if (recipe.itemPrefab == null)
        {
            Debug.LogWarning(
                recipe.name +
                " does not have an item prefab assigned."
            );

            return false;
        }


        // ADDED:
        // The prefab must identify which ScriptableItem
        // should be added to PlayerInventory.
        CraftedItem craftedItem =
            recipe.itemPrefab.GetComponent<CraftedItem>();


        if (craftedItem == null)
        {
            Debug.LogWarning(
                recipe.itemPrefab.name +
                " does not have a CraftedItem component."
            );

            return false;
        }


        if (craftedItem.InventoryItem == null)
        {
            Debug.LogWarning(
                recipe.itemPrefab.name +
                " does not have an Inventory Item assigned."
            );

            return false;
        }


        // Make sure the player has EVERYTHING before consuming anything.
        if (!CanCraft(
                recipe,
                playerInventory))
        {
            return false;
        }


        ConsumeIngredients(
            recipe,
            playerInventory
        );


        // ADDED:
        // Add the crafted result to the existing PlayerInventory.
        playerInventory.AddItem(
            craftedItem.InventoryItem,
            1
        );


        return true;
    }
}