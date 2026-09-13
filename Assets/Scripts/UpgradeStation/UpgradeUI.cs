using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Funds;
    [SerializeField] Transform CategoryUIRoot;
    [SerializeField] Transform UpgradeUIRoot;

    [SerializeField] GameObject CategoryUIPrefab;
    [SerializeField] GameObject UpgradeUIPrefab;

    PlayerUpgrades PlayerUpgrader;
    WeaponUpgrades WeaponUpgrader;

    List<UpgradeOption> Upgrades = new List<UpgradeOption>();

    GameObject Player;

    private void OnEnable()
    {
        Player = gameManager.instance.player;

        ClearUI_Upgrades();

        RefreshUI_Categories();
    }

    public void RefreshUI_Categories()
    {
        for(int childIndex = CategoryUIRoot.childCount - 1; childIndex >= 0; childIndex--)
        {
            Destroy(CategoryUIRoot.GetChild(childIndex).gameObject);
        }

        UpgradeCategory categoryUI = Instantiate(CategoryUIPrefab, CategoryUIRoot).GetComponent<UpgradeCategory>();

        categoryUI.Bind(Player, OnCatergorySelected);

        IPlayer playerInterface;

        if(Player.TryGetComponent<IPlayer>(out playerInterface))
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
                Debug.Log(stat);

                UpgradeOption option = Instantiate(UpgradeUIPrefab, UpgradeUIRoot).GetComponent<UpgradeOption>();

                option.Bind(stat.Key, stat.Value.Modifier, stat.Value.CountGetter, stat.Value.PriceGetter);

                Upgrades.Add(option);
            }
        }
        else if(upgradeObject.TryGetComponent<WeaponUpgrades>(out WeaponUpgrader))
        {
            foreach (var stat in WeaponUpgrader.GetStatMap())
            {
                UpgradeOption option = Instantiate(UpgradeUIPrefab, UpgradeUIRoot).GetComponent<UpgradeOption>();

                option.Bind(stat.Key, stat.Value.Modifier, stat.Value.CountGetter, stat.Value.PriceGetter);

                Upgrades.Add(option);
            }
        }
    }

    public void OnExit()
    {
        gameManager.instance.ShowUpgradeUI(false);
    }
}
