using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_EyesOnly : MonoBehaviour, IDamage
{
    [Header("Enemy Health")]
    [SerializeField] int hpMax = 20;
    private int hpCurrent = 0;

    [Header("Target Settings")]
    [SerializeField] LayerMask playerLayer;

    [Header("Movement & Combat")]
    public float viewRadius = 10f;
    [Range(0f, 360f)] public float viewAngle = 90f;
    //loses aggro after not seeing player for this amount of time
    public float timeToLoseAggro = 2f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float attackReach = 3f;
    [SerializeField] int attackDamage = 3;

    [Header("Random Patrol Settings")]
    public float patrolRadius = 15f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private NavMeshAgent agent;
    private float timeSinceLostPlayer;
    private float waitTimer;
    private bool isWaiting;
    private bool isChasing;

    private enum State { Patrol, Chase }
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

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                break;
            case State.Chase:
                ChaseLogic();
                break;
        }
    }

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        if (CanSeePlayer())
        {
            SwitchToState(State.Chase);
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                MoveToRandomPoint();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isWaiting = true;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
        }
    }

    void ChaseLogic()
    {
        
        agent.speed = chaseSpeed;
        Vector3 startPos = transform.position + Vector3.up * 1f;
        RaycastHit hitInfo;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                agent.isStopped = false;
            }
        }

        if (CanSeePlayer() && !isWaiting)
        {
            agent.SetDestination(PlayerTransform.position);
            timeSinceLostPlayer = 0f;
            if (Physics.Raycast(startPos, transform.forward, out hitInfo, attackReach, playerLayer))
            {
                if (hitInfo.transform == PlayerTransform)
                {
                    IDamage dmg = hitInfo.transform.GetComponent<IDamage>();
                    if (dmg != null)
                    {
                        dmg.takeDamage(attackDamage);
                    }

                    isWaiting = true;
                    waitTimer = 1f;
                    agent.isStopped = true;
                }
            }
        }
        else
        {
            timeSinceLostPlayer += Time.deltaTime;
            if (timeSinceLostPlayer >= timeToLoseAggro)
            {
                SwitchToState(State.Patrol);
                MoveToRandomPoint();
            }
        }
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = PlayerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= viewRadius)
        {
            Vector3 forward = transform.forward;
            float angleBetweenNodes = Vector3.Angle(forward, directionToPlayer);

            if (angleBetweenNodes <= viewAngle / 2f)
            {
                RaycastHit hit;
                Vector3 origin = transform.position + Vector3.up * 1f;
                Vector3 targetDir = (PlayerTransform.position + Vector3.up * 1f) - origin;
                if (Physics.Raycast(origin, targetDir.normalized, out hit, viewRadius))
                {
                    if (hit.transform == PlayerTransform)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void SwitchToState(State newState)
    {
        currentState = newState;
        if (newState == State.Chase)
        {
            timeSinceLostPlayer = 0f;
        }
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void takeDamage(int amount)
    {
        hpCurrent -= amount;
        Debug.Log($"{gameObject.name} HP: {hpCurrent}/{hpMax}");

        if (currentState != State.Chase)
        {
            SwitchToState(State.Chase);
        }

        if (hpCurrent <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}
