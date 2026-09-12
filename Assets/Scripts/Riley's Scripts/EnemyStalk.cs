using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour, IDamage
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] Renderer model;
    private Material modelMat;

    [Header("Stats")]
    [SerializeField] private int maxHP = 25;
    private int currentHP;

    [Header("Stalking Distance")]
    [SerializeField] private float minStalkDistance = 20f;
    [SerializeField] private float maxStalkDistance = 40f;

    [Header("Stalking Movement")]
    [SerializeField] private float stalkSpeed = 5f;
    [SerializeField] private float positionUpdateTime = 2f;
    [SerializeField] private int positionAttempts = 20;

    [Header("Player Visibility")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("Half of the player's field of view. 45 = 90 degrees total FOV.")]
    [SerializeField] private float playerViewAngle = 45f;

    [SerializeField] private LayerMask sightBlocker;

    [Header("Player Movement")]
    [SerializeField] private float playerMovementThreshold = 0.05f;
    private Vector3 lastPlayerPosition;
    private bool playerWasMoving;
    private bool wasVisible;

    [Header("Hiding")]
    [Tooltip("How far away from an obstacle the stalker will stanc.")]
    [SerializeField] private float hideDistance = 1.5f;
    [Tooltip("Maximum distance to search for nearby cover.")]
    [SerializeField] private float coverSearchDistance = 15f;
    [Tooltip("How far to search when finding a valid NavMesh position.")]
    [SerializeField] private float navMeshSearchDistance = 4f;

    private enum StalkerState
    {
        Stalking,
        Hiding
    }

    private StalkerState currentState = StalkerState.Stalking;
    
    private Vector3 currentStalkPosition;
    private float positionTimer;
    private bool hasStalkPosition;

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

    /*void OnEnable()
    {
        NoiseManager.OnNoiseMade += HearNoise;
    }

    void OnDisable()
    {
        NoiseManager.OnNoiseMade -= HearNoise;
    }*/

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHP = maxHP;
        agent.speed = stalkSpeed;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (PlayerTransform == null)
        {
            Debug.LogWarning("Stalker AI could not find the player.");
        }

        lastPlayerPosition = PlayerTransform.position;
    }

    /*void HearNoise(Vector3 noiseLocation, float noiseRadius)
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
    }*/

    void Update()
    {
        if (PlayerTransform == null || playerCamera == null) return;

        bool playerIsMoving = IsPlayerMoving();
        bool playerCanSeeMe = IsPlayerLookingAtMe();

        if (playerCanSeeMe)
        {
            if (!wasVisible)
            {
                HandleBeingSeen();
            }
            agent.isStopped = false;
            wasVisible = false;
            playerWasMoving = playerIsMoving;
            return;
        }

        wasVisible = false;

        if (currentState == StalkerState.Hiding)
        {
            if (!ReachedStalkPosition())
            {
                agent.isStopped = false;
                playerWasMoving = playerIsMoving;
                return;
            }

            agent.isStopped = true;

            if (playerIsMoving)
            {
                currentState = StalkerState.Stalking;
                hasStalkPosition = false;
                positionTimer = positionUpdateTime;
                agent.isStopped = false;
            }

            playerWasMoving = playerIsMoving;
            return;
        }

        HandleStalking(playerIsMoving);
        playerWasMoving = playerIsMoving;
    }

    private void HandleBeingSeen()
    {
        currentState = StalkerState.Hiding;
        hasStalkPosition = false;
        positionTimer = 0f;
        FindHidePosition();
    }

    private void HandleStalking(bool playerIsMoving)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distanceToPlayer < minStalkDistance)
        {
            MoveAwayFromPlayer();
            return;
        }

        if (distanceToPlayer > maxStalkDistance)
        {
            FindNewStalkPosition();
            return;
        }

        if (!playerIsMoving)
        {
            agent.isStopped = true;
            return;
        }

        if (playerIsMoving && !playerWasMoving)
        {
            agent.isStopped = false;
            hasStalkPosition = false;
            positionTimer = positionUpdateTime;
        }

        agent.isStopped = false;
        positionTimer += Time.deltaTime;

        if (positionTimer >= positionUpdateTime || !hasStalkPosition || ReachedStalkPosition())
        {
            FindNewStalkPosition();
        }

    }

    private void FindNewStalkPosition()
    {
        positionTimer = 0f;

        Vector3 bestPosition = transform.position;
        float bestScore = float.MinValue;

        bool foundPosition = false;

        for (int i = 0; i < positionAttempts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0f;

            if (randomDirection.sqrMagnitude < 0.01f) continue;

            randomDirection.Normalize();
            float distance = Random.Range(minStalkDistance, maxStalkDistance);

            Vector3 candidate = PlayerTransform.position + randomDirection * distance;

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(candidate, out hit, 5f, NavMesh.AllAreas)) continue;

            Vector3 validPosition = hit.position;
            float finalDistance = Vector3.Distance(validPosition, PlayerTransform.position);

            if (finalDistance < minStalkDistance || finalDistance > maxStalkDistance) continue;

            float score = ScoreStalkPosition(validPosition);

            if (!foundPosition || score > bestScore)
            {
                bestScore = score;
                bestPosition = validPosition;
                foundPosition = true;
            }
        }
        if (foundPosition)
        {
            currentStalkPosition = bestPosition;
            hasStalkPosition = true;

            agent.isStopped = false;
            agent.SetDestination(currentStalkPosition);
        }
    }

    private float ScoreStalkPosition(Vector3 position)
    {
        float score = 0f;
        Vector3 direction = position - PlayerTransform.position;

        if (direction.sqrMagnitude <= 0.01f) return float.MinValue;

        direction.Normalize();

        float behindAmount = Vector3.Dot(PlayerTransform.forward, direction);

        score -= behindAmount * 50f;

        if (!PlayerCanSeePosition(position))
        {
            score += 100f;
        }

        float idealDistance = (minStalkDistance + maxStalkDistance) * 0.5f;
        float actualDistance = Vector3.Distance(position, PlayerTransform.position);

        score -= Mathf.Abs(actualDistance - idealDistance);
        float movementDistance = Vector3.Distance(transform.position, position);

        score -= movementDistance * 0.1f;

        return score;
    }

    private void MoveAwayFromPlayer()
    {
        Vector3 direction = transform.position - PlayerTransform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Random.insideUnitCircle;
            direction.y = 0f;
        }

        direction.Normalize();

        Vector3 targetPosition = PlayerTransform.position + direction * maxStalkDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 10f, NavMesh.AllAreas))
        {
            currentStalkPosition = hit.position;
            hasStalkPosition = true;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    private void FindHidePosition()
    {
        Vector3 bestPosition = transform.position;
        float bestDistance = Mathf.Infinity;

        bool foundCover = false;

        Vector3 playerPosition = playerCamera.transform.position;

        for (int i = 0; i < positionAttempts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere;

            randomDirection.y = 0f;

            if (randomDirection.sqrMagnitude < 0.01f) continue;

            randomDirection.Normalize();

            if (!Physics.Raycast(transform.position + Vector3.up, randomDirection, out RaycastHit obstacleHit, coverSearchDistance, sightBlocker, QueryTriggerInteraction.Ignore))
                continue;

            Vector3 obstacleToPlayer = (playerPosition - obstacleHit.point).normalized;

            Vector3 candidate = obstacleHit.point - obstacleToPlayer * hideDistance;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 4f, NavMesh.AllAreas)) continue;

            Vector3 hidePosition = navHit.position;

            if (PlayerCanSeePosition(hidePosition)) continue;

            float distanceFromAI = Vector3.Distance(transform.position, hidePosition);

            if (distanceFromAI < bestDistance)
            {
                bestDistance = distanceFromAI;
                bestPosition = hidePosition;
                foundCover = true;
            }
        }

        if (foundCover)
        {
            currentStalkPosition = bestPosition;
            hasStalkPosition = true;

            agent.isStopped = false;
            agent.SetDestination(currentStalkPosition);
        }
        else
        {
            MoveAwayFromPlayer();
        }
    }

    private bool PlayerCanSeePosition(Vector3 position)
    {
        Vector3 direction = position + Vector3.up - playerCamera.transform.position;

        float distance = direction.magnitude;

        if (distance <= 0.01f) return true;

        direction.Normalize();

        float angle = Vector3.Angle(PlayerTransform.forward, direction);

        if (angle > playerViewAngle) return false;

        if(Physics.Raycast(playerCamera.transform.position, direction, distance, sightBlocker))
        {
            return false;
        }
        return true;
    }

    private bool IsPlayerLookingAtMe()
    {
        Vector3 directionToStalker = transform.position - playerCamera.transform.position;
        float distance = directionToStalker.magnitude;

        if (distance <= 0.01f) return false;

        directionToStalker.Normalize();

        float angle = Vector3.Angle(playerCamera.transform.forward, directionToStalker);

        if (angle > playerViewAngle) return false;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, directionToStalker, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return true;
            }
            return false;
        }
        return false;
    }

    private bool IsPlayerMoving()
    {
        Vector3 currentPosition = PlayerTransform.position;
        float movementAmount = Vector3.Distance(currentPosition, lastPlayerPosition);
        lastPlayerPosition = currentPosition;

        return movementAmount > playerMovementThreshold;
    }

    private bool ReachedStalkPosition()
    {
        if (!agent.hasPath) return true;
        if (agent.pathPending) return false;

        return agent.remainingDistance <= agent.stoppingDistance + 1f;
    }

    public void takeDamage(int amount)
    {
        currentHP -= amount;

        if (currentHP <= 0)
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
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentStalkPosition, 1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, coverSearchDistance);
    }
}

