using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CrafatbleItemRecipe : ScriptableObject
{
    [Serializable]
    public struct Item
    {
        public ScriptableItem itemData;
        public int Quantity;
    }

    public ScriptableItem craftedItemData;

    public GameObject itemPrefab;

    [SerializeField]
    public List<Item> ItemsNeeded;
}
