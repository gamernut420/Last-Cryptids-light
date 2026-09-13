using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingUIManager : MonoBehaviour
{
    [Header("Crafting Table")]

    [SerializeField]
    private CraftingTable craftingTable;


    [Header("UI Toggle")]

    [SerializeField]
    private GameObject craftingPanel;


    [SerializeField]
    private InventoryToggle inventoryToggle;


    [Header("UI Elements")]

    [SerializeField]
    private Transform requirementsContainer;

    [SerializeField]
    private GameObject requirementRowPreFab;

    [SerializeField]
    private GameObject buttonPrefab;

    [SerializeField]
    private Transform contentContainer;

    [SerializeField]
    private TextMeshProUGUI itemDecription;

    [SerializeField]
    private Image itemIcon;


    // ADDED FOR CRAFTING:
    private CrafatbleItemRecipe currentSelectedRecipe;


    // ADDED FOR CRAFTING UI:
    private bool craftingUIOpen;


    private void Start()
    {
        craftingUIOpen =
            false;


        if (craftingPanel != null)
        {
            craftingPanel.SetActive(
                false
            );
        }


        PopulateRecipeButtons();
    }


    // ADDED FOR CRAFTING UI:
    // Escape closes the crafting screen.
    private void Update()
    {
        if (craftingUIOpen &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCraftingUI();
        }
    }


    public void ToggleCraftingUI()
    {
        if (craftingUIOpen)
        {
            CloseCraftingUI();
        }
        else
        {
            OpenCraftingUI();
        }
    }


    public void OpenCraftingUI()
    {
        craftingUIOpen =
            true;


        if (craftingPanel != null)
        {
            craftingPanel.SetActive(
                true
            );
        }
        else
        {
            Debug.LogWarning(
                "CraftingUIManager does not have a CraftingPanel assigned."
            );
        }


        // ADDED FOR CRAFTING:
        // Refresh recipes whenever the table opens.
        PopulateRecipeButtons();


        if (inventoryToggle != null)
        {
            inventoryToggle.OpenInventory();
        }


        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible =
            true;
    }


    public void CloseCraftingUI()
    {
        craftingUIOpen =
            false;


        if (craftingPanel != null)
        {
            craftingPanel.SetActive(
                false
            );
        }


        if (inventoryToggle != null)
        {
            inventoryToggle.CloseInventory();
        }


        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible =
            false;
    }


    public bool IsCraftingUIOpen()
    {
        return craftingUIOpen;
    }


    private void PopulateRecipeButtons()
    {
        if (craftingTable == null)
        {
            Debug.LogWarning(
                "CraftingUIManager does not have a CraftingTable assigned."
            );

            return;
        }


        if (craftingTable.RecipeBook == null)
        {
            Debug.LogWarning(
                "CraftingTable does not have a RecipeBookSO assigned."
            );

            return;
        }


        if (contentContainer == null)
        {
            Debug.LogWarning(
                "CraftingUIManager does not have a Content Container assigned."
            );

            return;
        }


        if (buttonPrefab == null)
        {
            Debug.LogWarning(
                "CraftingUIManager does not have a Button Prefab assigned."
            );

            return;
        }


        // ADDED FOR CRAFTING:
        // Remove old generated recipe buttons.
        foreach (
            Transform child
            in contentContainer)
        {
            Destroy(
                child.gameObject
            );
        }


        // ADDED FOR CRAFTING:
        // Display every recipe in the recipe book.
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


            // CHANGED FOR CRAFTING:
            // Use Crafted Item Data directly.
            if (buttonText != null)
            {
                if (recipe.craftedItemData != null)
                {
                    buttonText.text =
                        recipe.craftedItemData.itemName;
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
                CrafatbleItemRecipe recipeCopy =
                    recipe;


                // ADDED FOR CRAFTING:
                // Recipes stay selectable even when
                // materials are missing so requirements
                // can still be inspected.
                btnComponent.interactable =
                    true;


                btnComponent.onClick.AddListener(
                    () =>
                        OnSelectedRecipe(
                            recipeCopy
                        )
                );
            }
        }
    }


    public void OnSelectedRecipe(
        CrafatbleItemRecipe selectedRecipe)
    {
        if (selectedRecipe == null)
        {
            return;
        }


        currentSelectedRecipe =
            selectedRecipe;


        UpdateRequirementUI(
            selectedRecipe
        );
    }


    private void UpdateRequirementUI(
        CrafatbleItemRecipe selectedRecipe)
    {
        if (selectedRecipe == null)
        {
            return;
        }


        if (requirementsContainer == null)
        {
            Debug.LogWarning(
                "CraftingUIManager does not have a Requirements Container assigned."
            );

            return;
        }


        // ADDED FOR CRAFTING:
        // Clear previous requirement rows.
        foreach (
            Transform child
            in requirementsContainer)
        {
            Destroy(
                child.gameObject
            );
        }


        // ADDED FOR CRAFTING:
        // Build ingredient display rows.
        foreach (
            CrafatbleItemRecipe.Item requirement
            in selectedRecipe.ItemsNeeded)
        {
            if (requirementRowPreFab == null)
            {
                continue;
            }


            GameObject row =
                Instantiate(
                    requirementRowPreFab,
                    requirementsContainer
                );


            TMP_Text rowText =
                row.GetComponentInChildren<TMP_Text>();


            if (rowText != null)
            {
                int playerHasAmount =
                    0;


                if (PlayerInventory.Instance != null &&
                    requirement.itemData != null)
                {
                    playerHasAmount =
                        PlayerInventory.Instance.GetAmount(
                            requirement.itemData
                        );
                }


                string displayName =
                    requirement.itemData != null
                        ? requirement.itemData.itemName
                        : requirement.ID;


                rowText.text =
                    displayName +
                    ": " +
                    requirement.Quantity +
                    " / " +
                    playerHasAmount;
            }
        }


        // CHANGED FOR CRAFTING:
        // Read selected item information directly
        // from Crafted Item Data.
        ScriptableItem craftedItemData =
            selectedRecipe.craftedItemData;


        if (craftedItemData != null)
        {
            if (itemDecription != null)
            {
                itemDecription.text =
                    craftedItemData.itemDescription;
            }


            if (itemIcon != null)
            {
                itemIcon.sprite =
                    craftedItemData.itemIcon;


                itemIcon.enabled =
                    craftedItemData.itemIcon != null;
            }
        }
        else
        {
            if (itemDecription != null)
            {
                itemDecription.text =
                    selectedRecipe.name;
            }


            if (itemIcon != null)
            {
                itemIcon.sprite =
                    null;

                itemIcon.enabled =
                    false;
            }
        }
    }


    // ADDED FOR CRAFTING:
    // Hook the CRAFT button's OnClick event
    // to this function.
    public void OnClickCraftButton()
    {
        if (currentSelectedRecipe == null)
        {
            return;
        }


        if (craftingTable == null ||
            PlayerInventory.Instance == null)
        {
            return;
        }


        // ADDED FOR CRAFTING:
        // Prevent crafting unless all ingredients exist.
        if (!craftingTable.CanCraft(
                currentSelectedRecipe,
                PlayerInventory.Instance))
        {
            Debug.Log(
                "Cannot craft " +
                currentSelectedRecipe.name +
                ": missing required materials."
            );


            UpdateRequirementUI(
                currentSelectedRecipe
            );


            return;
        }


        bool craftedSuccessfully =
            craftingTable.Craft(
                currentSelectedRecipe,
                PlayerInventory.Instance
            );


        if (craftedSuccessfully)
        {
            // ADDED FOR CRAFTING:
            // Update requirement counts after materials
            // have been consumed.
            UpdateRequirementUI(
                currentSelectedRecipe
            );


            // ADDED FOR CRAFTING:
            // Refresh recipe list after inventory changes.
            PopulateRecipeButtons();
        }
    }
}