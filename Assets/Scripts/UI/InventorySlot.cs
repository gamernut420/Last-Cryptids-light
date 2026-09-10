using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemText;
    [SerializeField] TextMeshProUGUI itemCount;
    [SerializeField] TextMeshProUGUI slotNumber;

    Image slotImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slotImage = GetComponent<Image>();

        itemImage.enabled = false;
        itemText.enabled = false;
        itemCount.enabled = false;
    }

    //public void UpdateSlotColor(Color color)
    //{
    //    slotImage.color = color;
    //}

    public void UpdateSlot(Sprite _itemImage, string _itemText, int _itemCount)
    {
        if(_itemImage != null)
        {
            itemImage.enabled = true;
            itemText.enabled = false;

            itemImage.sprite = _itemImage;
        }
        else
        {
            itemText.enabled = true;
            itemImage.enabled = false;

            itemText.text = _itemText;
        }

        if(_itemCount > 1)
        {
            itemCount.enabled = true;
            itemCount.text = _itemCount.ToString();
        }
        else
        {
            itemCount.enabled = false;
        }
    }

    //public void UpdateSlot(Sprite _itemImage, string _itemText, int _itemCount, Color color)
    //{
    //    UpdateSlot(_itemImage, _itemText, _itemCount);

    //    UpdateSlotColor(color);
    //}
}
