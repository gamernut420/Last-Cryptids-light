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

    UnityAction<int> PurchaseFNC;

    Func<int> UpgradeCountFNC;
    Func<int> PriceGetFNC;

    public struct UpgradeFuncs
    {
        public UnityAction<int> Modifier;

        public Func<int> CountGetter;

        public Func<int> PriceGetter;
    }

    public void Bind(string name, UnityAction<int> purchaseFunc, Func<int> upgraderCounter = null, Func<int> priceGetter = null)
    {
        UpgradeName.text = name;

        PurchaseFNC = purchaseFunc;

        UpgradeCountFNC = upgraderCounter;
        PriceGetFNC = priceGetter;

        Refresh();
    }

    void Refresh()
    {
        if(UpgradeCountFNC != null)
        {
            UpgradeCounter.text = UpgradeCountFNC.Invoke().ToString();

            if (PriceGetFNC != null)
            {
                Price.text = $"Price: {(PriceGetFNC() + (UpgradeCountFNC.Invoke() * PriceGetFNC()))}";
            }
        }
    }

    public void SetBuyable(bool buyable)
    {
        BackgroundPanel.color = buyable ? DefaultColor : InaffordableColor;

        UpgradeButton.interactable = buyable;
    }

    public void OnClicked()
    {
        if(PurchaseFNC != null)
        {
            PurchaseFNC.Invoke(1);
        }

        Refresh();
    }
}
