using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CraftingUIManager : MonoBehaviour
{
    [Header("Crafting Table")]

    // ADDED:
    // Reference to the actual crafting table.
    // The table gives this UI access to its RecipeBookSO.
    [SerializeField]
    private CraftingTable craftingTable;


    [Header("UI Elements")]

    [SerializeField]
    Transform requirementsContainer;

    [SerializeField]
    GameObject requirementRowPreFab;

    [SerializeField]
    GameObject buttonPrefab;

    [SerializeField]
    Transform contentContainer;

    [SerializeField]
    TextMeshProUGUI itemDecription;

    [SerializeField]
    Image itemIcon;


    // CHANGED:
    // This used to store the temporary CraftingRecipe struct.
    //
    // It now stores the team's real CrafatbleItemRecipe
    // ScriptableObject.
    private CrafatbleItemRecipe currentSelectedRecipe;


    private void Start()
    {
        PopulateRecipeButtons();
    }


    void PopulateRecipeButtons()
    {
        // ADDED:
        // Make sure the crafting table exists before
        // attempting to read recipes.
        if (craftingTable == null)
        {
            Debug.LogWarning(
                "CraftingUIManager does not have a CraftingTable assigned."
            );

            return;
        }


        // ADDED:
        // Make sure the crafting table has a RecipeBookSO.
        if (craftingTable.RecipeBook == null)
        {
            Debug.LogWarning(
                "CraftingTable does not have a RecipeBookSO assigned."
            );

            return;
        }


        // ADDED:
        // Clear existing buttons before rebuilding the list.
        //
        // This prevents duplicate buttons if the UI is refreshed.
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }


        // CHANGED:
        // Instead of looping through temporary mokeRecipe data,
        // loop through the real RecipeBookSO.
        foreach (
            CrafatbleItemRecipe recipe
            in craftingTable.RecipeBook.Recipes)
        {
            if (recipe == null)
            {
                continue;
            }


            GameObject newButton =
                Instantiate(
                    buttonPrefab,
                    contentContainer
                );


            TMP_Text buttonText =
                newButton.GetComponentInChildren<TMP_Text>();


            if (buttonText != null)
            {
                // ADDED:
                // Try to get the crafted item's proper inventory name.
                //
                // If the prefab is missing the CraftedItem component,
                // fall back to the recipe asset's name.
                CraftedItem craftedItem =
                    GetCraftedItem(recipe);


                if (craftedItem != null &&
                    craftedItem.InventoryItem != null)
                {
                    buttonText.text =
                        craftedItem.InventoryItem.itemName;
                }
                else
                {
                    buttonText.text =
                        recipe.name;
                }
            }


            Button btnComponent =
                newButton.GetComponent<Button>();


            if (btnComponent != null)
            {
                // ADDED:
                // Store a local copy so each generated button
                // correctly remembers its own recipe.
                CrafatbleItemRecipe recipeCopy =
                    recipe;


                btnComponent.onClick.AddListener(
                    () => OnSelectedRecipe(recipeCopy)
                );
            }
        }
    }


    // CHANGED:
    // Uses the actual CrafatbleItemRecipe asset.
    public void OnSelectedRecipe(
        CrafatbleItemRecipe selectedRecipe)
    {
        currentSelectedRecipe =
            selectedRecipe;


        UpdateRequirementUI(
            selectedRecipe
        );
    }


    // CHANGED:
    // Reads requirements directly from the team's recipe.
    private void UpdateRequirementUI(
        CrafatbleItemRecipe selectedRecipe)
    {
        if (selectedRecipe == null)
        {
            return;
        }


        // Clear old recipe requirement rows.
        foreach (
            Transform child
            in requirementsContainer)
        {
            Destroy(
                child.gameObject
            );
        }


        // ADDED:
        // Build a requirement row for every actual ingredient.
        foreach (
            CrafatbleItemRecipe.Item requirement
            in selectedRecipe.ItemsNeeded)
        {
            GameObject row =
                Instantiate(
                    requirementRowPreFab,
                    requirementsContainer
                );


            TMP_Text rowText =
                row.GetComponentInChildren<TMP_Text>();


            if (rowText != null)
            {
                int playerHasAmount = 0;


                // ADDED:
                // Check how many of this ScriptableItem
                // the player currently owns.
                if (PlayerInventory.Instance != null &&
                    requirement.itemData != null)
                {
                    playerHasAmount =
                        PlayerInventory.Instance.GetAmount(
                            requirement.itemData
                        );
                }


                // ADDED:
                // Prefer the ScriptableItem's display name.
                // Fall back to the teammate's existing ID
                // when itemData has not been assigned yet.
                string displayName =
                    requirement.itemData != null
                        ? requirement.itemData.itemName
                        : requirement.ID;


                // CHANGED:
                // Show:
                //
                // Copper Scrap: 3 / 5
                //
                // Required / Owned
                rowText.text =
                    displayName +
                    ": " +
                    requirement.Quantity +
                    " / " +
                    playerHasAmount;
            }
        }


        // ADDED:
        // Get information about the item produced
        // by this recipe.
        CraftedItem craftedItem =
            GetCraftedItem(
                selectedRecipe
            );


        if (craftedItem != null &&
            craftedItem.InventoryItem != null)
        {
            ScriptableItem itemData =
                craftedItem.InventoryItem;


            // ADDED:
            // Use the inventory item's icon for the
            // selected recipe display.
            if (itemIcon != null)
            {
                itemIcon.sprite =
                    itemData.itemIcon;
            }


            // ADDED:
            // The current ScriptableItem fields we know about
            // include itemName and itemIcon.
            //
            // Until ScriptableItem exposes a description,
            // display the item's name rather than guessing
            // at another field that may not exist.
            if (itemDecription != null)
            {
                itemDecription.text =
                    itemData.itemName;
            }
        }
        else
        {
            // ADDED:
            // Avoid displaying the previous recipe's icon
            // if this prefab is configured incorrectly.
            if (itemIcon != null)
            {
                itemIcon.sprite =
                    null;
            }


            if (itemDecription != null)
            {
                itemDecription.text =
                    selectedRecipe.name;
            }
        }
    }


    public void OnClickCraftButton()
    {
        // ADDED:
        // Do nothing until a recipe has actually been selected.
        if (currentSelectedRecipe == null)
        {
            return;
        }


        // ADDED:
        // Make sure all required systems exist.
        if (craftingTable == null ||
            PlayerInventory.Instance == null)
        {
            return;
        }


        // ADDED:
        // Send the selected recipe through:
        //
        // CraftingTable
        //      ↓
        // CraftingSystem
        //      ↓
        // PlayerInventory
        bool craftedSuccessfully =
            craftingTable.Craft(
                currentSelectedRecipe,
                PlayerInventory.Instance
            );


        if (craftedSuccessfully)
        {
            // ADDED:
            // Refresh the ingredient amounts after crafting.
            //
            // Example:
            //
            // Copper Scrap: 3 / 5
            //
            // becomes:
            //
            // Copper Scrap: 3 / 2
            UpdateRequirementUI(
                currentSelectedRecipe
            );
        }
    }


    // ADDED:
    // Helper function used by the UI to retrieve the
    // CraftedItem component from a recipe's result prefab.
    private CraftedItem GetCraftedItem(
        CrafatbleItemRecipe recipe)
    {
        if (recipe == null ||
            recipe.itemPrefab == null)
        {
            return null;
        }


        return recipe.itemPrefab
            .GetComponent<CraftedItem>();
    }
}