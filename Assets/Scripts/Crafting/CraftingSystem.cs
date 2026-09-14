using UnityEngine;

public static class CraftingSystem
{
    // ADDED FOR CRAFTING:
    // Checks whether the player has every ingredient
    // required by the selected recipe.
    public static bool CanCraft(
        CrafatbleItemRecipe recipe,
        PlayerInventory inventory)
    {
        if (recipe == null ||
            inventory == null)
        {
            return false;
        }


        if (recipe.ItemsNeeded == null)
        {
            return false;
        }


        foreach (
            CrafatbleItemRecipe.Item requirement
            in recipe.ItemsNeeded)
        {
            // ADDED FOR CRAFTING:
            // Every ingredient needs a valid ScriptableItem.
            if (requirement.itemData == null)
            {
                Debug.LogWarning(
                    "Recipe " +
                    recipe.name +
                    " has an ingredient with no Item Data."
                );

                return false;
            }


            // ADDED FOR CRAFTING:
            // Ignore incorrectly configured zero/negative amounts.
            if (requirement.Quantity <= 0)
            {
                continue;
            }


            // ADDED FOR CRAFTING:
            // Check that the player owns enough of this item.
            if (!inventory.HasItem(
                    requirement.itemData,
                    requirement.Quantity))
            {
                return false;
            }
        }


        return true;
    }


    // ADDED FOR CRAFTING:
    // Performs the complete crafting transaction.
    public static bool TryCraft(
        CrafatbleItemRecipe recipe,
        PlayerInventory inventory)
    {
        if (recipe == null ||
            inventory == null)
        {
            return false;
        }


        // ADDED FOR CRAFTING:
        // Recipe needs a valid finished inventory item.
        if (recipe.craftedItemData == null)
        {
            Debug.LogWarning(
                "Recipe " +
                recipe.name +
                " does not have Crafted Item Data assigned."
            );

            return false;
        }


        // ADDED FOR CRAFTING:
        // Never consume ingredients unless every
        // requirement can be satisfied first.
        if (!CanCraft(
                recipe,
                inventory))
        {
            Debug.Log(
                "Cannot craft " +
                recipe.craftedItemData.itemName +
                ": missing required materials."
            );

            return false;
        }


        // ADDED FOR CRAFTING:
        // Consume all required ingredients.
        foreach (
            CrafatbleItemRecipe.Item requirement
            in recipe.ItemsNeeded)
        {
            if (requirement.itemData == null ||
                requirement.Quantity <= 0)
            {
                continue;
            }


            inventory.RemoveItem(
                requirement.itemData,
                requirement.Quantity
            );
        }


        // ADDED FOR CRAFTING:
        // Add one completed item to the player's inventory.
        inventory.AddItem(
            recipe.craftedItemData,
            1
        );


        Debug.Log(
            "Crafted: " +
            recipe.craftedItemData.itemName
        );


        return true;
    }
}