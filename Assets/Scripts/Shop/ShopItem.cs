using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Item", fileName = "ShopItem_")]

public class ShopItem : ScriptableObject
{
    public ItemCatagory Catagory;
    public string ItemName;
    [TextArea(3,5)]public string Description;

    public int Price;
}
