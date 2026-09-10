using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopUI_Item : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ItemName;
    [SerializeField] TextMeshProUGUI Description;
    [SerializeField] TextMeshProUGUI Price;

    [SerializeField] Image BackgroundPanel;
    [SerializeField] Color DefaultColor;
    [SerializeField] Color SelectedColor;
    [SerializeField] Color InaffordableColor;

    ShopItem Item;
    UnityAction<ShopItem> SelectedFnc;

    public void Bind(ShopItem _item, UnityAction<ShopItem> onSelected)
    {
        Item = _item;

        SelectedFnc = onSelected;

        ItemName.text = Item.ItemName;
        Description.text = Item.Description;
        Price.text = Item.Price.ToString();
    }

    public void SetIsSelected(bool selected)
    {
        if (selected)
        {
            Debug.Log($"Selected Item {Item}");
        }
        
        BackgroundPanel.color = selected ? SelectedColor : DefaultColor;
    }

    public void SetCanAfford(bool canAfford)
    {
        Price.fontStyle = canAfford ? FontStyles.Normal : FontStyles.Strikethrough;

        BackgroundPanel.color = canAfford ? DefaultColor : InaffordableColor;
    }

    public void OnClicked()
    {
        if (Item != null && SelectedFnc != null)
        {
            SelectedFnc.Invoke(Item);
        }
    }
}
