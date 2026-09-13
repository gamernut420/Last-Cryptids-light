using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

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


    // Gets item amount
    public int GetAmount(ScriptableItem itemData)
    {
        if (items.ContainsKey(itemData))
        {
            return items[itemData];
        }

        return 0;
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

       for(int j = i; j < inventorySlots.Length; j++) 
        {
            inventorySlots[j].gameObject.SetActive(false);
            

        }
       
    }

    // Checks if player has enough items
    public bool HasItem(ScriptableItem itemData, int amount)
    {
        if (itemData == null)
        {
            return false;
        }

        if (!items.ContainsKey(itemData))
        {
            return false;
        }

        return items[itemData] >= amount;
    }

    // Removes items from inventory
    public bool RemoveItem(ScriptableItem itemData, int amount)
    {
        if (!HasItem(itemData, amount))
        {
            return false;
        }

        items[itemData] -= amount;

        if (items[itemData] <= 0)
        {
            items.Remove(itemData);
        }

        UpdateUI();

        return true;
    }

    // Adds a crafted item
    public void AddCraftedItem(ScriptableItem itemData, int amount)
    {
        AddItem(itemData, amount);
    }
}
