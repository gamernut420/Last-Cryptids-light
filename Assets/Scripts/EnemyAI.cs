using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Range(10, 50)] [SerializeField] int HP;
    [Range(100f, 300f)] [SerializeField] float listenerRange;
    [Range(50f, 100f)] [SerializeField] float detectionRange;
    [Range(20f, 40f)] [SerializeField] float minStalkDistance;
    [Range(40f, 80f)] [SerializeField] float maxStalkDistance;
    [SerializeField] float movementThreshold = 0.05f;

    [Header("Attack Settings")]
    [SerializeField] float attackReach = 2f;
    [SerializeField] LayerMask playerLayer;
    
    public Transform player;
    NavMeshAgent agent;
    Rigidbody playerRb;
    Vector3 lastPlayerPosition;

    bool flee = false, stalk = false;
    bool hit = false;
    bool attack = false;
    float attackTimer;
    float afterAttack = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if(player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
            lastPlayerPosition = player.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        Vector3 lookDirection = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookDirection);

        attackTimer += Time.deltaTime;
        afterAttack += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isPlayerMoving = CheckIfPlayerIsMoving();
        bool hasLineOfSight = CheckLineOfSight(distanceToPlayer);
        
        if (attack)
        {
            Attack(distanceToPlayer);
            return;
        }

        //Change attackTimer to for a quicker attack debug
        if (attackTimer > 30 && distanceToPlayer <= detectionRange && Random.Range(0,10) < 5 && hasLineOfSight)
        {
            attack = true;
            agent.isStopped = false;
            return;
        }

        if (afterAttack < 10)
        {
            if (!flee)
                FleeFromPlayer();
            return;
        }

        if (!isPlayerMoving)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
            }
            return;
        }

        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        if (distanceToPlayer <= maxStalkDistance)
            stalk = false;

        if (distanceToPlayer < minStalkDistance)
        {
            if (agent.hasPath && !flee)
            {
                FleeFromPlayer();
            }
            else if (!agent.hasPath)
            {
                FleeFromPlayer();
            }
            return;
        }

        bool hasReachedDestination = agent.remainingDistance <= agent.stoppingDistance;

        if (hasReachedDestination || !agent.hasPath || agent.velocity.sqrMagnitude == 0f || distanceToPlayer > maxStalkDistance)
        {
            if (distanceToPlayer <= detectionRange)
            {
                if (agent.hasPath && !stalk)
                {
                    StalkPlayer();
                    stalk = true;
                }
                else if (!agent.hasPath)
                {
                    StalkPlayer();
                }
            }
        }
    }

    bool CheckLineOfSight(float distanceToPlayer)
    {
        // Cast a ray from the enemy's chest area toward the player's chest area
        Vector3 startPos = transform.position + Vector3.up * 1f;
        Vector3 targetPos = player.position + Vector3.up * 1f;
        Vector3 direction = (targetPos - startPos).normalized;

        RaycastHit hitInfo;
        if (Physics.Raycast(startPos, direction, out hitInfo, distanceToPlayer))
        {
            // Return true only if the ray directly strikes the player object
            if (hitInfo.transform == player)
            {
                return true;
            }
        }
        return false;
    }

    void Attack(float currentDistance)
    {
        agent.SetDestination(player.position);

        Vector3 startPos = transform.position + Vector3.up * 1f;
        RaycastHit hitInfo;

        if (Physics.Raycast(startPos, transform.forward, out hitInfo, attackReach, playerLayer))
        {
            if (hitInfo.transform == player)
            {
                Debug.Log("Player Hit!");

                afterAttack = 0;
                attackTimer = 0;
                attack = false;
                stalk = false;
                flee = false;
            }
        }
    }

    bool CheckIfPlayerIsMoving()
    {
        if(playerRb != null)
        {
            return playerRb.linearVelocity.magnitude > movementThreshold;
        }

        float displacement = Vector3.Distance(player.position, lastPlayerPosition);
        lastPlayerPosition = player.position;

        return displacement > (movementThreshold * Time.deltaTime);
    }

    void StalkPlayer()
    {
        stalk = true;
        flee = false;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > maxStalkDistance)
        {
            agent.SetDestination(player.position);
            return;
        }

        Vector3 randDir = Random.insideUnitSphere ;
        randDir.y = 0;

        float randDis = Random.Range(minStalkDistance, maxStalkDistance);
        Vector3 targetPos = player.position + randDir.normalized * randDis;

        SetValidDestination(targetPos);
    }

    void FleeFromPlayer()
    {
        flee = true;
        stalk = false;
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        fleeDirection.y = 0;

        Vector3 variance = Random.insideUnitSphere * 0.3f;
        variance.y = 0;
        fleeDirection = (fleeDirection + variance).normalized;

        float escapeDistance = maxStalkDistance;
        Vector3 targetPos = transform.position + fleeDirection * escapeDistance;

        SetValidDestination(targetPos);
    }

    void SetValidDestination(Vector3 targetPos)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        hit = true;

        if(HP <= 0)
        {
            //Trigger Win event
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minStalkDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxStalkDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.darkGreen;
        Gizmos.DrawWireSphere(transform.position, listenerRange);
    }
}
