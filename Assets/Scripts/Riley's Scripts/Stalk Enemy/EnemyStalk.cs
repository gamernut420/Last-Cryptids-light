using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyStalk : MonoBehaviour, IDamage
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] Renderer model;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask sightBlocker;
    [SerializeField] private Camera playerCamera;

    [Header("Animation Parameters")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string dieTrigger = "Die";
    private string currentAnimation = "";

    [Header("Health")]
    [SerializeField] private int maxHP = 50;
    private int currentHP;

    [Header("Stalking Distance")]
    [SerializeField] private float minStalkDistance = 20f;
    [SerializeField] private float maxStalkDistance = 40f;

    [Header("Stalking Movement")]
    [SerializeField] private float stalkSpeed = 5f;
    [SerializeField] private float positionUpdateTime = 2f;
    [SerializeField] private int positionAttempts = 20;

    [Header("Player Visibility")]
    [Tooltip("Half of the player's field of view. 45 = 90 degrees total FOV.")]
    [SerializeField] private float playerViewAngle = 45f;
    [SerializeField] private float playerViewDistance = 100f;

    [Header("Player Movement")]
    [SerializeField] private float playerMovementThreshold = 0.01f;
    private Vector3 lastPlayerPosition;
    private bool playerWasMoving;
    private bool wasVisible;

    [Header("Hiding")]
    [Tooltip("How far away from an obstacle the stalker will stanc.")]
    [SerializeField] private float hideDistance = 1.5f;
    [Tooltip("Maximum distance to search for nearby cover.")]
    [SerializeField] private float coverSearchDistance = 15f;
    [SerializeField] private float coverOffset = 1.5f;
    [Tooltip("How far to search when finding a valid NavMesh position.")]
    [SerializeField] private float hideNavMeshSearchDistance = 4f;

    [Header("Attack")]
    [SerializeField] private GameObject attackHitbox1;
    [SerializeField] private GameObject attackHitbox2;
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float attackCooldown = 10f;
    [SerializeField] private float attackHitboxDuration = 0.5f;
    [SerializeField] private float attackRetreatDistance = 15f;

    [Header("Audio")]
    [Range(0, 1)]
    [SerializeField] float audStepsVol;
    private AudioManager footstepAudio;
    bool isPlayingStep;

    private enum StalkerState
    {
        Stalking,
        Hiding,
        Attacking,
        Dead
    }

    private StalkerState currentState = StalkerState.Stalking;
    
    private Vector3 currentStalkPosition;
    private float positionTimer = 0f;
    private float attackTimer = 0f;

    private bool attacking = false;
    private bool hasStalkPosition;
    private bool isDead;
    private int spawnAreaMask;

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
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        DetectSpawnNavMeshArea();
        agent = GetComponent<NavMeshAgent>();
        currentHP = maxHP;
        agent.speed = stalkSpeed;
        footstepAudio = GetComponent<AudioManager>();

        if (attackHitbox1 != null)
        {
            attackHitbox1.SetActive(false);
        }

        if (attackHitbox2 != null)
        {
            attackHitbox2.SetActive(false);
        }

        if (playerCamera == null)
        {
            playerCamera = PlayerTransform.GetComponentInChildren<Camera>(true);
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
        if (isDead)
            return;

        if (PlayerTransform == null || playerCamera == null) return;

        UpdateTimers();
        UpdateAnimation();

        bool playerIsMoving = IsPlayerMoving();
        bool playerCanSeeMe = IsPlayerLookingAtMe();
        bool attackPlayer = CanAttack();
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (currentState == StalkerState.Hiding && distanceToPlayer >= maxStalkDistance && !playerCanSeeMe)
        {
            currentState = StalkerState.Stalking;
            hasStalkPosition = false;

            agent.isStopped = false;

            FindNewStalkPosition();

            lastPlayerPosition = PlayerTransform.position;
            playerWasMoving = playerIsMoving;

            return;
        }

        if (agent.velocity.sqrMagnitude > 0.3f && !isPlayingStep)
        {
            if (footstepAudio != null)
            {
                StartCoroutine(PlayStep());
            }
        }

        if (attackPlayer)
        {
            if (attacking) return;

            if (distanceToPlayer <= attackRange)
            {
                agent.isStopped = true;
                FacePlayer();
                StartCoroutine(AttackRoutine());
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(PlayerTransform.position);
            }
            return;
        }

        if (playerCanSeeMe && !attacking)
        {
            if (!wasVisible || ReachedStalkPosition())
            {
                EnterHiding();
            }

            wasVisible = true;
            agent.isStopped = false;
            lastPlayerPosition = PlayerTransform.position;
            playerWasMoving = playerIsMoving;
            return;
        }

        wasVisible = false;

        if (currentState == StalkerState.Hiding)
        {
            if (!IsCameraLookingAtAI())
                HandleHiding(playerIsMoving);
            lastPlayerPosition = PlayerTransform.position;
            playerWasMoving = playerIsMoving;
            return;
        }

        HandleStalking(playerIsMoving);
        lastPlayerPosition = PlayerTransform.position;
        playerWasMoving = playerIsMoving;
    }

    private void DetectSpawnNavMeshArea()
    {
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            return;
        }

        int areaIndex = hit.mask;
        spawnAreaMask = areaIndex;

        if (agent != null)
        {
            agent.areaMask = spawnAreaMask;
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName))
            return;

        if (currentAnimation == animationName)
            return;

        currentAnimation = animationName;

        animator.CrossFade(animationName, 0.1f);
    }

    private void UpdateAnimation()
    {
        if (animator == null || agent == null)
            return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat(speedParameter, speed, 0.1f, Time.deltaTime);
    }

    private void UpdateTimers()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (positionTimer > 0f)
            positionTimer -= Time.deltaTime;
    }

    private void HandleStalking(bool playerIsMoving)
    {
        if (!playerIsMoving)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        positionTimer -= Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distanceToPlayer < minStalkDistance)
        {
            MoveAwayFromPlayer();
            return;
        }

        if (distanceToPlayer > maxStalkDistance)
        {
            FindNewStalkPosition();
            positionTimer = positionUpdateTime;
            return;
        }

        if (positionTimer <= 0f || !hasStalkPosition)
        {
            FindNewStalkPosition();
            positionTimer = positionUpdateTime;
        }

        if (hasStalkPosition)
        {
            agent.SetDestination(currentStalkPosition);
        }
    }

    private void FindNewStalkPosition()
    {
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
            positionTimer = 0f;
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
            direction = -PlayerTransform.forward;
            direction.y = 0f;
        }

        direction.Normalize();

        Vector3 targetPosition = transform.position + direction * maxStalkDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 10f, NavMesh.AllAreas))
        {
            currentStalkPosition = hit.position;
            hasStalkPosition = true;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    private void EnterHiding()
    {
        currentState = StalkerState.Hiding;
        hasStalkPosition = false;
        positionTimer = 0f;
        FindHidePosition();
    }

    private void HandleHiding(bool playerIsMoving)
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
            if (!attacking)
                MoveAwayFromPlayer();
        }
    }

    private bool IsCameraLookingAtAI()
    {
        if (playerCamera == null)
            return false;

        Vector3 directionToAI = (transform.position - playerCamera.transform.position).normalized;
        float dot = Vector3.Dot(playerCamera.transform.forward, directionToAI);

        return dot > 0.5f;
    }

    private bool PlayerCanSeePosition(Vector3 position)
    {
        Vector3 direction = position - playerCamera.transform.position;

        float distance = direction.magnitude;

        if (distance <= 0.01f) return true;

        direction.Normalize();

        float angle = Vector3.Angle(PlayerTransform.forward, direction);

        if (angle > playerViewAngle) return false;

        if (Physics.Raycast(playerCamera.transform.position, direction, distance, sightBlocker))
        {
            return false;
        }
        return true;
    }

    private bool IsPlayerLookingAtMe()
    {
        if (playerCamera == null)
            return false;

        Vector3 target = transform.position;
        Vector3 directionToStalker = target - playerCamera.transform.position;
        float distance = directionToStalker.magnitude;

        if (distance > playerViewDistance)
            return false;

        float angle = Vector3.Angle(playerCamera.transform.forward, directionToStalker.normalized);
        Debug.Log($"Looking: {angle} degrees");
        Debug.DrawRay(playerCamera.transform.position, directionToStalker.normalized * distance, Color.red);

        if (angle > playerViewAngle)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, directionToStalker.normalized, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {

            return hit.transform == transform || hit.transform.IsChildOf(transform);
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
        if (!hasStalkPosition) return true;
        if (agent.pathPending) return false;

        return agent.remainingDistance <= agent.stoppingDistance + 0.5f;
    }

    private bool CanAttack()
    {
        if (attackTimer > 0) return false;

        if (PlayerTransform == null) return false;

        float distance = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distance > minStalkDistance && !attacking) return false;

        if (!CheckLineOfSight()) return false;

        return true;
    }

    private bool CheckLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up;
        Vector3 target = PlayerTransform.position + Vector3.up;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, playerLayer))
            return hit.transform == PlayerTransform || hit.transform.IsChildOf(PlayerTransform);

        return false;
    }

    public void EnableAttackHitbox()
    {
        if (attackHitbox1 != null)
        {
            attackHitbox1.SetActive(true);
        }

        if (attackHitbox2 != null)
        {
            attackHitbox2.SetActive(true);
        }
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox1 != null)
        {
            attackHitbox1.SetActive(false);
        }

        if (attackHitbox2 != null)
        {
            attackHitbox2.SetActive(false);
        }
    }

    private IEnumerator AttackRoutine()
    {
        attacking = true;
        agent.isStopped = true;
        currentState = StalkerState.Attacking;

        FacePlayer();

        PlayAnimation(attackTrigger);

        yield return new WaitForSeconds(1.5f);

        attackTimer = attackCooldown;
        FindHidePosition();
        currentState = StalkerState.Hiding;

        if (agent != null)
            agent.isStopped = false;

        attacking = false;
    }    

    private IEnumerator PlayStep()
    {
        isPlayingStep = true;
        footstepAudio.PlaySound(audStepsVol);

        switch (currentState)
        {
            case StalkerState.Stalking:
                yield return new WaitForSeconds(0.5f);
                break;
            case StalkerState.Hiding:
            case StalkerState.Attacking:
                yield return new WaitForSeconds(0.3f);
                break;
        }
        isPlayingStep = false;
    }

    private void FacePlayer()
    {
        if (PlayerTransform == null) return;

        Vector3 direction = PlayerTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    public void takeDamage(int amount)
    {
        if (currentHP <= 0) return;

        currentHP -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. HP: {currentHP} / {maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentState = StalkerState.Dead;
        attacking = false;
        DisableAttackHitbox();
        StopAllCoroutines();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.ResetTrigger(attackTrigger);
            animator.SetTrigger(dieTrigger);
            StartCoroutine(DeathRoutine());
        }
        else
            Destroy(gameObject, 2f);
    }

    private IEnumerator DeathRoutine()
    {
        isDead = true;

        if (agent != null)
            agent.isStopped = true;

        animator.ResetTrigger(attackTrigger);
        animator.SetTrigger(dieTrigger);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
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

