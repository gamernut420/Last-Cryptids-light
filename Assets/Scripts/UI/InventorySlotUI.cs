using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI stackSizeText;

    public void SetItem(ScriptableItem item, int quantity)
    {
        if(item != null)
        {
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = item.itemIcon;

            if (quantity > 1)
            {
                stackSizeText.gameObject.SetActive(true);
                stackSizeText.text = quantity.ToString();
            }
            else
                stackSizeText.gameObject.SetActive(false);
        }
    }

    public void ClearSlot()
    {
        iconImage.gameObject.SetActive(false);
        stackSizeText.gameObject.SetActive(false);
    }
    public void UpdateSlot(Sprite _itemImage, string _itemText, int _itemCount)
    {
        if (_itemImage != null)
        {
            iconImage.enabled = true;

            iconImage.sprite = _itemImage;
        }
        else
        {
            iconImage.enabled = false;

        }

        if (_itemCount > 1)
        {
            stackSizeText.enabled = true;
            stackSizeText.text = _itemCount.ToString();
        }
        else
        {
            stackSizeText.enabled = false;
        }
    }
}
