using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    private FinalBoss boss;


    private void Awake()
    {
        boss =
            GetComponentInParent<FinalBoss>();
    }


    public void MeleeHit()
    {
        if (boss != null)
        {
            boss.MeleeHit();
        }
    }


    public void EndMeleeHit()
    {
        if (boss != null)
        {
            boss.EndMeleeHit();
        }
    }


    public void FireBeam()
    {
        if (boss != null)
        {
            boss.FireBeam();
        }
    }
}