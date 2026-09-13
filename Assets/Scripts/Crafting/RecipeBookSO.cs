using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewRecipeBook",
    menuName = "Operation Riftfall/Crafting/Recipe Book")]
public class RecipeBookSO : ScriptableObject
{
    [Header("Recipes")]

    [SerializeField]
    private List<CrafatbleItemRecipe> recipes =
        new List<CrafatbleItemRecipe>();


    public IReadOnlyList<CrafatbleItemRecipe> Recipes =>
        recipes;


    public int RecipeCount =>
        recipes.Count;


    public CrafatbleItemRecipe GetRecipe(
        int index)
    {
        if (index < 0 ||
            index >= recipes.Count)
        {
            return null;
        }


        return recipes[index];
    }


    public bool ContainsRecipe(
        CrafatbleItemRecipe recipe)
    {
        if (recipe == null)
        {
            return false;
        }


        return recipes.Contains(
            recipe
        );
    }
}