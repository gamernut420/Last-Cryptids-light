using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    // Stores item amounts
    [System.NonSerialized]
    public Dictionary<ScriptableItem, int> items =
        new Dictionary<ScriptableItem, int>();

    [Header("UI Slots Reference")]
    public InventorySlotUI[] inventorySlots;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    // Adds items
    public void AddItem(
        ScriptableItem itemData,
        int amount)
    {
        if (itemData == null)
        {
            return;
        }

        if (items.ContainsKey(itemData))
        {
            items[itemData] += amount;
        }
        else
        {
            items.Add(
                itemData,
                amount
            );
        }

        Debug.Log(
            itemData +
            ": " +
            items[itemData]
        );

        UpdateUI();
    }


    // Checks for item
    public bool HasItem(
        ScriptableItem itemData)
    {
        return items.ContainsKey(
            itemData
        );
    }


    // ADDED FOR CRAFTING:
    // Checks whether the player has at least
    // the required amount of an item.
    public bool HasItem(
        ScriptableItem itemData,
        int requiredAmount)
    {
        if (itemData == null ||
            requiredAmount <= 0)
        {
            return false;
        }

        return GetAmount(itemData)
               >= requiredAmount;
    }


    // Gets item amount
    public int GetAmount(
        ScriptableItem itemData)
    {
        if (itemData == null)
        {
            return 0;
        }

        if (items.ContainsKey(itemData))
        {
            return items[itemData];
        }

        return 0;
    }


    // ADDED FOR CRAFTING:
    // Removes a specified amount of an item.
    public bool RemoveItem(
        ScriptableItem itemData,
        int amount)
    {
        if (itemData == null ||
            amount <= 0)
        {
            return false;
        }

        if (!HasItem(
                itemData,
                amount))
        {
            return false;
        }

        items[itemData] -= amount;


        // ADDED FOR CRAFTING:
        // Remove the inventory entry completely
        // when the quantity reaches zero.
        if (items[itemData] <= 0)
        {
            items.Remove(
                itemData
            );
        }

        UpdateUI();

        return true;
    }


    // Update inventory UI in GM
    void UpdateUI()
    {
        int i = 0;

        foreach (
            KeyValuePair<ScriptableItem, int> kvp
            in items)
        {
            if (i < inventorySlots.Length)
            {
                inventorySlots[i]
                    .gameObject
                    .SetActive(true);

                inventorySlots[i]
                    .UpdateSlot(
                        kvp.Key.itemIcon,
                        kvp.Key.itemName,
                        kvp.Value
                    );

                i++;
            }
        }


        for (
            int j = i;
            j < inventorySlots.Length;
            j++)
        {
            inventorySlots[j]
                .gameObject
                .SetActive(false);
        }
    }
}