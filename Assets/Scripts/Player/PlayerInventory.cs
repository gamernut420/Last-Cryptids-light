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
}
