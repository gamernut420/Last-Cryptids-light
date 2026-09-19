
using UnityEngine;

public class HearOnlyProjectile : MonoBehaviour
{
    private EnemyAI_HearOnly owner;
    private bool hasHit;
    [SerializeField] private LayerMask returnLayers;

    public void Initialize(EnemyAI_HearOnly enemy)
    {
        owner = enemy;
        Debug.Log("HearOnlyProjectile initialized");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            "PROJECTILE TRIGGER ENTER: " +
            other.gameObject.name
        );

        if (hasHit)
            return;

        if ((returnLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            hasHit = true;

            Debug.Log("Projectile hit: " + other.gameObject.name);

            if (owner != null)
            {
                Debug.Log("Calling ProjectileHit()");
                owner.ProjectileHit();
            }
            else
            {
                Debug.LogError("HearOnlyProjectile owner is NULL!");
            }
        }
    }
}
