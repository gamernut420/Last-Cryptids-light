using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
<<<<<<< HEAD:Assets/New Folder with Selection/Scripts/Player/PlayerInventory.cs
=======
    public static PlayerInventory Instance { get; private set; }

>>>>>>> 6e12d616291331462449f8f33cd9aabbb51fb23d:Assets/Scripts/Player/PlayerInventory.cs
    // Stores item amounts
    [System.NonSerialized]
    public Dictionary<ScriptableItem, int> items = new Dictionary<ScriptableItem, int>();

    [Header("UI Slots Reference")]
    public InventorySlotUI[] inventorySlots;

    private void Awake()
    {
        if(Instance != null && Instance!=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Adds items
    public void AddItem(ScriptableItem itemData, int amount)
    {
        if (itemData == null) return;

        if(items.ContainsKey(itemData))
            items[itemData] += amount;
        else
            items.Add(itemData, amount);

        Debug.Log(itemData + ": " + items[itemData]);
        UpdateUI();
    }

    // Checks for item
    public bool HasItem(ScriptableItem itemName)
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
    public int GetAmount(ScriptableItem itemData)
    {
        if (items.ContainsKey(itemData))
        {
            return items[itemData];
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
        int i = 0;
       foreach(KeyValuePair<ScriptableItem, int> kvp in items)
        {
            if(i < inventorySlots.Length)
            {
                inventorySlots[i].gameObject.SetActive(true);
                inventorySlots[i].UpdateSlot(kvp.Key.itemIcon, kvp.Key.itemName, kvp.Value);
                i++;
            }
        }

<<<<<<< HEAD:Assets/New Folder with Selection/Scripts/Player/PlayerInventory.cs
        if (UpdateInventoryText != null)
=======
       for(int j = i; j < inventorySlots.Length; j++) 
>>>>>>> 6e12d616291331462449f8f33cd9aabbb51fb23d:Assets/Scripts/Player/PlayerInventory.cs
        {
            inventorySlots[j].gameObject.SetActive(false);
            

        }
       
    }
}