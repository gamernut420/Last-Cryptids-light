using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Range(50f, 100f)] [SerializeField] float detectionRange;
    [Range(20f, 40f)] [SerializeField] float minStalkDistance;
    [Range(40f, 80f)] [SerializeField] float maxStalkDistance;
    [SerializeField] float movementThreshold = 0.05f; 
    
    public Transform player;
    private NavMeshAgent agent;
    private Rigidbody playerRb;
    private Vector3 lastPlayerPosition;
    private bool flee = false, stalk = false;

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

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isPlayerMoving = CheckIfPlayerIsMoving();

        if(!isPlayerMoving)
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
                flee = true;
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
        Vector3 lookDirection = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookDirection);

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
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minStalkDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxStalkDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
