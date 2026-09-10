using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains all recipes available to a particular
/// crafting station.
///
/// Different crafting tables can use different books.
/// </summary>
[CreateAssetMenu(
    fileName = "NewRecipeBook",
    menuName = "Operation Riftfall/Crafting/Recipe Book")]
public class RecipeBookSO : ScriptableObject
{
    [SerializeField]
    private List<CraftingRecipeSO> recipes =
        new List<CraftingRecipeSO>();


    public IReadOnlyList<CraftingRecipeSO> Recipes =>
        recipes;
}