using UnityEngine;

/// <summary>
/// Handles crafting logic using the team's existing
/// CrafatbleItemRecipe and PlayerInventory systems.
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


            // ADDED FOR CRAFTING:
            // Each ingredient must have a ScriptableItem
            // assigned so PlayerInventory can check it.
            if (ingredient.itemData == null)
            {
                Debug.LogWarning(
                    recipe.name +
                    " has an ingredient with no ScriptableItem assigned."
                );

                return false;
            }


            // ADDED FOR CRAFTING:
            // Ingredient quantities must be at least 1.
            if (ingredient.Quantity <= 0)
            {
                Debug.LogWarning(
                    recipe.name +
                    " has an ingredient with an invalid quantity."
                );

                return false;
            }


            // ADDED FOR CRAFTING:
            // Check the actual ScriptableItem and required quantity.
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
    /// Removes all required ingredients from the player's inventory.
    ///
    /// Only call after CanCraft succeeds.
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


            // ADDED FOR CRAFTING:
            // Remove the exact ScriptableItem required
            // by this recipe.
            playerInventory.RemoveItem(
                ingredient.itemData,
                ingredient.Quantity
            );
        }
    }


    /// <summary>
    /// Attempts to craft the selected recipe.
    ///
    /// Returns true when crafting succeeds.
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


        // ADDED FOR CRAFTING:
        // The recipe should still have a prefab representing
        // the physical version of the crafted item.
        if (recipe.itemPrefab == null)
        {
            Debug.LogWarning(
                recipe.name +
                " does not have an item prefab assigned."
            );

            return false;
        }


        // ADDED FOR CRAFTING:
        // This is the ScriptableItem that will actually be
        // stored inside PlayerInventory.
        if (recipe.craftedItemData == null)
        {
            Debug.LogWarning(
                recipe.name +
                " does not have Crafted Item Data assigned."
            );

            return false;
        }


        // ADDED FOR CRAFTING:
        // Optional configuration warning.
        //
        // ScriptableItem already contains an itemModel field,
        // so ideally it should point to the same prefab
        // as recipe.itemPrefab.
        if (recipe.craftedItemData.itemModel == null)
        {
            Debug.LogWarning(
                recipe.craftedItemData.itemName +
                " does not have an itemModel assigned."
            );
        }


        // Check all ingredients before removing anything.
        if (!CanCraft(
                recipe,
                playerInventory))
        {
            return false;
        }


        // Remove recipe ingredients.
        ConsumeIngredients(
            recipe,
            playerInventory
        );


        // ADDED FOR CRAFTING:
        // Add the crafted ScriptableItem directly
        // to the team's existing inventory system.
        playerInventory.AddItem(
            recipe.craftedItemData,
            1
        );


        return true;
    }
}