using UnityEngine;

/// <summary>
/// Defines a type of crafting material used by the player.
///
/// Examples:
/// - Spore Pod
/// - Copper Scrap
/// - Chemical Solvent
/// - Spent Battery Cell
///
/// This is NOT the physical pickup object.
/// The actual pickup is a prefab using MaterialPickup.cs.
/// </summary>
[CreateAssetMenu(
    fileName = "NewCraftingMaterial",
    menuName = "Operation Riftfall/Crafting/Material")]
public class CraftingMaterialSO : ScriptableObject
{
    [Header("Material Information")]

    [SerializeField]
    private string materialName;

    [SerializeField]
    [TextArea]
    private string description;

    [SerializeField]
    private Sprite icon;

    [Header("Inventory")]

    [SerializeField]
    [Min(1)]
    private int maxStackSize = 99;


    // Public read-only accessors.
    // Other systems can read these values,
    // but they cannot accidentally change them at runtime.

    public string MaterialName => materialName;

    public string Description => description;

    public Sprite Icon => icon;

    public int MaxStackSize => maxStackSize;
}
