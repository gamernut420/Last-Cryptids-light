using UnityEngine;

public class Damage : MonoBehaviour
{
    enum damageType { bullet, enemyAttack, DOT};
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletDestroyTime;
    [SerializeField] ParticleSystem hitEffect;

    bool hasDamagedPlayer;
    private float damageTimer;
    private IDamage currentTarget;

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
        if (damageTimer  > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void OnEnable()
    {
        hasDamagedPlayer = false;
        damageTimer = 0f;
        currentTarget = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null)
        {
            if (type == damageType.DOT)
            {
                if (currentTarget != null) return;

                currentTarget = dmg;

                dmg.takeDamage(damageAmount);

                damageTimer = damageRate;

                return;
            }
        }
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

    // Created for the boss ranged beam attack, but can be used for DOT damage as well
    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) return;

        IDamage dmg = other.GetComponentInParent<IDamage>();

        if (dmg == null) return;

        if (type == damageType.DOT)
        {
            if (currentTarget != dmg) return;

            if (damageTimer > 0) return;

            dmg.takeDamage(damageAmount);
            damageTimer = damageRate;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponentInParent<IDamage>();

        if (dmg == null)
            return;

        if (type == damageType.DOT && currentTarget == dmg)
        {
            currentTarget = null;
            damageTimer = 0f;
        }
    }
}
