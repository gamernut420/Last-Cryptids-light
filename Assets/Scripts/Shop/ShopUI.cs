using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Funds;
    [SerializeField] Transform CategoryUIRoot;
    [SerializeField] Transform ItemUIRoot;

    [SerializeField] GameObject CategoryUIPrefab;
    [SerializeField] GameObject ItemUIPrefab;

    [SerializeField] List<ShopItem> AvailableItems;

    ItemCatagory SelectedCategory;

    List<ItemCatagory> Categories;
    Dictionary<ItemCatagory, ShopUI_Category> CategoryToUIMap;

    private void Start()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        Categories = new List<ItemCatagory>();
        CategoryToUIMap = new Dictionary<ItemCatagory, ShopUI_Category>();

        //Determain Category List
        foreach (var item in AvailableItems)
        {
            if (!Categories.Contains(item.Catagory))
            {
                Categories.Add(item.Catagory);
            }
        }

        Categories.Sort((lhs, rhs) => lhs.name.CompareTo(rhs.name));

        foreach(var category in Categories)
        {
            var categoryGO = Instantiate(CategoryUIPrefab, CategoryUIRoot);
            var categoryUI = categoryGO.GetComponent<ShopUI_Category>();

            categoryUI.Bind(category, OnCatergorySelected);
            CategoryToUIMap[category] = categoryUI;
        }

        if (Categories.Contains(SelectedCategory))
        {
            OnCatergorySelected(SelectedCategory);
        }
        else
        {
            SelectedCategory = null;
        }
    }

    void OnCatergorySelected(ItemCatagory newCategory)
    {
        SelectedCategory = newCategory;

        foreach (var category in Categories)
        {
            CategoryToUIMap[category].SetIsSelected(category == SelectedCategory);
        }
    }

    public void OnClickedPurchase()
    {

    }

    public void OnClickedExit()
    {

    }
}
