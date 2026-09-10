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
    public int GetAmount(ScriptableItem itemName)
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
        int index = 0;

        foreach (KeyValuePair<ScriptableItem, int> pair in items)
        {
            inventorySlots[index].SetItem(pair.Key, pair.Value);
            index++;
        }

        for(int i = index; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].ClearSlot();
        }
    }
}
