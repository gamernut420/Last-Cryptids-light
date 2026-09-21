using UnityEngine;

public class BasiceEnemyAnimationEven : MonoBehaviour
{
    private BasicEnemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<BasicEnemy>();
    }

    public void EnableHitbox()
    {
        if (enemy != null)
            enemy.EnableAttackHitbox();
    }

    public void DisableHitbox()
    {
        if (enemy != null)
            enemy.DisableAttackHitbox();
    }

    public void Shoot()
    {
        if (enemy != null)
            enemy.FireProjectile();
    }

    public void EndAnimation()
    {
        if (enemy != null)
            enemy.EndAttack();
    }
}
