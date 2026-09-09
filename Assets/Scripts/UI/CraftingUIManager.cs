using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
//Delete after crafting table and recipe book is done ---->
[System.Serializable]
public struct IngredientRequirement
{
    public string itemName;
    public int requiredAmount;
}

[System.Serializable]
public struct CraftingRecipe
{
    public string recipeName;
    public Sprite recipeIcon;
    public List<IngredientRequirement> requiredIngredients;
}
/// ----|
public class CraftingUIManager : MonoBehaviour
{
    //Delete after crafting table and recipe book is done --->
    [Header("Moke Test Data")]
    public List<CraftingRecipe> mokeRecipe;


    /// -----|

    [Header("Crafting Table")]
    //public CraftTable craftTable; // Needs the Craft table
   
    [Header("UI Elements")]
    public Transform requirementsContainer;
    public GameObject requirementRowPreFab;
    public Button craftButton;
    public GameObject buttonPrefab;
    public Transform contentContainer;

    private void Start()
    {
        PopulateRecipeButtons();
    }

    void PopulateRecipeButtons()
    {
        foreach (var recipe in mokeRecipe)
        {
            GameObject newButton = Instantiate(buttonPrefab, contentContainer);
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = recipe.recipeName;

            // Set button text or icon here
            Button btnComponent = newButton.GetComponent<Button>();
            if (btnComponent != null) btnComponent.onClick.AddListener(() => OnSelectedRecipe(recipe));
        }
    }

    private CraftingRecipe currentSelectedRecipe; // Needs the Recipe book

    public void OnSelectedRecipe(CraftingRecipe selectedRecipe)
    {
        currentSelectedRecipe = selectedRecipe;
        UpdateRequirementUI(selectedRecipe);
    }

    private void UpdateRequirementUI(CraftingRecipe selectedRecipe)
    {
        // Clear old recipe
        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);

        // New recipe
        foreach (var requirement in selectedRecipe.requiredIngredients)
        {
            GameObject row = Instantiate(requirementRowPreFab, requirementsContainer);

            TMP_Text rowText = row.GetComponentInChildren<TMP_Text>();
            if (rowText != null)
                rowText.text = $"{requirement.itemName}: {requirement.requiredAmount}";
        }
    }

    public void OnClickCraftButton()
    {
        //if (currentSelectedRecipe != null && craftingTable != null)
        //{
        //    craftingTable.craftItem(currentSelectedRecipe);
        //}
    }

}
