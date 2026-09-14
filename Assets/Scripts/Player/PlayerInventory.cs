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
    public InventorySlotUI[] inventorySlots;


    // ADDED FOR CRAFTING QUICK SLOTS:
    // Expected order:
    // Element 0 = Slot 1
    // Element 1 = Slot 2
    // Element 2 = Slot 3
    // Element 3 = Slot 4
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
        // CHANGED FOR CRAFTED QUICK SLOTS:
        // Only initialize slots 3 and 4.
        // Slots 1 and 2 are left alone for other systems.
        if (quickSlots != null)
        {
            for (int i = 2; i < quickSlots.Length && i <= 3; i++)
            {
                InventorySlotUI slot =
                    quickSlots[i];


                if (slot == null)
                {
                    continue;
                }


                slot.ClearSlot();
                slot.gameObject.SetActive(true);
            }
        }


        UpdateUI();
    }


    // Normal pickups go only into the main inventory.
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
    // Finished crafted items go into normal inventory
    // and then slots 3 or 4.
    public void AddCraftedItem(
        ScriptableItem itemData,
        int amount)
    {
        if (itemData == null ||
            amount <= 0)
        {
            return;
        }


        AddItem(
            itemData,
            amount
        );


        AddOrUpdateCraftedQuickSlot(
            itemData
        );
    }


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


        UpdateUI();


        // CHANGED FOR CRAFTED QUICK SLOTS:
        // Only checks slots 3 and 4.
        RefreshCraftedQuickSlot(
            itemData
        );


        return true;
    }


    // ADDED FOR CRAFTING QUICK SLOTS:
    // Crafted items may ONLY occupy Slot 3 or Slot 4.
    private void AddOrUpdateCraftedQuickSlot(
        ScriptableItem itemData)
    {
        if (quickSlots == null ||
            quickSlots.Length < 4)
        {
            Debug.LogWarning(
                "PlayerInventory needs 4 Quick Slots assigned."
            );

            return;
        }


        int amount =
            GetAmount(
                itemData
            );


        // First check if this crafted item is already
        // assigned to Slot 3 or Slot 4.
        for (int i = 2; i <= 3; i++)
        {
            InventorySlotUI slot =
                quickSlots[i];


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


        // Then find the first empty crafted-item slot.
        // Slot 3 is preferred, then Slot 4.
        for (int i = 2; i <= 3; i++)
        {
            InventorySlotUI slot =
                quickSlots[i];


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


        // Main inventory still keeps the crafted item.
        Debug.Log(
            "Crafted quick slots 3 and 4 are full. " +
            itemData.itemName +
            " remains in the normal inventory."
        );
    }


    // ADDED FOR CRAFTING QUICK SLOTS:
    // Updates only Slot 3 or Slot 4.
    private void RefreshCraftedQuickSlot(
        ScriptableItem itemData)
    {
        if (quickSlots == null ||
            quickSlots.Length < 4)
        {
            return;
        }


        for (int i = 2; i <= 3; i++)
        {
            InventorySlotUI slot =
                quickSlots[i];


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


    // Updates only the large inventory grid.
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