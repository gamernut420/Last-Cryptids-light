using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI stackSizeText;


    // ADDED FOR INVENTORY SLOTS:
    // Tracks which ScriptableItem is actually assigned
    // to this specific inventory/hotbar slot.
    public ScriptableItem CurrentItem { get; private set; }


    // ADDED FOR INVENTORY SLOTS:
    // Tracks how many of the item are represented
    // by this slot.
    public int CurrentQuantity { get; private set; }


    // ADDED FOR INVENTORY SLOTS:
    // Lets PlayerInventory determine whether this
    // slot is available for a newly crafted item.
    public bool IsEmpty =>
        CurrentItem == null;


    public void SetItem(
        ScriptableItem item,
        int quantity)
    {
        // ADDED FOR INVENTORY SLOTS:
        CurrentItem = item;
        CurrentQuantity = quantity;


        if (item != null)
        {
            iconImage.gameObject.SetActive(true);
            iconImage.enabled = true;
            iconImage.sprite = item.itemIcon;


            if (quantity > 1)
            {
                stackSizeText.gameObject.SetActive(true);
                stackSizeText.enabled = true;
                stackSizeText.text =
                    quantity.ToString();
            }
            else
            {
                stackSizeText.gameObject.SetActive(false);
                stackSizeText.enabled = false;
            }
        }
        else
        {
            ClearSlot();
        }
    }


    public void ClearSlot()
    {
        // ADDED FOR INVENTORY SLOTS:
        CurrentItem = null;
        CurrentQuantity = 0;


        iconImage.sprite = null;
        iconImage.gameObject.SetActive(false);
        iconImage.enabled = false;


        stackSizeText.text = "";
        stackSizeText.gameObject.SetActive(false);
        stackSizeText.enabled = false;
    }


    public void UpdateSlot(
        Sprite _itemImage,
        string _itemText,
        int _itemCount)
    {
        if (_itemImage != null)
        {
            iconImage.enabled = true;
            iconImage.gameObject.SetActive(true);

            iconImage.sprite =
                _itemImage;
        }
        else
        {
            iconImage.enabled = false;
        }


        if (_itemCount > 1)
        {
            stackSizeText.enabled = true;
            stackSizeText.gameObject.SetActive(true);

            stackSizeText.text =
                _itemCount.ToString();
        }
        else
        {
            stackSizeText.enabled = false;
            stackSizeText.gameObject.SetActive(false);
        }
    }
}