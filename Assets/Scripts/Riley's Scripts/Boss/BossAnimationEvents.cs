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


    // ADDED:
    // Called by an Animation Event during
    // the boss teleport animation.
    public void Teleport()
    {
        if (boss != null)
        {
            boss.PerformTeleport();
        }
    }
}