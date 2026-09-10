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

    // ADDED FOR CRAFTING:
    // Allows recipes to check whether the player has a specific
    // quantity of an item instead of only checking if it exists.
    public bool HasItem(string itemName, int requiredAmount)
    {
        if (requiredAmount <= 0)
        {
            return false;
        }

        return GetAmount(itemName) >= requiredAmount;
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

    // ADDED FOR CRAFTING:
    // Removes a specified amount of an item when a recipe is crafted.
    // Returns false if the player does not have enough of the item.
    public bool RemoveItem(string itemName, int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (!HasItem(itemName, amount))
        {
            return false;
        }

        items[itemName] -= amount;

        // ADDED FOR CRAFTING:
        // Removes the inventory entry entirely if the amount reaches zero.
        if (items[itemName] <= 0)
        {
            items.Remove(itemName);
        }

        UpdateUI();

        return true;
    }

    // Update inventory UI in GM
    void UpdateUI()
    {
        string tempText = string.Empty;

        foreach (string item in items.Keys)
        {
            tempText += item + ": " + GetAmount(item) + "\n";
        }

        if (UpdateInventoryText != null)
        {
            UpdateInventoryText(tempText);
        }
    }
}