using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Funds;
    [SerializeField] Transform CategoryUIRoot;
    [SerializeField] Transform UpgradeUIRoot;

    [SerializeField] GameObject CategoryUIPrefab;
    [SerializeField] GameObject UpgradeUIPrefab;

    PlayerUpgrades PlayerUpgrader;
    WeaponUpgrades WeaponUpgrader;

    List<UpgradeOption> Upgrades;

    GameObject Player;
    IPlayer playerInterface;

    private void OnEnable()
    {
        Player = gameManager.instance.player;

        playerInterface = Player.GetComponent<IPlayer>();

        Upgrades = new List<UpgradeOption>();

        ClearUI_Upgrades();

        RefreshUI_Categories();
    }

    void RefreshUI_General()
    {
        if(playerInterface != null)
        {
            Funds.text = $"Funds: {playerInterface.GetPlayerFunds()}";

            if (Upgrades.Count > 0)
            {
                foreach (UpgradeOption upgrade in Upgrades)
                {
                    upgrade.SetBuyable(playerInterface.GetPlayerFunds() >= upgrade.GetPrice());
                }
            }
        }
        
    }

    public void RefreshUI_Categories()
    {
        for(int childIndex = CategoryUIRoot.childCount - 1; childIndex >= 0; childIndex--)
        {
            Destroy(CategoryUIRoot.GetChild(childIndex).gameObject);
        }

        UpgradeCategory categoryUI = Instantiate(CategoryUIPrefab, CategoryUIRoot).GetComponent<UpgradeCategory>();

        categoryUI.Bind(Player, OnCatergorySelected);

        if(playerInterface != null)
        {
            GameObject[] weapons = playerInterface.GetPlayerHotbar();

            for(int i = 0; i < 2; i++)
            {
                if(weapons[i] != null)
                {
                    categoryUI = Instantiate(CategoryUIPrefab, CategoryUIRoot).GetComponent<UpgradeCategory>();

                    categoryUI.Bind(weapons[i], OnCatergorySelected);
                }
            }
        }
    }

    void ClearUI_Upgrades()
    {
        for (int childIndex = UpgradeUIRoot.childCount - 1; childIndex >= 0; childIndex--)
        {
            Destroy(UpgradeUIRoot.GetChild(childIndex).gameObject);
        }

        PlayerUpgrader = null;
        WeaponUpgrader = null;
        Upgrades.Clear();
    }

    void OnCatergorySelected(GameObject upgradeObject)
    {
        ClearUI_Upgrades();

        if(upgradeObject.TryGetComponent<PlayerUpgrades>(out PlayerUpgrader))
        {
            foreach(var stat in PlayerUpgrader.GetStatMap())
            {
                UpgradeOption option = Instantiate(UpgradeUIPrefab, UpgradeUIRoot).GetComponent<UpgradeOption>();

                option.Bind(stat.Key, OnPurchase, stat.Value);

                Upgrades.Add(option);
            }
        }
        else if(upgradeObject.TryGetComponent<WeaponUpgrades>(out WeaponUpgrader))
        {
            foreach (var stat in WeaponUpgrader.GetStatMap())
            {
                UpgradeOption option = Instantiate(UpgradeUIPrefab, UpgradeUIRoot).GetComponent<UpgradeOption>();

                option.Bind(stat.Key, OnPurchase, stat.Value);

                Upgrades.Add(option);
            }
        }

        RefreshUI_General();
    }

    public void OnPurchase(int price)
    {
        Player.GetComponent<IPlayer>().ModifyPlayerFunds(-price);

        RefreshUI_General();
    }

    public void OnExit()
    {
        gameManager.instance.ShowUpgradeUI(false);
    }
}
