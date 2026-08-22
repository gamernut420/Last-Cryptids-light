using UnityEngine;

public interface IPlayer
{
    bool PlayerRefillAmmo(int amount);

    void PlayerAddItem(string itemName, int amount);

    void PlayerAddWeapon(GameObject Weapon);
}
