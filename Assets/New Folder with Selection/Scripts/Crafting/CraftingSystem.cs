using System.Collections.Generic;

/// <summary>
/// Handles the actual crafting rules.
///
/// This class does not control the UI or physical crafting table.
/// It simply checks recipes, removes ingredients,
/// and gives the crafted item to the player.
/// </summary>
public static class CraftingSystem
{
    /// <summary>
    /// Checks whether the player owns every material
    /// required by the selected recipe.
    /// </summary>
    public static bool CanCraft(
        CraftingRecipeSO recipe,
        PlayerInventory playerInventory)
    {
        if (recipe == null || playerInventory == null)
        {
            return false;
        }

        IReadOnlyList<CraftingIngredient> ingredients =
            recipe.Ingredients;

        for (int i = 0; i < ingredients.Count; i++)
        {
            CraftingIngredient ingredient =
                ingredients[i];

            if (ingredient.Material == null)
            {
                return false;
            }

            // PlayerInventory stores items by string name.
            //
            // We use the material ScriptableObject's MaterialName
            // as the inventory key.
            if (!playerInventory.HasItem(
                    ingredient.Material.MaterialName,
                    ingredient.AmountRequired))
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// Removes all materials required by a recipe.
    ///
    /// This should only be called after CanCraft() succeeds.
    /// </summary>
    private static void ConsumeIngredients(
        CraftingRecipeSO recipe,
        PlayerInventory playerInventory)
    {
        IReadOnlyList<CraftingIngredient> ingredients =
            recipe.Ingredients;

        for (int i = 0; i < ingredients.Count; i++)
        {
            CraftingIngredient ingredient =
                ingredients[i];

            playerInventory.RemoveItem(
                ingredient.Material.MaterialName,
                ingredient.AmountRequired
            );
        }
    }


    /// <summary>
    /// Attempts to craft the selected recipe.
    ///
    /// Returns true if crafting succeeds.
    /// </summary>
    public static bool TryCraft(
        CraftingRecipeSO recipe,
        PlayerInventory playerInventory)
    {
        if (recipe == null || playerInventory == null)
        {
            return false;
        }

        if (recipe.CraftedItem == null)
        {
            return false;
        }

        // Check everything before consuming anything.
        if (!CanCraft(
                recipe,
                playerInventory))
        {
            return false;
        }

        // Remove crafting materials.
        ConsumeIngredients(
            recipe,
            playerInventory
        );

        // PlayerInventory expects a string,
        // so use the ScriptableObject's item name.
        playerInventory.AddItem(
            recipe.CraftedItem.ItemName,
            recipe.CraftedAmount
        );

        return true;
    }
}