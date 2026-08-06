using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;
    private Rigidbody playerRb;
    private Vector3 lastPlayerPosition;

    [Range(50f, 100f)] [SerializeField] float detectionRange;
    [Range(20f, 40f)] [SerializeField] float minStalkDistance;
    [Range(40f, 80f)] [SerializeField] float maxStalkDistance;
    [SerializeField] float movementThreshold = 0.05f; 

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
            agent.isStopped = true;
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            StalkPlayer(distanceToPlayer);
        }
        else
        {
            agent.ResetPath();
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

    void StalkPlayer(float currentDistance)
    {
        Vector3 lookDirection = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookDirection);


        if(currentDistance > maxStalkDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else if(currentDistance < minStalkDistance)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minStalkDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxStalkDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
