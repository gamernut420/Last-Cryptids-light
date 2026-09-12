using UnityEngine;

public interface IPlayer
{
    bool PlayerRefillAmmo(int amount);

    void PlayerAddItem(ScriptableItem itemName, int amount);

    void PlayerAddItem(GameObject Item);

    bool HealPlayer(int amount, bool overHeal = false);

    int GetPlayerFunds();

    void ModifyPlayerFunds(int ammount);

    GameObject[] GetPlayerHotbar();

    ProjectileManager GetProjectileManager();
}
