using UnityEngine.AI;
using UnityEngine;

public class EnemyAI_HearOnly : MonoBehaviour, IDamage
{
    [Header("Enemy Health")]
    [SerializeField] int hpMax = 40;
    private int hpCurrent;

    [Header("Hearing Settings")]
    public float hearingSensitivity = 1f;
    public float timeToForgetSound = 2f;

    [Header("Movement & Combat")]
    public float patrolSpeed = 2f;
    public float investigationSpeed = 10f;
    public float patrolRadius = 15f;
    public float attackRadius = 3f;
    [SerializeField] int attackDamage = 3;
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] LayerMask playerLayer;

    private NavMeshAgent agent;
    private float memoryTimer;
    private float attackTimer;
    private Vector3 lastHeardPosition;

    private enum State { Patrol, InvestigateSound, Attack }
    private State currentState = State.Patrol;

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
    void OnEnable()
    {
        NoiseManager.OnNoiseMade += HearNoise;
    }
    
    void OnDisable()
    {
        NoiseManager.OnNoiseMade -= HearNoise;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        hpCurrent = hpMax;
        MoveToRandomPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

        float absoluteDistanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        if (absoluteDistanceToPlayer <= 3f)
        {
            currentState = State.Attack;
        }

        attackTimer += Time.deltaTime;

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                break;
            case State.InvestigateSound:
                InvestigationLogic();
                break;
            case State.Attack:
                AttackLogic();
                break;
        }
    }

    void HearNoise(Vector3 noisePosition, float loudnessRange)
    {
        float distanceToNoise = Vector3.Distance(transform.position, noisePosition);
        float actualHearingRange = loudnessRange * hearingSensitivity;

        if (distanceToNoise <= actualHearingRange)
        {
            lastHeardPosition = noisePosition;
            memoryTimer = timeToForgetSound;
            
            if (currentState != State.InvestigateSound)
            {
                currentState = State.InvestigateSound;
            }
        }
    }

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MoveToRandomPoint();
        }
    }

    void InvestigationLogic()
    {
        agent.speed = investigationSpeed;
        agent.SetDestination(lastHeardPosition);
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        if (distanceToPlayer <= attackRadius)
        {
            currentState = State.Attack;
            return;
        }
        if (!agent.pathPending && agent.remainingDistance < .5f)
        {
            memoryTimer -= Time.deltaTime;
        }

        if (memoryTimer <= 0f)
        {
            currentState = State.Patrol;
            MoveToRandomPoint();
        }
    }

    void AttackLogic()
    {
        Vector3 targetDirection = new Vector3(PlayerTransform.position.x, transform.position.y, PlayerTransform.position.z);
        transform.LookAt(targetDirection);

        agent.SetDestination(PlayerTransform.position);

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;

            Vector3 rayOrigin = transform.position + Vector3.up * 1f;
            Vector3 rayDirection = transform.forward;
            RaycastHit hitInfo;

            if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, 3f, playerLayer))
            {
                if (hitInfo.transform == PlayerTransform)
                {
                    IDamage dmg = hitInfo.transform.GetComponent<IDamage>();
                    if (dmg != null)
                    {
                        dmg.takeDamage(attackDamage);
                    }
                }
            }
        }

        if (distanceToPlayer > attackRadius)
        {
            lastHeardPosition = PlayerTransform.position;
            currentState = State.InvestigateSound;
        }
    }

    public void takeDamage(int amount)
    {
        hpCurrent -= amount;
        
        if (currentState != State.Attack && PlayerTransform != null)
        {
            lastHeardPosition = PlayerTransform.position;
            currentState = State.InvestigateSound;
        }    

        if (hpCurrent <= 0)
        {
            Destroy (gameObject);
        }
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit hitInfo;
        if (NavMesh.SamplePosition(randomDirection, out hitInfo, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hitInfo.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (currentState == State.InvestigateSound)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastHeardPosition, 1f);
            Gizmos.DrawLine(transform.position, lastHeardPosition);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
