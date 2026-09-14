using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeOption : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI UpgradeName;
    [SerializeField] TextMeshProUGUI Price;
    [SerializeField] TextMeshProUGUI UpgradeCounter;

    [SerializeField] Image BackgroundPanel;
    [SerializeField] Color DefaultColor;
    [SerializeField] Color InaffordableColor;

    [SerializeField] Button UpgradeButton;

    UpgradeFuncs FunctionCalls;

    public struct UpgradeFuncs
    {
        public UnityAction<int> PurchaseFNC;

        public UnityAction<int> Modifier;

        public Func<int> CountGetter;

        public Func<int> PriceGetter;
    }

    public void Bind(string name, UnityAction<int> purchaseFunc, UpgradeFuncs functions)
    {
        UpgradeName.text = name;

        FunctionCalls = functions;

        FunctionCalls.PurchaseFNC = purchaseFunc;

        Refresh();
    }

    void Refresh()
    {
        if(FunctionCalls.CountGetter != null)
        {
            UpgradeCounter.text = FunctionCalls.CountGetter.Invoke().ToString();

            if (FunctionCalls.PriceGetter != null)
            {
                
                Price.text = $"Price: {GetPrice()}";
            }
        }
    }

    public int GetPrice()
    {
        int cost = 1;

        if (FunctionCalls.CountGetter != null && FunctionCalls.PriceGetter != null)
        {
            cost = (FunctionCalls.PriceGetter() + (FunctionCalls.CountGetter.Invoke() * FunctionCalls.PriceGetter()));
        }

        return cost;
    }

    public void SetBuyable(bool buyable)
    {
        BackgroundPanel.color = buyable ? DefaultColor : InaffordableColor;

        UpgradeButton.interactable = buyable;
    }

    public void OnClicked()
    {
        if(FunctionCalls.Modifier != null && FunctionCalls.PurchaseFNC != null)
        {
            FunctionCalls.PurchaseFNC(GetPrice());

            FunctionCalls.Modifier.Invoke(1);
        }

        Refresh();
    }
}
