using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines one crafting recipe.
///
/// The recipe contains:
/// - Ingredient pickup prefabs
/// - Result prefab
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


    // CHANGED:
    // Recipes now produce a prefab instead of only an ItemSO.
    //
    // This allows every crafted item prefab to contain its
    // own unique behavior scripts.
    //
    // Examples:
    //
    // MiniBeacon
    // - BatteryPoweredItem
    // - EnemyDeterrentZone
    // - HealingZone
    // - SafeZone
    //
    // Flare
    // - EnemyDeterrentZone
    //
    // Air Purifier
    // - BatteryPoweredItem
    // - EnvironmentProtection
    [SerializeField]
    private GameObject craftedItemPrefab;


    [SerializeField]
    [Min(1)]
    private int craftedAmount = 1;


    [Header("Requirements")]

    [SerializeField]
    private List<CraftingIngredient> ingredients =
        new List<CraftingIngredient>();


    public string RecipeName =>
        recipeName;

    public string Description =>
        description;

    public Sprite Icon =>
        icon;


    // ADDED:
    // Provides access to the physical prefab created
    // by this recipe.
    public GameObject CraftedItemPrefab =>
        craftedItemPrefab;


    public int CraftedAmount =>
        craftedAmount;

    public IReadOnlyList<CraftingIngredient> Ingredients =>
        ingredients;
}