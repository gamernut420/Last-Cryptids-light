using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance
    {
        get;
        private set;
    }


    // Stores ALL actual inventory quantities.
    [System.NonSerialized]
    public Dictionary<ScriptableItem, int> items =
        new Dictionary<ScriptableItem, int>();


    [Header("Overall Inventory UI")]

    // Existing slots inside the large inventory screen.
    public InventorySlotUI[] inventorySlots;


    // ADDED FOR CRAFTING QUICK SLOTS:
    // These are specifically:
    // InvSlot1, InvSlot2, InvSlot3, InvSlot4.
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
        // ADDED FOR CRAFTING QUICK SLOTS:
        // Keep the four HUD slots visible but empty.
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
                slot.gameObject.SetActive(true);
            }
        }


        // Refresh normal inventory.
        UpdateUI();
    }


    // Adds a normal inventory item.
    //
    // World materials use this method.
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


        UpdateUI();
    }


    // ADDED FOR CRAFTING:
    // Adds a finished craft to normal inventory
    // AND assigns it to the first available quick slot.
    public void AddCraftedItem(
        ScriptableItem itemData,
        int amount)
    {
        if (itemData == null ||
            amount <= 0)
        {
            return;
        }


        // Add to real inventory first.
        AddItem(
            itemData,
            amount
        );


        // Then place/update it in slots 1-4.
        AddOrUpdateQuickSlot(
            itemData
        );
    }


    // Checks whether an item exists at all.
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


        return GetAmount(itemData)
               >= requiredAmount;
    }


    // Gets owned quantity.
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


    // Removes inventory quantity.
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


        if (items[itemData] <= 0)
        {
            items.Remove(
                itemData
            );
        }


        // Update main inventory.
        UpdateUI();


        // ADDED FOR CRAFTING QUICK SLOTS:
        // If this item is also currently on the hotbar,
        // update or clear that slot.
        RefreshQuickSlot(
            itemData
        );


        return true;
    }


    // ADDED FOR CRAFTING QUICK SLOTS:
    // Updates an existing slot for the same item,
    // otherwise finds the first empty slot:
    //
    // 1 -> 2 -> 3 -> 4
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


        int amount =
            GetAmount(
                itemData
            );


        // First see whether this item is
        // already assigned to a quick slot.
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
                    amount
                );

                return;
            }
        }


        // Otherwise find the first empty slot.
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
                    amount
                );

                return;
            }
        }


        // The item is still safely stored in the main
        // inventory even if all quick slots are full.
        Debug.Log(
            "All quick slots are full. " +
            itemData.itemName +
            " remains in the normal inventory."
        );
    }


    // ADDED FOR CRAFTING QUICK SLOTS:
    // Refresh or clear a quick slot if the item's
    // inventory quantity changes.
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


            int amount =
                GetAmount(
                    itemData
                );


            if (amount <= 0)
            {
                slot.ClearSlot();

                // Keep numbered HUD box visible.
                slot.gameObject.SetActive(true);
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


    // Updates the LARGE inventory grid only.
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
                .SetActive(true);


            inventorySlots[i]
                .SetItem(
                    kvp.Key,
                    kvp.Value
                );


            i++;
        }


        // Clear unused main-inventory slots.
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
                .SetActive(false);
        }
    }
}