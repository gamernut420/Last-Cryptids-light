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
    }

    public GameObject itemPrefab;

    
    [SerializeField] public List<Item> ItemsNeeded;
}
