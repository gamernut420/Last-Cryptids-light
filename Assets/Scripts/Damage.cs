using UnityEngine;

public class Damage : MonoBehaviour
{
    enum damageType { bullet, enemyAttack, DOT};
    [SerializeField] damageType type;

    [SerializeField] private bool affectedByDifficulty;

    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletDestroyTime;
    [SerializeField] ParticleSystem hitEffect;

    bool hasDamagedPlayer;
    private float damageTimer;
    private IDamage currentTarget;
    private int baseDamageAmount;


    public void Awake()
    {
        baseDamageAmount = damageAmount;
    }
    

    void Start()
    {
        ApplyDifficultyScaling();
        if (type == damageType.bullet)
        {
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * bulletSpeed;
            }
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
        if (baseDamageAmount > 0)
        {
            ApplyDifficultyScaling();
        }
    }

    public void ApplyDifficultyScaling()
    {
        damageAmount = baseDamageAmount;

        if (!affectedByDifficulty)
        {
            return;
        }

        DifficultyManager difficultyManager = DifficultyManager.GetInstance();
        if (difficultyManager != null)
        {
            damageAmount = difficultyManager.GetScaledEnemyDamage(baseDamageAmount);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        { return; }

        IDamage damageTarget = other.GetComponent<IDamage>();

        if (damageTarget != null && type == damageType.DOT)
        {
            if (currentTarget != null)
            {
                return;
            }

            currentTarget = damageTarget;
            damageTarget.takeDamage(damageAmount);
            damageTimer = damageRate;
            return;
        }
        if (damageTarget != null)
        {
            if (type == damageType.enemyAttack)
            {
                if (hasDamagedPlayer)
                {
                    return;
                }

                hasDamagedPlayer = true;
            }

            damageTarget.takeDamage(damageAmount);
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
        if (other.isTrigger)
        {
            return;
        }

        IDamage damageTarget = other.GetComponentInParent<IDamage>();
        if (damageTarget == null)
        {
            return;
        }

        if (type == damageType.DOT)
        {
            if (currentTarget != damageTarget || damageTimer > 0f)
            {
                return;
            }

            damageTarget.takeDamage(damageAmount);
            damageTimer = damageRate;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        { return; }

        IDamage damageTarget = other.GetComponentInParent<IDamage>();

        if (damageTarget == null)
            return;

        if (type == damageType.DOT && currentTarget == damageTarget)
        {
            currentTarget = null;
            damageTimer = 0f;
        }
    }
}
