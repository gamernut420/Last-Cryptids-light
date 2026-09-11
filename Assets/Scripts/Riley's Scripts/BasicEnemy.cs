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
    [SerializeField] Renderer model;
    private Material modelMat;

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
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float loseAggro = 2f;

    private float aggroTimer;
    private float attackTimer;
    private bool attacking;

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
    [SerializeField] private float launchAngle = 45f;
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

    Color colorOrig;
    private bool bossEnemy = false;

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
        RandomizeEnemyType();
        SetEnemyColor();
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

    private void Update()
    {
        if (PlayerTransform == null) return;

        attackTimer -= Time.deltaTime;
        aggroTimer -= Time.deltaTime;
        throwTimer += Time.deltaTime;

        CalculatePlayerVelocity();

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
    }

    void SetEnemyColor()
    {
        if (model != null)
        {
            modelMat = model.material;
            modelMat.EnableKeyword("_EMISSION");

            switch (aiType)
            {
                case AIType.Melee:
                    modelMat.color = Color.blue;
                    modelMat.SetColor("_EmissionColor", Color.blue);
                    break;
                case AIType.Range:
                    modelMat.color = Color.yellow;
                    modelMat.SetColor("_EmissionColor", Color.yellow);
                    break;
            }

            colorOrig = modelMat.color;
        }
    }

    private void Roam()
    {
        if (attacking) return;

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
        float currentDetectionRange;
        float currentFieldOfView;

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

        if (distanceToPlayer > detectionRange) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > fieldOfView / 2f) return false;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, detectionRange))
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
        else if (distanceToPlayer > rangedAttackRange - minimumRangedDistance)
        {
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

        attackTimer = attackCooldown;
    }

    private IEnumerator AttackRoutine()
    {
        attacking = true;
        StopMovement();
        FaceTarget();

        yield return new WaitForSeconds(0.2f);

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
            yield return new WaitForSeconds(attackDuration);
            attackHitbox.SetActive(false);
        }

        if (agent != null)
        {
            agent.isStopped = false;
        }
        attacking = false;
    }

    private void RangedAttack()
    {
        if (throwTimer < throwCooldown) return;

        if (projectilePrefab == null || throwPoint == null) return;

        ThrowObject();
        throwTimer = 0f;
    }

    private void ThrowObject()
    {
        if (projectilePrefab == null || throwPoint == null) return;

        Vector3 targetPosition = PredictTargetPosition();

        Vector3 launchVelocity =
            CalculateLaunchVelocity(throwPoint.position, targetPosition, launchAngle);

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

            float angle = launchAngle * Mathf.Deg2Rad;

            float gravity = Mathf.Abs(Physics.gravity.y);

            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);

            float denominator = 2f * cosAngle * cosAngle * (horizontalDistance * Mathf.Tan(angle) - verticalDistance);

            if (denominator <= 0.01f) return PlayerTransform.position;

            float projectileSpeed = Mathf.Sqrt(gravity * horizontalDistance * horizontalDistance / denominator);

            if (float.IsNaN(projectileSpeed) || float.IsInfinity(projectileSpeed) || projectileSpeed <= 0f)
            {
                return PlayerTransform.position;
            }

            float horizontalSpeed = projectileSpeed * cosAngle;

            if (horizontalSpeed <= 0.01f) return PlayerTransform.position;

            float flightTime = horizontalDistance / horizontalSpeed;

            targetPosition = PlayerTransform.position + velocity * flightTime;

            targetPosition.y = PlayerTransform.position.y;
        }

        return targetPosition;
    }

    private Vector3 CalculateLaunchVelocity(Vector3 startPoint, Vector3 targetPoint, float angleInDegrees)
    {
        Vector3 playerXZ = new Vector3(targetPoint.x, startPoint.y, targetPoint.z);
        float distanceXZ = Vector3.Distance(startPoint, playerXZ);
        float deltaY = targetPoint.y - startPoint.y;

        float radAngle = angleInDegrees * Mathf.Deg2Rad;
        float gravity = Physics.gravity.y;

        float velocitySquared = (gravity * distanceXZ * distanceXZ) / (2 * Mathf.Cos(radAngle) * Mathf.Cos(radAngle) * (deltaY - distanceXZ * Mathf.Tan(radAngle)));

        if (velocitySquared <= 0)
        {
            return (targetPoint - startPoint).normalized * 10f;
        }

        float totalSpeed = Mathf.Sqrt(velocitySquared);
        float forwardSpeed = totalSpeed * Mathf.Cos(radAngle);
        float verticalSpeed = totalSpeed * Mathf.Sin(radAngle);

        Vector3 directionXZ = (playerXZ - startPoint).normalized;
        Vector3 launchVelocity = directionXZ * forwardSpeed + Vector3.up * verticalSpeed;
        return launchVelocity;
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

    public void takeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log("Basic Enemy Health: " + currentHP + "/" + maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Basic Enemy Defeated!");

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // Draw detection range
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw the two edges of the field of view
        Vector3 leftDirection = Quaternion.Euler(0f, -fieldOfView / 2f, 0f) * transform.forward;

        Vector3 rightDirection = Quaternion.Euler(0f, fieldOfView / 2f, 0f) * transform.forward;

        Gizmos.DrawRay(transform.position, leftDirection * detectionRange);

        Gizmos.DrawRay(transform.position, rightDirection * detectionRange);
    }
}

