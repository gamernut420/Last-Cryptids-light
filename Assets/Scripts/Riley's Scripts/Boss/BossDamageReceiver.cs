using UnityEngine;

public class BossDamageReceiver : MonoBehaviour, IDamage
{
    private FinalBoss boss;

    private void Awake()
    {
        boss = GetComponentInParent<FinalBoss>();
    }

    public void takeDamage(int amount)
    {
        if (boss != null)
        {
            boss.takeDamage(amount);
        }
    }
}
