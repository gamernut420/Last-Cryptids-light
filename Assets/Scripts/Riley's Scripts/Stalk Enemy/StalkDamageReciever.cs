using UnityEngine;

public class StalkDamageReciever : MonoBehaviour, IDamage
{
    private EnemyStalk enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyStalk>();
    }

    public void takeDamage(int amount)
    {
        if (enemy != null)
        {
            enemy.takeDamage(amount);
        }
    }
}
