using UnityEngine;

public class BlindDamageReciever : MonoBehaviour, IDamage
{
    private EnemyAI_HearOnly enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyAI_HearOnly>();
    }

    public void takeDamage(int amount)
    {
        if (enemy != null)
        {
            enemy.takeDamage(amount);
        }
    }
}
