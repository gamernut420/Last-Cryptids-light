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

        SelectedFnc = onSelected;

        SetIsSelected(false);
    }

    public void SetIsSelected(bool selected)
    {
        BackgroundPanel.color = selected ? SelectedColor : DefaultColor;
    }

    public void OnClicked()
    {
        if (Category != null && SelectedFnc != null)
        {
            SelectedFnc.Invoke(Category);
        }
    }
}
