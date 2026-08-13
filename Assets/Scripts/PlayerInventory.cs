using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    Dictionary<string, int> items = new Dictionary<string, int>();

    [SerializeField] TextMeshProUGUI inventoryText;

    public void AddItem(string itemName, int amount)
    {
        if (items.ContainsKey(itemName))
        {
            items[itemName] += amount;
        }
        else
        {
            items.Add(itemName, amount);
        }

        Debug.Log(itemName + ": " + items[itemName]);
        UpdateUI();
    }

    public bool HasItem(string itemName)
    {
        return items.ContainsKey(itemName);
    }

    public int GetAmount(string itemName)
    {
        if (items.ContainsKey(itemName))
        {
            return items[itemName];
        }

        return 0;
    }

    void UpdateUI()
    {
        inventoryText.text = "Fuel: " + GetAmount("Fuel") + "\n";
        inventoryText.text += "Tubes: " + GetAmount("Radio Tube") + "\n";
        inventoryText.text += "Batteries: " + GetAmount("Battery");
    }
}
