using TMPro;
using UnityEngine;

public class ShopUI_Item : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ItemName;
    [SerializeField] TextMeshProUGUI Description;
    [SerializeField] TextMeshProUGUI Price;

    ShopItem Item;

    public void Bind(ShopItem _item)
    {
        Item = _item;

        ItemName.text = Item.ItemName;
        Description.text = Item.Description;
        Price.text = Item.Price.ToString();
    }
}
