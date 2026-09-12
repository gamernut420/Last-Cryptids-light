using System.Collections.Generic;
using UnityEngine;
using static UpgradeOption;

public class PlayerUpgrades : MonoBehaviour
{
    [Header("----- Increase Ammounts -----")]
    [SerializeField][Min(0f)] float HPIncrease = 5f;
    [SerializeField][Min(0)] int HPPrice = 1;
    [SerializeField][Min(0f)] float StaminaIncrease = 5f;
    [SerializeField][Min(0)] int StaminaPrice = 1;
    [SerializeField][Min(0f)] float SpeedIncrease = 5f;
    [SerializeField][Min(0)] int SpeedPrice = 1;

    playerController player;

    int hpUpgrades = 0;
    int staminaUpgrades = 0;
    int speedUpgrades = 0;

    float baseHealth;
    float baseSpeed;
    float baseStamina;

    Dictionary<string, UpgradeFuncs> StatMap;

    private void Awake()
    {
        StatMap = new Dictionary<string, UpgradeFuncs>()
        {
            {"Health", new UpgradeFuncs()
            { 
                Modifier = ModifyHPUpgrades,
                CountGetter = GetHPUpgrades,
                PriceGetter = GetHPPrice
            } 
            },
            {"Speed", new UpgradeFuncs()
            {
                Modifier = ModifySpeedUpgrades,
                CountGetter = GetSpeedUpgrades,
                PriceGetter = GetSpeedPrice
            }
            },
            {"Stamina", new UpgradeFuncs()
            {
                Modifier = ModifyStaminaUpgrades,
                CountGetter = GetStaminaUpgrades,
                PriceGetter = GetStaminaPrice
            }
            }
        };
    }

    private void Start()
    {
        player = GetComponent<playerController>();

        baseHealth = player.GetMaxHP();
        baseSpeed = player.GetMaxSpeed();
        baseStamina = player.GetMaxStamina();
    }

    public void ApplyUpgrades()
    {
        float healthAmmount = HPIncrease * hpUpgrades + baseHealth;

        player.SetMaxHP(healthAmmount);

        player.SetCurrentHP(healthAmmount);

        player.SetMaxStamina(StaminaIncrease * staminaUpgrades + baseStamina);

        player.SetMaxSpeed(SpeedIncrease * speedUpgrades + baseSpeed);
    }

    public Dictionary<string, UpgradeFuncs> GetStatMap()
    {
        return StatMap;
    }

    public int GetHPUpgrades()
    {
        return hpUpgrades;
    }

    public int GetHPPrice()
    {
        return HPPrice;
    }

    public void ModifyHPUpgrades(int ammount)
    {
        hpUpgrades += ammount;

        ApplyUpgrades();
    }

    public int GetStaminaUpgrades()
    {
        return staminaUpgrades;
    }

    public int GetStaminaPrice()
    {
        return StaminaPrice;
    }

    public void ModifyStaminaUpgrades(int ammount)
    {
        staminaUpgrades += ammount;

        ApplyUpgrades();
    }

    public int GetSpeedUpgrades()
    {
        return speedUpgrades;
    }

    public int GetSpeedPrice()
    {
        return SpeedPrice;
    }

    public void ModifySpeedUpgrades(int ammount)
    {
        speedUpgrades += ammount;

        ApplyUpgrades();
    }
}
