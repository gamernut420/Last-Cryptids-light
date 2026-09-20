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


        // CHANGED FOR HUD QUICK SLOTS:
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