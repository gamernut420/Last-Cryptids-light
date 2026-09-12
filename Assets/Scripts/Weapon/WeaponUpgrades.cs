using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UpgradeOption;

public class WeaponUpgrades : MonoBehaviour
{
    [Header("----- Increase Ammounts -----")]
    [SerializeField][Min(0f)] float DamageIncrease = 10f;
    [SerializeField][Min(0)] int DamagePrice = 1;
    [SerializeField][Min(0f)] float VelocityIncrease = 25f;
    [SerializeField][Min(0)] int VelocityPrice = 1;
    [Tooltip("This is as a percentage 25 = 25% of firerate")]
    [SerializeField][Min(0f)] float FireRateIncrease = 2.5f;
    [SerializeField][Min(0)] int FireRatePrice = 1;
    [SerializeField][Min(0f)] int MaxAmmoIncrease = 5;
    [SerializeField][Min(0)] int MaxAmmoPrice = 1;
    [SerializeField][Min(0f)] int MagSizeIncrease = 5;
    [SerializeField][Min(0)] int MagSizePrice = 1;

    GunController gun;

    int damageUpgrades = 0;
    int velocityUpgrades = 0;
    int fireRateUpgrades = 0;
    int maxAmmoUpgrades = 0;
    int magSizeUpgrades = 0;

    float damageBase;
    float velocityBase;
    float fireRateBase;
    int maxAmmoBase;
    int magSizeBase;

    Dictionary<string, UpgradeFuncs> StatMap;

    private void Awake()
    {
        StatMap = new Dictionary<string, UpgradeFuncs>()
        {
            {"Damage", new UpgradeFuncs()
            {
                Modifier = ModifyDamage,
                CountGetter = GetDamageUpgrades,
                PriceGetter = GetDamagePrice,
            }
            },
            {"Velocity", new UpgradeFuncs()
            {
                Modifier = ModifyVelocity,
                CountGetter = GetVelocityUpgrades,
                PriceGetter = GetVelocityPrice,
            }
            },
            {"Fire-Rate", new UpgradeFuncs()
            {
                Modifier = ModifyFireRate,
                CountGetter = GetFireRateUpgrades,
                PriceGetter = GetFireRatePrice,
            }
            },
            {"Max Ammo", new UpgradeFuncs()
            {
                Modifier = ModifyMaxAmmo,
                CountGetter = GetMaxAmmoUpgrades,
                PriceGetter = GetMaxAmmoPrice,
            }
            },
            {"Mag Size", new UpgradeFuncs()
            {
                Modifier = ModifyMagSize,
                CountGetter = GetMagSizeUpgrades,
                PriceGetter = GetMagSizePrice,
            }
            }
        };
    }

    private void Start()
    {
        gun = GetComponent<GunController>();

        damageBase = gun.GetDamage();
        velocityBase = gun.GetVelocity();
        fireRateBase = gun.GetFireRate();
        maxAmmoBase = gun.GetMaxAmmo();
        magSizeBase = gun.GetMagSize();
    }

    public void ApplyUpgrades()
    {
        gun.SetDamage(damageUpgrades * DamageIncrease + damageBase);

        gun.SetVelocity(velocityUpgrades * VelocityIncrease + velocityBase);

        gun.SetFireRate(fireRateBase - (fireRateBase * ((fireRateUpgrades * FireRateIncrease) / 100)));

        int reserveAmmo = maxAmmoUpgrades * MaxAmmoIncrease + maxAmmoBase;
        gun.SetMaxAmmo(reserveAmmo);
        gun.SetCurrentReserveAmmo(reserveAmmo);

        gun.SetMagSize(magSizeUpgrades * MagSizeIncrease + magSizeBase);
    }

    public Dictionary<string, UpgradeFuncs> GetStatMap()
    {
        return StatMap;
    }

    public int GetDamageUpgrades()
    {
        return damageUpgrades;
    }

    public int GetDamagePrice()
    {
        return DamagePrice;
    }

    public void ModifyDamage(int ammount)
    {
        damageUpgrades += ammount;

        ApplyUpgrades();
    }

    public int GetVelocityUpgrades()
    {
        return velocityUpgrades;
    }

    public int GetVelocityPrice()
    {
        return VelocityPrice;
    }

    public void ModifyVelocity(int ammount)
    {
        velocityUpgrades += ammount;

        ApplyUpgrades();
    }

    public int GetFireRateUpgrades()
    {
        return fireRateUpgrades;
    }

    public int GetFireRatePrice()
    {
        return FireRatePrice;
    }

    public void ModifyFireRate(int ammount)
    {
        fireRateUpgrades += ammount;

        ApplyUpgrades();
    }

    public int GetMaxAmmoUpgrades()
    {
        return maxAmmoUpgrades;
    }

    public int GetMaxAmmoPrice()
    {
        return MaxAmmoPrice;
    }

    public void ModifyMaxAmmo(int ammount)
    {
        maxAmmoUpgrades += ammount;

        ApplyUpgrades();
    }

    public int GetMagSizeUpgrades()
    {
        return magSizeUpgrades;
    }

    public int GetMagSizePrice()
    {
        return MagSizePrice;
    }

    public void ModifyMagSize(int ammount)
    {
        magSizeUpgrades += ammount;

        ApplyUpgrades();
    }
}
