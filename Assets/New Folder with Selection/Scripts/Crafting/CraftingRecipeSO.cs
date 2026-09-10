using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines one crafting recipe.
///
/// The recipe contains:
/// - Ingredients
/// - Result item
/// - Result quantity
///
/// The crafting table performs the actual crafting logic.
/// </summary>
[CreateAssetMenu(
    fileName = "NewCraftingRecipe",
    menuName = "Operation Riftfall/Crafting/Recipe")]
public class CraftingRecipeSO : ScriptableObject
{
    [Header("Recipe Information")]

    [SerializeField]
    private string recipeName;

    [SerializeField]
    [TextArea]
    private string description;

    [SerializeField]
    private Sprite icon;

    [Header("Result")]

    [SerializeField]
    private ItemSO craftedItem;

    [SerializeField]
    [Min(1)]
    private int craftedAmount = 1;

    [Header("Requirements")]

    [SerializeField]
    private List<CraftingIngredient> ingredients =
        new List<CraftingIngredient>();


    public string RecipeName => recipeName;

    public string Description => description;

    public Sprite Icon => icon;

    public ItemSO CraftedItem => craftedItem;

    public int CraftedAmount => craftedAmount;

    public IReadOnlyList<CraftingIngredient> Ingredients =>
        ingredients;
}

