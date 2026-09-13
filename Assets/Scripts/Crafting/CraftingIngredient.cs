using System;
using UnityEngine;

/// <summary>
/// Represents one material requirement inside a recipe.
///
/// Example:
/// Copper Scrap x2
/// </summary>
[Serializable]
public class CraftingIngredient
{
    // CHANGED:
    // Recipes now reference the actual MaterialPickup prefab.
    //
    // Example:
    // CopperScrapPickup.prefab
    //
    // The prefab already knows which CraftingMaterialSO
    // it represents, so the designer does not have to
    // assign the material data twice.
    [SerializeField]
    private MaterialPickup materialPrefab;


    [SerializeField]
    [Min(1)]
    private int amountRequired = 1;


    // ADDED:
    // Retrieves the CraftingMaterialSO directly from the
    // MaterialPickup prefab.
    public CraftingMaterialSO Material =>
        materialPrefab != null
            ? materialPrefab.Material
            : null;


    // ADDED:
    // Exposes the actual pickup prefab in case the
    // crafting UI needs it later.
    public MaterialPickup MaterialPrefab =>
        materialPrefab;


    public int AmountRequired =>
        amountRequired;
}