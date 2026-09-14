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
            if (requirement.itemData == null)
            {
                Debug.LogWarning(
                    "Recipe " +
                    recipe.name +
                    " has an ingredient with no Item Data."
                );

                return false;
            }


            if (requirement.Quantity <= 0)
            {
                continue;
            }


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


        if (recipe.craftedItemData == null)
        {
            Debug.LogWarning(
                "Recipe " +
                recipe.name +
                " does not have Crafted Item Data assigned."
            );

            return false;
        }


        // Never consume anything unless every
        // ingredient is available first.
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


        // Consume ingredients.
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
        // Finished crafts enter the normal inventory
        // AND the first available slot 1-4.
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