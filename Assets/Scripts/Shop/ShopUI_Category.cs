using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;


public class ShopUI_Category : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI CategoryName;
    [SerializeField] Image BackgroundPanel;
    [SerializeField] Color DefaultColor;
    [SerializeField] Color SelectedColor;
    
    ItemCatagory Category;
    UnityAction<ItemCatagory> SelectedFnc;

    public void Bind(ItemCatagory _category, UnityAction<ItemCatagory> onSelected)
    {
        Category = _category;

        CategoryName.text = Category.CatagoryName;

        SetIsSelected(false);
    }

    public void SetIsSelected(bool selected)
    {
        BackgroundPanel.color = selected ? SelectedColor : DefaultColor;
    }

    public void OnClicked()
    {
        SelectedFnc.Invoke(Category);
    }
}
