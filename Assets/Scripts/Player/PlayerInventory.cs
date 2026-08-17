using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{

    // Stores item amounts
    Dictionary<string, int> items = new Dictionary<string, int>();

    public static System.Action<string> UpdateInventoryText;

    // Adds items
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
        UpdateUI();
    }

    // Checks for item
    public bool HasItem(string itemName)
    {
        return items.ContainsKey(itemName);
    }


    // Gets item amount
    public int GetAmount(string itemName)
    {
        if (items.ContainsKey(itemName))
        {
            return items[itemName];
        }

        return 0;
    }


    // Update inventory UI in GM
    void UpdateUI()
    {
        string tempText = string.Empty;

        foreach (string item in items.Keys)
        {
            tempText += item + ": " + GetAmount(item) + "\n";
        }

        if(UpdateInventoryText != null)
        {
            UpdateInventoryText(tempText);
        }
    }
}
