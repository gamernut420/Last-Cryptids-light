using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_WaveType : MonoBehaviour, IDamage
{
    public enum AIType { Tanky, Fast, Strafer}
    [Tooltip("Leave as is; it will randomize with weights automatically on spawn.")]
    [SerializeField] private AIType currentType;
    [SerializeField] Renderer model;
    private Material modelMat;

    [Header("Attack Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string beaconTag = "Beacon";
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] int attackDamage = 2;

    [Header("Targeting Settings")]
    [SerializeField] float playerAggroRadius = 8f;

    [Header("Spawn Chance Weights")]
    [Tooltip("Higher numbers increase the chance of this type spawning.")]
    [SerializeField] public float tankyWeight = 25f;
    [SerializeField] public float fastWeight = 50f;
    [SerializeField] public float straferWeight = 25f;

    [Header("Tanky AI Stats")]
    [SerializeField] private float tankMaxSpeed = 2f;
    [SerializeField] private float tankMinSpeed = 1f;
    [SerializeField] private float tankMaxHP = 60f;
    [SerializeField] private float tankMinHP = 40f;

    [Header("Fast AI Stats")]
    [SerializeField] private float fastMaxSpeed = 7f;
    [SerializeField] private float fastMinSpeed = 5f;
    [SerializeField] private float fastMaxHP = 30f;
    [SerializeField] private float fastMinHP = 15f;

    [Header("Strafer AI Stats")]
    [SerializeField] private float straferMaxSpeed = 4f;
    [SerializeField] private float straferMinSpeed = 3f;
    [SerializeField] private float straferMaxHP = 40f;
    [SerializeField] private float straferMinHP = 30f;

    [Header("Runtime Stats (Read Only)")]
    [SerializeField] private float maxHP;
    [SerializeField] private float currentHP;
    [SerializeField] private float currentSpeed;
    [SerializeField] private Transform currentTarget;

    Color colorOrig;

    private NavMeshAgent agent;
    private float strafeTimer;
    private bool isStunned;
    private float stunTimer;
    private float attackTimer;

    private Transform PlayerTransform
    {
        get
        {
            if (gameManager.instance != null && gameManager.instance.player != null)
            {
                return gameManager.instance.player.transform;
            }
            return null;
        }
    }

    private Transform BeaconTransform
    {
        get
        {
            if (gameManager.instance != null && gameManager.instance.player != null)
            {
                return gameManager.instance.beacon.transform;
            }
            return null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        RandomizeEnemyType();
        InitializeStats();
        SetEnemyColor();
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                agent.isStopped = false;
            }
            return;
        }

        SelectTarget();
        if (currentTarget == null) return;

        switch (currentType)
        {
            case AIType.Tanky:
            case AIType.Fast:
                MoveDirect();
                break;
            case AIType.Strafer:
                MoveStrafer();
                break;
        }

        Attack();
    }

    void SetEnemyColor()
    {
        if (model != null)
        {
            modelMat = model.material;
            modelMat.EnableKeyword("_EMISSION");
            
            switch (currentType)
            {
                case AIType.Tanky:
                    modelMat.color = Color.green;
                    modelMat.SetColor("_EmissionColor", Color.green);
                    break;
                case AIType.Fast:
                    modelMat.color = Color.cyan;
                    modelMat.SetColor("_EmissionColor", Color.cyan);
                    break;
                case AIType.Strafer:
                    modelMat.color = Color.yellow;
                    modelMat.SetColor("_EmissionColor", Color.yellow);
                    break;
            }

            colorOrig = modelMat.color;
        }
    }

    private void SelectTarget()
    {
        Transform player = PlayerTransform;
        Transform beacon = BeaconTransform;

        currentTarget = beacon;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float distanceToBeacon = Vector3.Distance(transform.position, beacon.position);

            if (distanceToPlayer <= playerAggroRadius && distanceToPlayer < distanceToBeacon)
            {
                currentTarget = player;
            }
        }
    }

    private void RandomizeEnemyType()
    {
        float totalWeight = tankyWeight + fastWeight + straferWeight;
        float randomRoll = Random.Range(0f, totalWeight);

        if (randomRoll < tankyWeight)
        {
            currentType = AIType.Tanky;
        }
        else if (randomRoll < tankyWeight + fastWeight)
        {
            currentType = AIType.Fast;
        }
        else
        {
            currentType = AIType.Strafer;
        }
    }

    private void InitializeStats()
    {
        switch (currentType)
        {
            case AIType.Tanky:
                maxHP = Random.Range(tankMinHP, tankMaxHP);
                currentSpeed = Random.Range(tankMinSpeed, tankMaxSpeed);
                break;
            case AIType.Fast:
                maxHP = Random.Range(fastMinHP, fastMaxHP);
                currentSpeed = Random.Range(fastMinSpeed, fastMaxSpeed);
                break;
            case AIType.Strafer:
                maxHP = Random.Range(straferMinHP, straferMaxHP);
                currentSpeed = Random.Range(straferMinSpeed, straferMaxSpeed);
                break;
        }

        currentHP = maxHP;

        if (agent != null)
        {
            agent.speed = currentSpeed;
        }
    }

    private void MoveDirect()
    {
        agent.SetDestination(currentTarget.position);
    }

    private void MoveStrafer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        float distanceToBeacon = Vector3.Distance(transform.position, BeaconTransform.position);

        if (currentTarget == BeaconTransform && distanceToBeacon <= playerAggroRadius && distanceToBeacon < distanceToPlayer)
        {
            agent.SetDestination(BeaconTransform.position);
        }

        else
        {
            strafeTimer += Time.deltaTime;

            if (distanceToPlayer > playerAggroRadius)
            {
                float strafeDirection = Mathf.Sin(strafeTimer * 1f) > 0 ? 1f : -1f;
                Vector3 strafeOffset = transform.right * strafeDirection * 8f;

                Vector3 targetPosition = PlayerTransform.position + strafeOffset;
                agent.SetDestination(targetPosition);
            }
            else
            {
                agent.SetDestination(PlayerTransform.position);
            }
        }
    }

    void Attack()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= attackRange)
        {
            RaycastHit hit;
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;

            if (Physics.Raycast(transform.position + Vector3.up, directionToTarget, out hit, attackRange))
            {
                if (hit.collider != null && hit.collider.CompareTag(playerTag) || hit.collider.CompareTag(beaconTag))
                {
                    ExecuteAttack(hit.collider.gameObject);
                }
            }
        }
    }

    private void ExecuteAttack(GameObject target)
    {
        attackTimer = attackCooldown;
        IDamage dmg = target.transform.GetComponent<IDamage>();
        if (dmg != null)
        {
            dmg.takeDamage(attackDamage);
        }
    }

    public void takeDamage(int amount)
    {
        if (currentHP <= 0) return;

        currentHP -= attackDamage;
        Debug.Log($"{gameObject.name} took {attackDamage} damage. HP remaining: {currentHP}");
        StartCoroutine(flashRed());
        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator flashRed()
    {
        modelMat.color = Color.red;
        modelMat.SetColor("_EmissionColor", Color.red);
        yield return new WaitForSeconds(0.1f);
        modelMat.color = colorOrig;
        modelMat.SetColor("_EmissionColor", colorOrig);
    }

    public void ApplyFlashLightStun(float duration)
    {
        if (isStunned == true) return;

        isStunned = true;
        stunTimer = duration;
        if (agent != null) agent.isStopped = true;
        Debug.Log("Enemy is blinded by flashlight");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerAggroRadius);
    }
}
