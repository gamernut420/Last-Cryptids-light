using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance
    {
        get;
        private set;
    }


    // Stores ALL actual inventory item amounts.
    [System.NonSerialized]
    public Dictionary<ScriptableItem, int> items =
        new Dictionary<ScriptableItem, int>();


    [Header("Overall Inventory UI")]

    // Existing large inventory grid.
    public InventorySlotUI[] inventorySlots;


    // ADDED FOR CRAFTED ITEMS:
    // These are the numbered HUD slots 1-4.
    [Header("Quick Slots 1-4")]
    public InventorySlotUI[] quickSlots;


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
        // ADDED FOR QUICK SLOTS:
        // Make sure the four hotbar slots begin empty
        // but remain visible on the HUD.
        if (quickSlots != null)
        {
            foreach (
                InventorySlotUI slot
                in quickSlots)
            {
                if (slot == null)
                {
                    continue;
                }


                slot.ClearSlot();

                slot.gameObject.SetActive(
                    true
                );
            }
        }


        // Refresh the normal inventory display.
        UpdateUI();
    }


    // Adds a normal item to the player's inventory.
    //
    // Materials picked up in the world use this.
    // They do NOT automatically enter slots 1-4.
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


        // Update the large inventory grid.
        UpdateUI();
    }


    // ADDED FOR CRAFTING:
    // Adds a completed crafted item to the normal
    // inventory AND assigns it to slots 1-4.
    public void AddCraftedItem(
        ScriptableItem itemData,
        int amount)
    {
        if (itemData == null ||
            amount <= 0)
        {
            return;
        }


        // First add it to the actual inventory.
        AddItem(
            itemData,
            amount
        );


        // Then display it in the quick-slot system.
        AddOrUpdateQuickSlot(
            itemData
        );
    }


    // Checks whether the inventory contains an item.
    public bool HasItem(
        ScriptableItem itemData)
    {
        if (itemData == null)
        {
            return false;
        }


        return items.ContainsKey(
            itemData
        );
    }


    // ADDED FOR CRAFTING:
    // Checks whether enough of an item is owned.
    public bool HasItem(
        ScriptableItem itemData,
        int requiredAmount)
    {
        if (itemData == null ||
            requiredAmount <= 0)
        {
            return false;
        }


        return GetAmount(
            itemData
        ) >= requiredAmount;
    }


    // Gets the current quantity of an item.
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


    // Removes a quantity from inventory.
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


        // Remove empty entries completely.
        if (items[itemData] <= 0)
        {
            items.Remove(
                itemData
            );
        }


        // Update normal inventory.
        UpdateUI();


        // ADDED FOR QUICK SLOTS:
        // If the consumed item is also present in
        // slots 1-4, update or clear that slot.
        RefreshQuickSlot(
            itemData
        );


        return true;
    }


    // ADDED FOR QUICK SLOTS:
    // If this item is already assigned to a quick slot,
    // update its displayed stack.
    //
    // Otherwise place it in the first empty slot:
    // 1 -> 2 -> 3 -> 4.
    private void AddOrUpdateQuickSlot(
        ScriptableItem itemData)
    {
        if (quickSlots == null ||
            quickSlots.Length == 0)
        {
            Debug.LogWarning(
                "PlayerInventory has no Quick Slots assigned."
            );

            return;
        }


        int currentAmount =
            GetAmount(
                itemData
            );


        // First see whether this exact item is
        // already in one of the four slots.
        foreach (
            InventorySlotUI slot
            in quickSlots)
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


        // Not already assigned.
        // Find first empty slot.
        foreach (
            InventorySlotUI slot
            in quickSlots)
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


        // Inventory still receives the item even
        // if all four quick slots are occupied.
        Debug.Log(
            "All quick slots are full. " +
            itemData.itemName +
            " remains in the normal inventory."
        );
    }


    // ADDED FOR QUICK SLOTS:
    // Called when an item quantity changes.
    private void RefreshQuickSlot(
        ScriptableItem itemData)
    {
        if (quickSlots == null)
        {
            return;
        }


        foreach (
            InventorySlotUI slot
            in quickSlots)
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


            int currentAmount =
                GetAmount(
                    itemData
                );


            if (currentAmount <= 0)
            {
                slot.ClearSlot();

                // Keep the numbered HUD slot visible.
                slot.gameObject.SetActive(
                    true
                );
            }
            else
            {
                slot.SetItem(
                    itemData,
                    currentAmount
                );
            }


            return;
        }
    }


    // Updates the LARGE inventory grid.
    public void UpdateUI()
    {
        if (inventorySlots == null)
        {
            return;
        }


        int i =
            0;


        foreach (
            KeyValuePair<ScriptableItem, int> kvp
            in items)
        {
            if (i >= inventorySlots.Length)
            {
                break;
            }


            if (inventorySlots[i] == null)
            {
                i++;
                continue;
            }


            inventorySlots[i]
                .gameObject
                .SetActive(
                    true
                );


            inventorySlots[i]
                .SetItem(
                    kvp.Key,
                    kvp.Value
                );


            i++;
        }


        // Clear unused large-inventory slots.
        for (
            int j = i;
            j < inventorySlots.Length;
            j++)
        {
            if (inventorySlots[j] == null)
            {
                continue;
            }


            inventorySlots[j]
                .ClearSlot();


            inventorySlots[j]
                .gameObject
                .SetActive(
                    false
                );
        }
    }
}