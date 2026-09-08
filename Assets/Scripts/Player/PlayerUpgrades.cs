using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    [Header("----- Increase Ammounts -----")]
    [SerializeField][Min(0f)] float HPIncrease = 5f;
    [SerializeField][Min(0f)] float StaminaIncrease = 5f;
    [SerializeField][Min(0f)] float SpeedIncrease = 5f;

    playerController player;

    int hpUpgrades = 0;
    int staminaUpgrades = 0;
    int speedUpgrades = 0;

    float baseHealth;
    float baseSpeed;
    float baseStamina;

    private void Start()
    {
        player = GetComponent<playerController>();

        baseHealth = player.GetMaxHP();
        baseSpeed = player.GetMaxSpeed();
        baseStamina = player.GetMaxStamina();
    }

    public void ModifyHPUpgrades(int ammount)
    {
        hpUpgrades += ammount;

        ApplyUpgrades();
    }

    public void ModifyStaminaUpgrades(int ammount)
    {
        staminaUpgrades += ammount;

        ApplyUpgrades();
    }

    public void ModifySpeedUpgrades(int ammount)
    {
        speedUpgrades += ammount;

        ApplyUpgrades();
    }

    public void ApplyUpgrades()
    {
        float healthAmmount = HPIncrease * hpUpgrades + baseHealth;

        player.SetMaxHP(healthAmmount);

        player.SetCurrentHP(healthAmmount);

        float staminaAmmount = StaminaIncrease * staminaUpgrades + baseStamina;

        player.SetMaxStamina(staminaAmmount);

        float speedAmmount = SpeedIncrease * speedUpgrades + baseSpeed;

        player.SetMaxSpeed(speedAmmount);
    }
}
