using UnityEngine;

public class StalkDamageReciever : MonoBehaviour, IDamage
{
    private EnemyStalk enemy;
    [SerializeField] private bool isHead;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyStalk>();
    }

    public void takeDamage(int amount)
    {
        if (enemy != null)
        {
            int finalDamage = amount;

            if (isHead)
            {
                finalDamage = Mathf.RoundToInt(amount * 1.5f);
                enemy.SetNextDamageCritical(true);
            }
            else
            {
                enemy.SetNextDamageCritical(false);
            }

            enemy.takeDamage(finalDamage);
        }
    }
}
