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
            // Ignore zero or negative requirements.
            if (requirement.Quantity <= 0)
            {
                continue;
            }


            // ADDED FOR CRAFTING:
            // Make sure the player owns enough
            // of this exact ScriptableItem.
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
    // Handles the entire crafting transaction.
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
        // Make sure the recipe has a finished
        // ScriptableItem assigned.
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
        // Never consume materials unless the
        // complete recipe can be crafted.
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
        // Consume every required ingredient.
        foreach (
            CrafatbleItemRecipe.Item requirement
            in recipe.ItemsNeeded)
        {
            if (requirement.itemData == null ||
                requirement.Quantity <= 0)
            {
                continue;
            }


            bool removed =
                inventory.RemoveItem(
                    requirement.itemData,
                    requirement.Quantity
                );


            // ADDED FOR CRAFTING:
            // This should normally never fail because
            // CanCraft() already validated everything,
            // but stop if inventory changes unexpectedly.
            if (!removed)
            {
                Debug.LogWarning(
                    "Crafting failed while removing " +
                    requirement.itemData.itemName
                );

                return false;
            }
        }


        // CHANGED FOR QUICK SLOTS:
        // Add the completed item to the normal inventory
        // and place it into the first available quick slot.
        inventory.AddCraftedItem(
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