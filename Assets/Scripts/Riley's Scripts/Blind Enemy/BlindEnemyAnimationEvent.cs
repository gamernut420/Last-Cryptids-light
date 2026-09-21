using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAI_HearOnly enemy;

    [SerializeField] private GameObject punch1Hitbox;
    [SerializeField] private GameObject punch2Hitbox;
    [SerializeField] private GameObject punch3Hitbox;

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

    public void EnablePunch1Hitbox()
    {
        punch1Hitbox.SetActive(true);
    }

    public void EnablePunch2Hitbox()
    {
        punch2Hitbox.SetActive(true);
    }

    public void EnablePunch3Hitbox()
    {
        punch3Hitbox.SetActive(true);
    }

    public void DisablePunch1Hitbox()
    {
        punch1Hitbox.SetActive(false);
    }

    public void DisablePunch2Hitbox()
    {
        punch2Hitbox.SetActive(false);
    }

    public void DisablePunch3Hitbox()
    {
        punch3Hitbox.SetActive(false);
    }

    public void EndAttack()
    {
        if (enemy != null)
        {
            enemy.EndAttack();
        }
    }
}

