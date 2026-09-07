using UnityEngine;

public interface IPlayer
{
    bool PlayerRefillAmmo(int amount);

    void PlayerAddItem(string itemName, int amount);

    void PlayerAddItem(GameObject Item);

    ProjectileManager GetProjectileManager();
}
