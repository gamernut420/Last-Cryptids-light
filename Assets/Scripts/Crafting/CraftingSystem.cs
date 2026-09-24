using UnityEngine;

public static class CraftingSystem
{
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


        // Add the successfully crafted item
        // to the player's inventory.
        inventory.AddCraftedItem(
            recipe.craftedItemData,
            1
        );


        Debug.Log(
            "Crafted: " +
            recipe.craftedItemData.itemName
        );


        // ADDED:
        // Any successful craft completes the
        // "Craft an Item" objective.
        //
        // ObjectiveManager handles:
        // - marking the objective complete
        // - unlocking the next objective
        // - updating the compass target
        // - triggering the mission UI state change
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance
                .CompleteObjective(
                    "secondary_craft_an_item"
                );
        }


        return true;
    }
}