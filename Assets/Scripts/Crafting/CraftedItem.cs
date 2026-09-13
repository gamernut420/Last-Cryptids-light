using UnityEngine;

/// <summary>
/// Connects a crafted prefab to the ScriptableItem
/// stored by PlayerInventory.
/// </summary>
public class CraftedItem : MonoBehaviour
{
    [Header("Inventory Data")]

    // ADDED:
    // ScriptableItem representation of this prefab.
    //
    // Example:
    //
    // MiniBeacon.prefab
    //     -> SI_MiniBeacon
    [SerializeField]
    private ScriptableItem inventoryItem;


    public ScriptableItem InventoryItem =>
        inventoryItem;
}
