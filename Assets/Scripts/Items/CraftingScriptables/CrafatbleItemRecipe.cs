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
        // This lets recipes use drag-and-drop item assets instead of
        // depending only on string IDs.
        //
        // The existing ID field is intentionally kept so teammate code
        // that may still use it is not broken.
        public ScriptableItem itemData;
    }


    public GameObject itemPrefab;


    [SerializeField]
    public List<Item> ItemsNeeded;
}