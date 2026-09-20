using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BasicEnemy : MonoBehaviour, IDamage
{
    public enum AIType 
    { 
        Melee, 
        Range 
    }

    [Header("AI Type")]
    [SerializeField] private AIType aiType;

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private LayerMask sightBlocker;
    [SerializeField] private Animator animator;

    [Header("Roaming")]
    [SerializeField] private float roamRadius = 10f;
    [SerializeField] private float roamWaitTime = 2f;

    [Header("Melee")]

    [Header("Health")]
    [SerializeField] private float meleeMaxHP = 50f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float fieldOfView = 90f;
    
    [Header("Attack")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackSpeed = 6f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float loseAggro = 2f;

    private float aggroTimer;
    private float attackTimer;
    private bool attacking;
    private bool animationFinished;

    [Header("Range")]

    [Header("Health")]
    [SerializeField] private float rangeMaxHP = 30f;

    [Header("Movement")]
    [SerializeField] private float RmoveSpeed = 3.5f;
    [SerializeField] private float RdetectionRange = 30f;
    [SerializeField] private float RfieldOfView = 90f;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwCooldown = 3f;
    [SerializeField] private float rangedAttackRange = 15f;
    [SerializeField] private float minimumRangedDistance = 8f;
    [SerializeField] private float throwSpeed = 20f;
    [SerializeField] private float projectileLifetime = 5f;

    [Header("Prediction")]
    public bool usePlayerRigidbody = true;
    public float estimatedPlayerSpeed = 5f;

    private float throwTimer;
    private Rigidbody playerRb; 
    private Vector3 lastPlayerPosition;
    private Vector3 calculatedPlayerVelocity;

    private float roamTimer;
    private Vector3 roamPosition;
    
    private float maxHP;
    private float currentHP;
    private float speed;
    private float currentDetectionRange;
    private float currentFieldOfView;

    [Header("Audio")]
    [Range(0f, 1f)]
    [SerializeField] private float audStepsVol;
    private bool isPlayingStep;
    private AudioManager footstepAudio;
    private int spawnAreaMask;

    private bool bossEnemy = false;
    private bool dead;

    public void SetBossEnemy()
    {
        bossEnemy = true;
    }

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

    private void RandomizeEnemyType()
    {
        float randomRoll = Random.Range(0, 100);
        if (randomRoll < 60)
        {
            aiType = AIType.Melee;
        }
        else
        {
            aiType = AIType.Range;
        }
    }

    private void Start()
    {
        footstepAudio = GetComponent<AudioManager>();
        DetectSpawnNavMeshArea();
        RandomizeEnemyType();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (aiType == AIType.Melee)
        {
            maxHP = meleeMaxHP;
            speed = moveSpeed;
        }
        else
        {
            maxHP = rangeMaxHP;
            speed = RmoveSpeed;
        }
        //added by sean
        DifficultyManager difficultyManager = DifficultyManager.GetInstance();
        if (difficultyManager != null)
        {
            maxHP = difficultyManager.GetScaledEnemyHealth(maxHP);
            speed = difficultyManager.GetScaledEnemySpeed(speed);
            attackSpeed = difficultyManager.GetScaledEnemySpeed(attackSpeed);
        }
        //end added by sean

        currentHP = maxHP;

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null)
        {
            agent.speed = speed;
        }

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }    

        if (PlayerTransform != null)
        {
            playerRb = PlayerTransform.GetComponent<Rigidbody>();
            lastPlayerPosition = PlayerTransform.position;
        }
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

    private void Update()
    {
        if (PlayerTransform == null) return;

        attackTimer -= Time.deltaTime;
        aggroTimer -= Time.deltaTime;
        throwTimer += Time.deltaTime;

        CalculatePlayerVelocity();
        UpdateAnimation();

        if (CanSeePlayer() || aggroTimer > 0f || bossEnemy)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

            if (aiType == AIType.Melee)
            {
                MeleeBehavior(distanceToPlayer);
            }
            else
            {
                FaceTarget();
                RangedBehavior(distanceToPlayer);
            }
        }
        else
        {
            Roam();
        }

        if (agent.velocity.sqrMagnitude > 0.3f && !isPlayingStep)
        {
            if (footstepAudio != null)
            {
                StartCoroutine(PlayStep());
            }
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null || agent == null)
            return;

        if (dead)
            return;

        float currentSpeed = agent.velocity.magnitude;

        if (currentSpeed < 0.1f)
        {
            animator.SetFloat("Speed", 0f);
        }
        else if (currentSpeed < 5f)
        {
            animator.SetFloat("Speed", 1f);
        }
        else
        {
            animator.SetFloat("Speed", 2f);
        }
    }

    private void Roam()
    {
        if (attacking) return;

        agent.speed = speed;
        agent.isStopped = false;
        roamTimer -= Time.deltaTime;

        if (roamTimer <= 0f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * roamRadius;

            randomDirection += transform.position;
            randomDirection.y = transform.position.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
            {
                roamPosition = hit.position;
                agent.SetDestination(roamPosition);
            }
            roamTimer = roamWaitTime;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
        }
    }

    private bool CanSeePlayer()
    {
        if (aiType == AIType.Melee)
        {
            currentDetectionRange = detectionRange;
            currentFieldOfView = fieldOfView;
        }
        else
        {
            currentDetectionRange = RdetectionRange;
            currentFieldOfView = RfieldOfView;
        }

        Vector3 directionToPlayer = PlayerTransform.position - transform.position;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > currentDetectionRange) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > currentFieldOfView / 2f) return false;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, currentDetectionRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                aggroTimer = loseAggro;
                return true;
            }
            return false;
        }
        return false;
    }

    private void MeleeBehavior(float distanceToPlayer)
    {
        if (attacking) return;

        agent.speed = attackSpeed;

        if (distanceToPlayer <= attackRange)
        {
            StopMovement();
            FaceTarget();
            Attack();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(PlayerTransform.position);
        }
    }

    private void RangedBehavior(float distanceToPlayer)
    {
        if (attacking) return;

        if (distanceToPlayer < minimumRangedDistance)
        {
            BackAwayFromPlayer();
        }
        else if (distanceToPlayer > rangedAttackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(PlayerTransform.position);
        }
        else
        {
            StopMovement();
        }

        if (distanceToPlayer <= rangedAttackRange)
        {
            RangedAttack();
        }
    }

    private void Attack()
    {
        if (attackTimer > 0f) return;
        if (attacking) return;

        StartCoroutine(AttackRoutine());
    }

    public void EnableAttackHitbox()
    {
      if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
        }
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    public void EndAttack()
    {
        animationFinished = true;
    }

    private IEnumerator AttackRoutine()
    {
        attacking = true;
        animationFinished = false;
        StopMovement();
        FaceTarget();

        if (animator != null)
        {
            animator.SetTrigger("sword attack");
        }

        yield return new WaitUntil(() => animationFinished);

        if (agent != null)
        {
            agent.isStopped = false;
        }
        attacking = false;
        attackTimer = attackCooldown;
    }

    private void RangedAttack()
    {
        if (throwTimer < throwCooldown) return;

        if (projectilePrefab == null || throwPoint == null) return;

        StartCoroutine(RangedAttackRoutine());
        throwTimer = 0f;
    }

    private IEnumerator RangedAttackRoutine()
    {
        attacking = true;
        animationFinished = false;
        StopMovement();
        FaceTarget();

        if (animator != null)
            animator.SetTrigger("shoots gun_2");

        yield return new WaitUntil(() => animationFinished);

        if (agent != null)
            agent.isStopped = false;

        attacking = false;
    }

    public void FireProjectile()
    {
        if (dead)
            return;

        ThrowObject();
    }

    private void ThrowObject()
    {
        if (projectilePrefab == null || throwPoint == null) return;

        Vector3 targetPosition = PredictTargetPosition();

        Vector3 launchVelocity = CalculateLaunchVelocity(throwPoint.position, targetPosition, throwSpeed);

        if (launchVelocity.sqrMagnitude <= 0.01f) return;

        GameObject thrownObj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.LookRotation(launchVelocity));

        Rigidbody rb = thrownObj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = launchVelocity;
        }
        if (thrownObj != null)
        {
            Destroy(thrownObj, projectileLifetime);
        }
    }

    private void CalculatePlayerVelocity()
    {
        if (PlayerTransform == null) return;

        if (Time.deltaTime <= 0f) return;

        calculatedPlayerVelocity =(PlayerTransform.position - lastPlayerPosition) / Time.deltaTime;

        calculatedPlayerVelocity.y = 0f;

        lastPlayerPosition = PlayerTransform.position;
    }

    private Vector3 PredictTargetPosition()
    {
        if (PlayerTransform == null) return transform.position;

        Vector3 targetPosition = PlayerTransform.position;

        Vector3 velocity = calculatedPlayerVelocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude < 0.05f) return targetPosition;

        for (int i = 0; i < 5; i++)
        {
            Vector3 direction = targetPosition - throwPoint.position;
            float horizontalDistance = new Vector3(direction.x, 0f, direction.z).magnitude;
            float verticalDistance = direction.y;

            if (horizontalDistance < 0.01f)
                return targetPosition;

            float gravity = Mathf.Abs(Physics.gravity.y);
            float speedSquared = throwSpeed * throwSpeed;
            float discriminant = speedSquared * speedSquared - gravity *
                (gravity * horizontalDistance * horizontalDistance + 2f * verticalDistance * speedSquared);
            
            if (discriminant <= 0f) 
                return PlayerTransform.position;

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float tanAngle = (speedSquared - sqrtDiscriminant) / (gravity * horizontalDistance);
            float angle = Mathf.Atan(tanAngle);
            float horizontalSpeed = throwSpeed * Mathf.Cos(angle);

            if (horizontalSpeed <= 0.01f)
                return PlayerTransform.position;
          
            float flightTime = horizontalDistance / horizontalSpeed;
            targetPosition = PlayerTransform.position + velocity * flightTime;
            targetPosition.y = PlayerTransform.position.y;
        }

        return targetPosition;
    }

    private Vector3 CalculateLaunchVelocity(Vector3 startPoint, Vector3 targetPoint, float speed)
    {
        Vector3 displacement = targetPoint - startPoint;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);

        float distance = horizontalDisplacement.magnitude;
        float heightDifference = displacement.y;

        if (distance < 0.01f)
        {
            return displacement.normalized * speed;
        }

        float gravity = Mathf.Abs(Physics.gravity.y);
        float speedSquared = speed * speed;

        float discriminant = speedSquared * speedSquared - gravity * 
            (gravity * distance * distance + 2f * heightDifference * speedSquared);

        if (discriminant < 0f)
        {
            Debug.LogWarning("Projectile target is unreachable at speed " + speed);
            return displacement.normalized * speed;
        }

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);

        float angle = Mathf.Atan((speedSquared - sqrtDiscriminant) / (gravity * distance));
        Vector3 horizontalDirection = horizontalDisplacement.normalized;
        Vector3 velocity = horizontalDirection * (speed * Mathf.Cos(angle));
        velocity.y = speed * Mathf.Sin(angle);

        return velocity;
    }

    private void BackAwayFromPlayer()
    {
        if (agent == null) return;

        Vector3 directionAwayFromPlayer = transform.position - PlayerTransform.position;
        directionAwayFromPlayer.y = 0f;

        if (directionAwayFromPlayer.sqrMagnitude <= 0.01f) return;

        directionAwayFromPlayer.Normalize();
        Vector3 targetPosition = transform.position + directionAwayFromPlayer * 5f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    private void StopMovement()
    {
        if (agent == null) return;

        agent.isStopped = true;
    }

    private void FaceTarget()
    {
        if (PlayerTransform == null) return;

        Vector3 direction = PlayerTransform.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude <= 0.01f) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private IEnumerator PlayStep()
    {
        isPlayingStep = true;
        footstepAudio.PlaySound(audStepsVol);

        if (agent.speed == attackSpeed)
        {
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return new WaitForSeconds(0.6f);
        }

        isPlayingStep = false;
    }

    public void takeDamage(int amount)
    {
        if (dead)
            return;

        currentHP -= amount;
        Debug.Log("Basic Enemy Health: " + currentHP + "/" + maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (dead)
            return;

        dead = true;
        Debug.Log("Basic Enemy Defeated!");

        gameManager.instance.playerScript.ModifyPlayerFunds(10);
        gameManager.instance.AddKill();

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        if (agent != null)
            agent.isStopped = true;

        animator.ResetTrigger("sword attack");
        animator.ResetTrigger("shoots gun_2");
        animator.SetTrigger("Death");
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // Draw detection range
        Gizmos.DrawWireSphere(transform.position, currentDetectionRange);

        // Draw the two edges of the field of view
        Vector3 leftDirection = Quaternion.Euler(0f, -currentFieldOfView / 2f, 0f) * transform.forward;

        Vector3 rightDirection = Quaternion.Euler(0f, currentFieldOfView / 2f, 0f) * transform.forward;

        Gizmos.DrawRay(transform.position, leftDirection * currentDetectionRange);

        Gizmos.DrawRay(transform.position, rightDirection * currentDetectionRange);
    }
}

