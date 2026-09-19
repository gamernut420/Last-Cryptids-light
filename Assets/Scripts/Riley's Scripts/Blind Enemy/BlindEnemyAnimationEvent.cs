using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAI_HearOnly enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyAI_HearOnly>();
    }

    public void ReleaseProjectile()
    {
        if (enemy != null)
        {
            enemy.ReleaseProjectile();
        }
    }
}

