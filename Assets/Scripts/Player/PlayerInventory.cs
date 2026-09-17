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


    // CHANGED FOR 5-SLOT HUD:
    // Element 0 = Slot 1 - Weapon
    // Element 1 = Slot 2 - Weapon
    // Element 2 = Slot 3 - Crafted
    // Element 3 = Slot 4 - Crafted
    // Element 4 = Slot 5 - Crafted
    [Header("HUD Quick Slots 1-5")]
    public InventorySlotUI[] quickSlots;


    // ADDED FOR HUD:
    private const int CraftedSlotStart = 2;
    private const int CraftedSlotEnd = 4;


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
        ConfigureQuickSlots();

        UpdateUI();
    }


    // ADDED FOR 5-SLOT HUD:
    private void ConfigureQuickSlots()
    {
        if (quickSlots == null ||
            quickSlots.Length < 5)
        {
            Debug.LogWarning(
                "PlayerInventory: Assign all 5 HUD Quick Slots."
            );

            return;
        }


        // Slots 1 and 2 are weapons.
        if (quickSlots[0] != null)
        {
            quickSlots[0].ConfigureSlot(
                1,
                InventorySlotUI.SlotType.Weapon
            );
        }


        if (quickSlots[1] != null)
        {
            quickSlots[1].ConfigureSlot(
                2,
                InventorySlotUI.SlotType.Weapon
            );
        }


        // Slots 3-5 are crafted items.
        for (
            int i = CraftedSlotStart;
            i <= CraftedSlotEnd;
            i++)
        {
            InventorySlotUI slot =
                quickSlots[i];


            if (slot == null)
            {
                continue;
            }


            slot.ConfigureSlot(
                i + 1,
                InventorySlotUI.SlotType.Crafted
            );


            slot.ClearSlot();

            slot.gameObject.SetActive(
                true
            );
        }
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
            items[itemData] +=
                amount;
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


    // Finished crafted items go into the normal inventory
    // and then attempt to occupy slots 3-5.
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


        return GetAmount(
            itemData
        ) >= requiredAmount;
    }


    public int GetAmount(
        ScriptableItem itemData)
    {
        if (itemData == null)
        {
            return 0;
        }


        if (items.ContainsKey(
                itemData))
        {
            return items[
                itemData
            ];
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


        items[itemData] -=
            amount;


        if (items[itemData] <= 0)
        {
            items.Remove(
                itemData
            );
        }


        UpdateUI();


        RefreshCraftedQuickSlot(
            itemData
        );


        return true;
    }


    // Crafted items can ONLY use Slot 3, 4, or 5.
    private void AddOrUpdateCraftedQuickSlot(
        ScriptableItem itemData)
    {
        if (quickSlots == null ||
            quickSlots.Length < 5)
        {
            Debug.LogWarning(
                "PlayerInventory: 5 Quick Slots are required."
            );

            return;
        }


        int amount =
            GetAmount(
                itemData
            );


        // First see if this item is already assigned.
        for (
            int i = CraftedSlotStart;
            i <= CraftedSlotEnd;
            i++)
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


        // Otherwise use the first empty crafted slot.
        for (
            int i = CraftedSlotStart;
            i <= CraftedSlotEnd;
            i++)
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


        // Main inventory still retains the item.
        Debug.Log(
            "Crafted HUD slots 3-5 are full. " +
            itemData.itemName +
            " remains in the main inventory."
        );
    }


    // Updates crafted HUD slots after an item is used.
    private void RefreshCraftedQuickSlot(
        ScriptableItem itemData)
    {
        if (quickSlots == null ||
            quickSlots.Length < 5)
        {
            return;
        }


        for (
            int i = CraftedSlotStart;
            i <= CraftedSlotEnd;
            i++)
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

                slot.gameObject.SetActive(
                    true
                );
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


    // ADDED FOR HUD:
    // Useful later when selecting 1-5.
    public void SetQuickSlotSelected(
        int slotIndex)
    {
        if (quickSlots == null)
        {
            return;
        }


        for (
            int i = 0;
            i < quickSlots.Length;
            i++)
        {
            if (quickSlots[i] == null)
            {
                continue;
            }


            quickSlots[i].SetSelected(
                i == slotIndex
            );
        }
    }


    // Updates only the large inventory grid.
    public void UpdateUI()
    {
        if (inventorySlots == null)
        {
            return;
        }


        int i = 0;


        foreach (
            KeyValuePair<
                ScriptableItem,
                int> kvp
            in items)
        {
            if (i >=
                inventorySlots.Length)
            {
                break;
            }


            if (inventorySlots[i] ==
                null)
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
            if (inventorySlots[j] ==
                null)
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