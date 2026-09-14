using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance
    {
        get;
        private set;
    }


    // Stores actual inventory quantities.
    [System.NonSerialized]
    public Dictionary<ScriptableItem, int> items =
        new Dictionary<ScriptableItem, int>();


    [Header("UI Slots Reference")]
    public InventorySlotUI[] inventorySlots;


    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
    }


    private void Start()
    {
        // ADDED FOR INVENTORY SLOTS:
        // Make sure all assigned slots begin empty.
        if (inventorySlots == null)
        {
            return;
        }


        foreach (
            InventorySlotUI slot
            in inventorySlots)
        {
            if (slot != null)
            {
                slot.ClearSlot();

                // Keep the 1–4 slots visible.
                slot.gameObject.SetActive(true);
            }
        }
    }


    // Adds items.
    public void AddItem(
        ScriptableItem itemData,
        int amount)
    {
        if (itemData == null ||
            amount <= 0)
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
            itemData.itemName +
            ": " +
            items[itemData]
        );


        // ADDED FOR INVENTORY SLOTS:
        // Update the matching slot or put this item
        // into the first available slot.
        AddOrUpdateSlot(
            itemData
        );
    }


    // Checks for item.
    public bool HasItem(
        ScriptableItem itemName)
    {
        return items.ContainsKey(
            itemName
        );
    }


    // ADDED FOR CRAFTING:
    // Checks whether the player has the required
    // quantity of an item.
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


    // Gets item amount.
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
    // Removes a specified quantity of an item.
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
        // Remove empty entries completely.
        if (items[itemData] <= 0)
        {
            items.Remove(
                itemData
            );
        }


        // ADDED FOR INVENTORY SLOTS:
        // Refresh the specific slot containing this item.
        RefreshSlot(
            itemData
        );


        return true;
    }


    // ADDED FOR INVENTORY SLOTS:
    // If the item is already assigned to a slot,
    // update its stack count.
    //
    // Otherwise use the first empty slot:
    // 1, then 2, then 3, then 4.
    private void AddOrUpdateSlot(
        ScriptableItem itemData)
    {
        if (inventorySlots == null)
        {
            return;
        }


        int currentAmount =
            GetAmount(itemData);


        // First check whether this exact item
        // already occupies one of the slots.
        foreach (
            InventorySlotUI slot
            in inventorySlots)
        {
            if (slot == null)
            {
                continue;
            }


            if (slot.CurrentItem ==
                itemData)
            {
                slot.SetItem(
                    itemData,
                    currentAmount
                );

                return;
            }
        }


        // ADDED FOR INVENTORY SLOTS:
        // Item is not currently assigned,
        // so find the first available slot.
        foreach (
            InventorySlotUI slot
            in inventorySlots)
        {
            if (slot == null)
            {
                continue;
            }


            if (slot.IsEmpty)
            {
                slot.SetItem(
                    itemData,
                    currentAmount
                );

                return;
            }
        }


        // ADDED FOR INVENTORY SLOTS:
        // Inventory still contains the item,
        // but all four quick slots are occupied.
        Debug.Log(
            "No empty inventory slot available for " +
            itemData.itemName
        );
    }


    // ADDED FOR INVENTORY SLOTS:
    // Updates or clears the slot representing
    // a particular ScriptableItem.
    private void RefreshSlot(
        ScriptableItem itemData)
    {
        if (inventorySlots == null)
        {
            return;
        }


        foreach (
            InventorySlotUI slot
            in inventorySlots)
        {
            if (slot == null)
            {
                continue;
            }


            if (slot.CurrentItem !=
                itemData)
            {
                continue;
            }


            int amount =
                GetAmount(itemData);


            if (amount <= 0)
            {
                slot.ClearSlot();
            }
            else
            {
                slot.SetItem(
                    itemData,
                    amount
                );
            }


            return;
        }
    }


    // ADDED FOR INVENTORY SLOTS:
    // Full rebuild if another system needs to force
    // the UI to synchronize with the inventory.
    public void UpdateUI()
    {
        if (inventorySlots == null)
        {
            return;
        }


        foreach (
            InventorySlotUI slot
            in inventorySlots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
                slot.gameObject.SetActive(true);
            }
        }


        foreach (
            KeyValuePair<ScriptableItem, int> kvp
            in items)
        {
            AddOrUpdateSlot(
                kvp.Key
            );
        }
    }
}