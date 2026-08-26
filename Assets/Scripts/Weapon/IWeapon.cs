using UnityEngine;

public interface IWeapon
{
    bool WeaponRefillAmmo(int amount);

    void SetPlayerVariables(IPlayer player = null, ICamera camera = null, Vector3 gripLocation = default);

    void SetWeaponUse(bool inUse);

    string GetWeaponName();
}
