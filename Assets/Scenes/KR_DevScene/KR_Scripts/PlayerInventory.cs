using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    Dictionary<string, int> items = new Dictionary<string, int>();

    public void AddItem(string itemName, int amount)
    {
        if (items.ContainsKey(itemName))
        {
            items[itemName] += amount;
        }
        else
        {
            items.Add(itemName, amount);
        }

        Debug.Log(itemName + ": " + items[itemName]);
    }

    public bool HasItem(string itemName)
    {
        return items.ContainsKey(itemName);
    }

    public int GetAmount(string itemName)
    {
        if (items.ContainsKey(itemName))
        {
            return items[itemName];
        }

        return 0;
    }
}
