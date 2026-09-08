using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingUIManager : MonoBehaviour
{
    [Header("Crafting Table")]
    //public CraftTable craftTable; // Needs the Craft table

    [Header("UI Elements")]
    public Transform requirementsContainer;
    public GameObject requirementRowPreFab;
    public Button craftButton;

    //private CraftingRecipe currentSelectedRecipe; // Needs the Recipe book

    public void OnSelectedRecipe(CraftingRecipe selectedRecipe)
    {
        currectSelectedRecipe = selectedRecipe;
        UpdateRequirementUI(selectedRecipe)
    }
}
