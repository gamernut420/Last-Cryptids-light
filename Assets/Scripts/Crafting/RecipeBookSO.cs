using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a collection of crafting recipes that can be used
/// by a crafting table or crafting UI.
///
/// This recipe book now uses the team's existing
/// CrafatbleItemRecipe ScriptableObjects.
/// </summary>
[CreateAssetMenu(
    fileName = "NewRecipeBook",
    menuName = "Operation Riftfall/Crafting/Recipe Book")]
public class RecipeBookSO : ScriptableObject
{
    [Header("Recipes")]

    // CHANGED:
    // RecipeBook now stores the team's existing
    // CrafatbleItemRecipe assets instead of CraftingRecipeSO.
    [SerializeField]
    private List<CrafatbleItemRecipe> recipes =
        new List<CrafatbleItemRecipe>();


    /// <summary>
    /// Read-only access to every recipe stored in this book.
    ///
    /// This allows the crafting table and crafting UI to read
    /// recipes without being able to directly replace the list.
    /// </summary>
    public IReadOnlyList<CrafatbleItemRecipe> Recipes =>
        recipes;


    // ADDED:
    // Returns the number of recipes currently stored in the book.
    // Useful for crafting UI generation and debugging.
    public int RecipeCount =>
        recipes.Count;


    // ADDED:
    // Safely returns a recipe by index.
    //
    // Returns null if the requested index is outside
    // the valid recipe range.
    public CrafatbleItemRecipe GetRecipe(int index)
    {
        if (index < 0 ||
            index >= recipes.Count)
        {
            return null;
        }

        return recipes[index];
    }


    // ADDED:
    // Checks whether a specific recipe exists in this recipe book.
    public bool ContainsRecipe(
        CrafatbleItemRecipe recipe)
    {
        if (recipe == null)
        {
            return false;
        }

        return recipes.Contains(recipe);
    }
}