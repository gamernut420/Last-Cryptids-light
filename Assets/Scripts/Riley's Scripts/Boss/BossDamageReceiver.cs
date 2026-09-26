using UnityEngine;

public class BossDamageReceiver : MonoBehaviour, IDamage
{
    [SerializeField] private bool isHeadHitbox = false;
    private FinalBoss boss;

    private void Awake()
    {
        boss = GetComponentInParent<FinalBoss>();
    }

    public void takeDamage(int amount)
    {
        if (boss != null)
        {
            int finalDamage = amount;

            if (isHeadHitbox)
            {
                finalDamage = Mathf.RoundToInt(amount * 1.5f);
                boss.SetNextDamageCritical(true);
            }
            else
            {
                boss.SetNextDamageCritical(false);
            }

            boss.takeDamage(finalDamage);
        }
    }
}
