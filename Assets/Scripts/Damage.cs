using UnityEngine;

public class Damage : MonoBehaviour
{
    enum damageType { bullet, enemyAttack };
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletDestroyTime;
    [SerializeField] ParticleSystem hitEffect;

    bool hasDamagedPlayer;
    private float damageTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (type == damageType.bullet)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
            Destroy(gameObject, bulletDestroyTime);
        }
    }

    private void Update()
    {
        if (damageTimer  > 0)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    // Created for the boss ranged beam attack, but can be used for DOT damage as well
    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg == null) return;

        if (type == damageType.enemyAttack)
        {
            if (damageTimer > 0) return;

            dmg.takeDamage(damageAmount);
            damageTimer = damageRate;
        }
    }

    private void OnEnable()
    {
        hasDamagedPlayer = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null)
        {
            if (type == damageType.enemyAttack)
            {
                if (hasDamagedPlayer)
                    return;

                hasDamagedPlayer = true;
            }
            dmg.takeDamage(damageAmount);
        }

        if (type == damageType.bullet)
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
