using UnityEngine;

/// <summary>
/// General category used for inventory organization
/// and UI filtering.
/// </summary>
public enum ItemType
{
    Consumable,
    Weapon,
    Equipment,
    Gadget,
    QuestItem
}

/// <summary>
/// Base data definition for an item.
///
/// Crafted items reference this ScriptableObject
/// instead of spawning a physical GameObject immediately.
/// </summary>
public abstract class ItemSO : ScriptableObject
{
    [Header("Item Information")]

    [SerializeField]
    private string itemName;

    [SerializeField]
    [TextArea]
    private string description;

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private ItemType itemType;

    [Header("Inventory")]

    [SerializeField]
    private bool stackable = true;

    [SerializeField]
    [Min(1)]
    private int maxStackSize = 99;


    public string ItemName => itemName;

    public string Description => description;

    public Sprite Icon => icon;

    public ItemType ItemType => itemType;

    public bool Stackable => stackable;

    public int MaxStackSize => maxStackSize;
}
