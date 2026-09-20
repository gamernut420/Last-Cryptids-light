using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class CrafatbleItemRecipe : ScriptableObject
{
    [Serializable]
    public struct Item
    {
        public string ID;

        public int Quantity;


        // ADDED FOR CRAFTING:
        // Direct reference to the ScriptableItem used by PlayerInventory.
        //
        // This allows ingredients to be assigned by dragging
        // ScriptableItem assets into the recipe.
        //
        // The existing ID field is kept for compatibility
        // with teammate code.
        public ScriptableItem itemData;
    }


    public GameObject itemPrefab;


    // ADDED FOR CRAFTING:
    // ScriptableItem representation of the item produced
    // by this recipe.
    //
    // Example:
    //
    // itemPrefab:
    // CraftableExposureFlare.prefab
    //
    // craftedItemData:
    // SI_ExposureFlare
    public ScriptableItem craftedItemData;


    [SerializeField]
    public List<Item> ItemsNeeded;
}