using UnityEngine;

public class WeaponUpgrades : MonoBehaviour
{
    [Header("----- Increase Ammounts -----")]
    [SerializeField][Min(0f)] float DamageIncrease = 10f;
    [SerializeField][Min(0f)] float VelocityIncrease = 25f;
    [Tooltip("This is as a percentage 25 = 25% of firerate")]
    [SerializeField][Min(0f)] float FireRateIncrease = 2.5f;
    [SerializeField][Min(0f)] int MaxAmmoIncrease = 5;
    [SerializeField][Min(0f)] int MagSizeIncrease = 5;

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
    int magAmmoBase;

    private void Start()
    {
        gun = GetComponent<GunController>();

        damageBase = gun.GetDamage();
        velocityBase = gun.GetVelocity();
        fireRateBase = gun.GetFireRate();
        maxAmmoBase = gun.GetMaxAmmo();
        magAmmoBase = gun.GetMagSize();
    }

    public void ApplyUpgrades()
    {
        gun.SetDamage(damageUpgrades * DamageIncrease + damageBase);

        gun.SetVelocity(velocityUpgrades * VelocityIncrease + velocityBase);

        gun.SetFireRate(fireRateBase - (fireRateBase * ((fireRateUpgrades * FireRateIncrease) / 100)));

        int reserveAmmo = maxAmmoUpgrades * MaxAmmoIncrease + maxAmmoBase;
        gun.SetMaxAmmo(reserveAmmo);
        gun.SetCurrentReserveAmmo(reserveAmmo);

        gun.SetMagSize(magSizeUpgrades * MagSizeIncrease + magAmmoBase);
    }

    public int GetDamageUpgrades()
    {
        return damageUpgrades;
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

    public void ModifyVelocity(int ammount)
    {
        velocityUpgrades += ammount;

        ApplyUpgrades();
    }

    public int GetFireRateUpgrades()
    {
        return fireRateUpgrades;
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

    public void ModifyMaxAmmo(int ammount)
    {
        maxAmmoUpgrades += ammount;

        ApplyUpgrades();
    }

    public int GetMagSizeUpgrades()
    {
        return magSizeUpgrades;
    }

    public void ModifyMagSize(int ammount)
    {
        magSizeUpgrades += ammount;

        ApplyUpgrades();
    }
}
