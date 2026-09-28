using UnityEngine;

public class StalkAnimationEvent : MonoBehaviour                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              //Riley Bohn's code
{
    private EnemyStalk enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyStalk>();
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
}
