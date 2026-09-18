using System;
using System.Runtime.CompilerServices;
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

    bool isMaxed = false;

    UpgradeFuncs FunctionCalls;

    public struct UpgradeFuncs
    {
        public UnityAction<int> PurchaseFNC;

        public UnityAction<int> Modifier;

        public Func<int> CountGetter;

        public int Price;

        public int MaxCount;
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
            if(FunctionCalls.MaxCount > FunctionCalls.CountGetter.Invoke())
            {
                UpgradeCounter.text = FunctionCalls.CountGetter.Invoke().ToString();

                Price.text = $"Price: {GetPrice()}";
            }
            else
            {
                UpgradeCounter.text = "Max";

                Price.text = String.Empty;

                isMaxed = true;

                SetBuyable(false);
            }
        }
    }

    public int GetPrice()
    {
        int cost = 1;

        if (FunctionCalls.CountGetter != null)
        {
            cost = (FunctionCalls.Price + (FunctionCalls.CountGetter.Invoke() * FunctionCalls.Price));
        }

        return cost;
    }

    public void SetBuyable(bool buyable)
    {
        if (isMaxed == true)
        {
            buyable = false;
        }

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
