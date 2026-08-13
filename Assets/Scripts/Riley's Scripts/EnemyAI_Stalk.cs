using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Range(10, 50)][SerializeField] int HP;
    [Range(100f, 300f)][SerializeField] float listenerRange;
    [Range(50f, 100f)][SerializeField] float detectionRange;
    [Range(20f, 40f)][SerializeField] float minStalkDistance;
    [Range(40f, 80f)][SerializeField] float maxStalkDistance;
    [Range(1f, 10f)][SerializeField] float hearingSensitivity;

    [SerializeField] float movementThreshold = 0.05f;

    [Header("Attack Settings")]
    [SerializeField] float attackReach = 2f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Collider attackCollider;

    NavMeshAgent agent;
    Rigidbody playerRb;
    Vector3 lastPlayerPosition;

    public bool flee = false, stalk = false;
    public bool investigatingSound = false;
    private Vector3 soundTargetPosition;

    bool hit = false;
    public bool attack = false;
    public float attackTimer;
    public float afterAttack;
    public float stun;
    bool stopStun = false;

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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (gameManager.instance != null && gameManager.instance.player != null)
        {
            playerRb = gameManager.instance.player.GetComponent<Rigidbody>();
            lastPlayerPosition = PlayerTransform.position;
        }
    }

    void HearNoise(Vector3 noiseLocation, float noiseRadius)
    {
        if (PlayerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        // FIX: Ignore noises completely if we are currently Stalking, Fleeing, or Attacking
        if (attack || flee || stalk || distanceToPlayer <= maxStalkDistance)
        {
            return;
        }

        float distanceToNoise = Vector3.Distance(transform.position, noiseLocation);
        float actualHearingDistance = noiseRadius * hearingSensitivity;

        if (distanceToNoise <= listenerRange && distanceToNoise <= actualHearingDistance)
        {
            investigatingSound = true;
            soundTargetPosition = noiseLocation;

            agent.isStopped = false;
            agent.SetDestination(soundTargetPosition);
        }
    }

    void Update()
    {
        if (PlayerTransform == null) return;

        attackTimer += Time.deltaTime;
        afterAttack += Time.deltaTime;
        stun += Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        bool isPlayerMoving = CheckIfPlayerIsMoving();
        bool hasLineOfSight = CheckLineOfSight(distanceToPlayer);

        if (flee && distanceToPlayer > maxStalkDistance)
        {
            afterAttack = 10f;
            flee = false;
        }

        // Handle Stun (Highest Priority)
        if (stun < 1f)
        {
            if (!agent.isStopped) agent.isStopped = true;
            stopStun = true;
            return;
        }
        else if (stopStun)
        {
            agent.isStopped = false;
            stopStun = false;
        }

        // Handle Attack State
        if (attack)
        {
            TransformLookAtPlayer();
            Attack(distanceToPlayer);
            return;
        }

        // Trigger Attack Chance
        if (attackTimer > 30f && distanceToPlayer <= detectionRange && Random.Range(0f, 10f) < 5f && hasLineOfSight)
        {
            attack = true;
            investigatingSound = false;
            agent.isStopped = false;
            return;
        }

        // Flee if player is too close (High Priority over Sound)
        if (distanceToPlayer < minStalkDistance || (afterAttack < 10f && distanceToPlayer <= maxStalkDistance))
        {
            investigatingSound = false; // Drop sound investigation to run away
            if (afterAttack >= 10f)
            {
                TransformLookAtPlayer();
            }
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

        // Default Stalk Behavior (High Priority over Sound)
        if (distanceToPlayer <= detectionRange)
        {
            investigatingSound = false; // Drop sound investigation to stalk the player
            TransformLookAtPlayer();

            if (!isPlayerMoving && distanceToPlayer <= detectionRange)
            {
                if (!agent.isStopped) agent.isStopped = true;
                return;
            }

            if (agent.isStopped) agent.isStopped = false;

            if (agent.hasPath && !stalk)
            {
                StalkPlayer();
            }
            else if (agent.remainingDistance <= agent.stoppingDistance)
            {
                StalkPlayer();
            }
            if (distanceToPlayer > maxStalkDistance)
            {
                stalk = false;
            }

            return;
        }

        // Handle Sound Investigation (Lowest Priority fallback)
        if (investigatingSound)
        {
            if (agent.isStopped) agent.isStopped = false;

            // Stop investigating if we reached the sound origin point
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                investigatingSound = false;
            }
            return;
        }

        // Reset tracking states if nothing is happening out of range
        stalk = false;
        flee = false;
    }

    void TransformLookAtPlayer()
    {
        Vector3 lookDirection = new Vector3(PlayerTransform.position.x, transform.position.y, PlayerTransform.position.z);
        transform.LookAt(lookDirection);
    }

    bool CheckLineOfSight(float distanceToPlayer)
    {
        Vector3 startPos = transform.position + Vector3.up * 1f;
        Vector3 targetPos = PlayerTransform.position + Vector3.up * 1f;
        Vector3 direction = (targetPos - startPos).normalized;
        RaycastHit hitInfo;

        if (Physics.Raycast(startPos, direction, out hitInfo, distanceToPlayer))
        {
            if (hitInfo.transform == PlayerTransform) return true;
        }
        return false;
    }

    void Attack(float currentDistance)
    {
        if (PlayerTransform == null) return;

        agent.SetDestination(PlayerTransform.position);
        Vector3 startPos = transform.position + Vector3.up * 1f;
        RaycastHit hitInfo;

        if (Physics.Raycast(startPos, transform.forward, out hitInfo, attackReach, playerLayer))
        {
            if (hitInfo.transform == PlayerTransform)
            {
                Debug.Log("Player Hit!");
                afterAttack = 0f;
                attackTimer = 0f;
                attack = false;
                stalk = false;
                flee = false;
            }
        }
    }

    bool CheckIfPlayerIsMoving()
    {
        if (PlayerTransform == null) return false;
        if (playerRb != null)
        {
            return playerRb.linearVelocity.magnitude > movementThreshold;
        }
        float displacement = Vector3.Distance(PlayerTransform.position, lastPlayerPosition);
        lastPlayerPosition = PlayerTransform.position;
        return displacement > (movementThreshold * Time.deltaTime);
    }

    void StalkPlayer()
    {
        if (PlayerTransform == null) return;
        stalk = true;
        flee = false;
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distanceToPlayer > maxStalkDistance)
        {
            agent.SetDestination(PlayerTransform.position);
            stalk = false;
            return;
        }

        Vector3 randDir = Random.insideUnitSphere;
        randDir.y = 0;

        float randDis = Random.Range(minStalkDistance, maxStalkDistance);
        Vector3 targetPos = PlayerTransform.position + randDir.normalized * randDis;

        SetValidDestination(targetPos);
    }

    void FleeFromPlayer()
    {
        flee = true;
        stalk = false;
        Vector3 fleeDirection = (transform.position - PlayerTransform.position).normalized;
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
        stun = 0f;
        if (HP <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (PlayerTransform == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minStalkDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxStalkDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, listenerRange);
    }
}
