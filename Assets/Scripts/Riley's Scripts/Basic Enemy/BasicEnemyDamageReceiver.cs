using UnityEngine;

public class BasicEnemyDamageReceiver : MonoBehaviour, IDamage
{
    private BasicEnemy enemy;
    [SerializeField] private bool isHead;

    private void Awake()
    {
        enemy = GetComponentInParent<BasicEnemy>();
    }

    public void takeDamage(int amount)
    {
        if (enemy != null)
        {
            if (isHead)
                amount += 5;
            enemy.takeDamage(amount);
        }
    }
}
