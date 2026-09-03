using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemText;
    [SerializeField] TextMeshProUGUI itemCount;
    [SerializeField] TextMeshProUGUI slotNumber;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemImage.enabled = false;
        itemText.enabled = false;
        itemCount.enabled = false;
        slotNumber.enabled = false;
    }

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
}
