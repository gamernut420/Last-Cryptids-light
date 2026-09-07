using UnityEngine;

public interface IPlayer
{
    bool PlayerRefillAmmo(int amount);

    void PlayerAddItem(string itemName, int amount);

    void PlayerAddItem(GameObject Item);

    bool HealPlayer(int amount, bool overHeal = false);

    ProjectileManager GetProjectileManager();
}
