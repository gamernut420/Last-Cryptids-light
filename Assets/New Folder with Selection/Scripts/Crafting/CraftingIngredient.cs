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
    [SerializeField]
    private CraftingMaterialSO material;

    [SerializeField]
    [Min(1)]
    private int amountRequired = 1;


    public CraftingMaterialSO Material => material;

    public int AmountRequired => amountRequired;
}