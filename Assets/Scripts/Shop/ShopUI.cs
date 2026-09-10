using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Funds;
    [SerializeField] Transform CategoryUIRoot;
    [SerializeField] Transform ItemUIRoot;
    [SerializeField] Button PurchaseButton;

    [SerializeField] GameObject CategoryUIPrefab;
    [SerializeField] GameObject ItemUIPrefab;

    [SerializeField] List<ShopItem> AvailableItems;

    ItemCatagory SelectedCategory;
    ShopItem SelectedItem;

    List<ItemCatagory> Categories;
    Dictionary<ItemCatagory, ShopUI_Category> CategoryToUIMap;
    Dictionary<ShopItem, ShopUI_Item> ItemToUIMap;

    IPlayer player;

    private void Start()
    {
        player = gameManager.instance.player.GetComponent<IPlayer>();

        RefreshUI_Common();

        RefreshUI_Categories();
    }

    void RefreshUI_Common()
    {
        if(player != null)
        {
            Funds.text = $"Points: {player.GetPlayerFunds().ToString()}";
        }

        PurchaseButton.interactable = (
            player != null &&
            SelectedItem != null &&
            CanPurchase()
            );

        if(ItemToUIMap != null)
        {
            foreach (var itemMap in ItemToUIMap)
            {
                var item = itemMap.Key;
                var itemUI = itemMap.Value;

                itemUI.SetCanAfford(player.GetPlayerFunds() >= item.Price);
            }
        }
    }

    void RefreshUI_Categories()
    {
        for (int childIndex = CategoryUIRoot.childCount - 1; childIndex >= 0; childIndex--)
        {
            var childGameObject = CategoryUIRoot.GetChild(childIndex);

            Destroy(childGameObject.gameObject);
        }

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

        if (!Categories.Contains(SelectedCategory))
        {
            SelectedCategory = null;
            
        }

        OnCatergorySelected(SelectedCategory);
    }

    void RefreshShopUI_Items()
    {
        for(int childIndex = ItemUIRoot.childCount - 1; childIndex >= 0; childIndex--)
        {
            var childGameObject = ItemUIRoot.GetChild(childIndex);

            Destroy(childGameObject.gameObject);
        }

        ItemToUIMap = new Dictionary<ShopItem, ShopUI_Item>();

        foreach (var item in AvailableItems)
        {
            if(item.Catagory != SelectedCategory)
            {
                continue;
            }

            var itemGO = Instantiate(ItemUIPrefab, ItemUIRoot);
            var itemUI = itemGO.GetComponent<ShopUI_Item>();

            itemUI.Bind(item, OnItemSelected);
            ItemToUIMap[item] = itemUI;
        }

        RefreshUI_Common();
    }

    void OnCatergorySelected(ItemCatagory newCategory)
    {
        if(SelectedCategory != null && newCategory != null && newCategory != SelectedCategory)
        {
            SelectedItem = null;
        }

        SelectedCategory = newCategory;

        foreach (var category in Categories)
        {
            CategoryToUIMap[category].SetIsSelected(category == SelectedCategory);
        }

        RefreshShopUI_Items();
        RefreshUI_Common();
    }

    void OnItemSelected(ShopItem newItem)
    {
        Debug.Log($"Selected {newItem}");

        SelectedItem = newItem;

        foreach (var itemMap in ItemToUIMap)
        {
            var item = itemMap.Key;
            var itemUI = itemMap.Value;

            itemUI.SetIsSelected(item == SelectedItem);
        }

        RefreshUI_Common();
    }

    bool CanPurchase()
    {
        if(player != null && SelectedItem != null)
        {
            return player.GetPlayerFunds() >= SelectedItem.Price;
        }
        
        return false;
    }

    void PurchaseItem()
    {
        player.ModifyPlayerFunds(-SelectedItem.Price);

        //Add spawning of items here
    }

    public void OnClickedPurchase()
    {
        if (CanPurchase())
        {
            PurchaseItem();
        }

        RefreshUI_Common();
    }

    public void OnClickedExit()
    {
        gameManager.instance.ShowShopUI(false);
    }
}
