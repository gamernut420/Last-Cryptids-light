using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class EnemyAI_HearOnly : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    private Material modelMat;

    [Header("Health")]
    [SerializeField] private int maxHP = 50;
    private int currentHP;

    [Header("Hearing Settings")]
    public float hearingSensitivity = 1f;
    public float timeToForgetSound = 2f;

    [Header("Audio")]
    [Range(0, 1)][SerializeField] float audStepsVol;

    [Header("Movement & Combat")]
    public float attackSpeed = 10f;
    public float patrolSpeed = 2f;
    public float investigationSpeed = 10f;
    public float patrolRadius = 15f;
    public float attackRadius = 3f;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackHitboxDuration = 0.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] LayerMask playerLayer;

    private bool attacking;

    [Header("Projectile")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileCooldown = 5f;
    [SerializeField] private float launchAngle = 10f;
    [SerializeField] private float projectileLifetime = 5f;

    private float projectileTimer;

    Color colorOrig;

    [SerializeField] Renderer leftEarRenderer;
    [SerializeField] Renderer rightEarRenderer;
    private Material leftEarMat;
    private Material rightEarMat;
    private Color leftEarOrig;
    private Color rightEarOrig;

    private NavMeshAgent agent;
    private float memoryTimer;
    private float attackTimer;
    private float pathUpdateTimer;
    private Vector3 lastHeardPosition;

    private Vector3 playerLastPosition;
    private bool isPlayerMoving;
    private bool isPlayerTouchingMe;
    bool isPlayingStep;
    private AudioManager footstepAudio;

    public enum State { Patrol, InvestigateSound, Attack }
    public State currentState = State.Patrol;

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
        currentHP = maxHP;

        CheckEnemyMaterial();
        MoveToRandomPoint();
        footstepAudio = GetComponent<AudioManager>();

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
        
        if (PlayerTransform != null)
        {
            playerLastPosition = PlayerTransform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

        projectileTimer -= Time.deltaTime;

        TrackPlayerMovement();
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

        if (agent.velocity.sqrMagnitude > 0.3f && !isPlayingStep)
        {
            if (footstepAudio != null)
            {
                StartCoroutine(PlayStep());
            }
        }
    }

    void CheckEnemyMaterial()
    {
        if (model != null)
        {
            modelMat = model.material;
            colorOrig = modelMat.color;
            modelMat.EnableKeyword("_EMISSION");
        }
        
        if (leftEarRenderer != null)
        {
            leftEarMat = leftEarRenderer.material;
            leftEarOrig = leftEarMat.GetColor("_BaseColor");
            leftEarMat.EnableKeyword("_EMISSION");
        }
        if (rightEarRenderer)
        {
            rightEarMat = rightEarRenderer.material;
            rightEarOrig = rightEarMat.GetColor("_BaseColor");
            rightEarMat.EnableKeyword("_EMISSION");
        }
    }

    //Ear goes red when alert of sound and back to original color when it loses aggro
    void SetEarsAlert(bool isAlert)
    {
        // Configure your normal colors vs alert colors
        Color targetLeftColor = isAlert ? Color.red : leftEarOrig;
        Color targetRightColor = isAlert ? Color.red : rightEarOrig;

        // Apply an HDR intensity multiplier (e.g., 3f) to the alert color to make it glow brightly
        Color emissionLeftColor = isAlert ? (Color.red * 3f) : leftEarOrig;
        Color emissionRightColor = isAlert ? (Color.red * 3f) : rightEarOrig;


        if (leftEarMat != null)
        {
            leftEarMat.SetColor("_BaseColor", targetLeftColor);
            leftEarMat.SetColor("_EmissionColor", emissionLeftColor);
        }

        if (rightEarMat != null)
        {
            rightEarMat.SetColor("_BaseColor", targetRightColor);
            rightEarMat.SetColor("_EmissionColor", emissionRightColor);
        }
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

    private void ThrowProjectile(Vector3 targetPosition)
    {
        if (projectile == null || projectileSpawnPoint == null) return;

        Vector3 launchVelocity = CalculateLaunchVelocity(projectileSpawnPoint.position, targetPosition, launchAngle);

        if (launchVelocity.sqrMagnitude <= 0.01f) return;

        GameObject thrownObj = Instantiate(projectile, projectileSpawnPoint.position, Quaternion.LookRotation(launchVelocity));

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

    void TrackPlayerMovement()
    {
        if (Vector3.Distance(PlayerTransform.position, playerLastPosition) > 0.01f)
        {
            isPlayerMoving = true;
        }
        else
        {
            isPlayerMoving = false;
        }
        playerLastPosition = PlayerTransform.position;

        float enemyRadius = agent.radius + 1f;
        isPlayerTouchingMe = Physics.CheckSphere(transform.position, enemyRadius, playerLayer);
    }

    void HearNoise(Vector3 noisePosition, float loudnessRange)
    {
        float distanceToNoise = Vector3.Distance(transform.position, noisePosition);
        float actualHearingRange = loudnessRange * hearingSensitivity;

        if (distanceToNoise <= actualHearingRange)
        {
           
            lastHeardPosition = noisePosition;
            memoryTimer = timeToForgetSound;

            SetEarsAlert(true);

            if (projectileTimer <= 0f && currentState != State.Attack)
            {
                ThrowProjectile(noisePosition);
                projectileTimer = projectileCooldown;
            }

            if (currentState != State.Attack)
            {
                currentState = State.InvestigateSound;
                agent.speed = investigationSpeed;

                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(lastHeardPosition);
                }
            }
        }
    }

    void PatrolLogic()
    {
        agent.stoppingDistance = 0;
        agent.speed = patrolSpeed;
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distanceToPlayer <= attackRadius && (isPlayerMoving || isPlayerTouchingMe))
        {
            currentState = State.Attack;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 2f)
        {
            MoveToRandomPoint();
        }
    }

    void InvestigationLogic()
    {
        agent.stoppingDistance = 0;
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        if (distanceToPlayer <= attackRadius && (isPlayerMoving || isPlayerTouchingMe))
        {
            currentState = State.Attack;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 2f)
        {
            memoryTimer -= Time.deltaTime;
        }

        if (memoryTimer <= 0f)
        {
            SetEarsAlert(false);
            currentState = State.Patrol;
            MoveToRandomPoint();
        }
    }

    void AttackLogic()
    {
        agent.stoppingDistance = 2;
        agent.speed = attackSpeed;
        Vector3 targetDirection = new Vector3(PlayerTransform.position.x, transform.position.y, PlayerTransform.position.z);
        transform.LookAt(targetDirection);

        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= 0.2f)
        {
            pathUpdateTimer = 0f;

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(PlayerTransform.position);
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (!isPlayerMoving && !isPlayerTouchingMe)
        {
            lastHeardPosition = PlayerTransform.position;
            memoryTimer = timeToForgetSound;
            currentState = State.InvestigateSound;
            agent.speed = investigationSpeed;
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(lastHeardPosition);
            }
            return;
        }

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            StartCoroutine(AttackRoutine());
        }

        if (distanceToPlayer > attackRadius)
        {
            lastHeardPosition = PlayerTransform.position;
            memoryTimer = timeToForgetSound;
            currentState = State.InvestigateSound;
            agent.speed = investigationSpeed;
            agent.SetDestination(lastHeardPosition);
        }
    }

    private IEnumerator AttackRoutine()
    {
        attacking = true;
        agent.isStopped = true;

        Vector3 targetDirection = PlayerTransform.position - transform.position;
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(targetDirection);
        }

        yield return new WaitForSeconds(0.2f);

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
            yield return new WaitForSeconds(attackHitboxDuration);
            attackHitbox.SetActive(false);
        }

        agent.isStopped = false;
        attacking = false;
    }

    public void takeDamage(int amount)
    {
        if (currentHP <= 0) return;

        currentHP -= amount;
        Debug.Log("Blind Enemy Health: " + currentHP + "/" + maxHP);
        StartCoroutine(flashRed());

        if (currentHP <= 0)
        {
            Die();
        }

        if (currentState != State.Attack && PlayerTransform != null)
        {
            lastHeardPosition = PlayerTransform.position;
            memoryTimer = timeToForgetSound;
            currentState = State.InvestigateSound;
            AttackLogic();
            SetEarsAlert(true);
        }    
    }

    private void Die()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        Destroy(gameObject);

        if (gameManager.instance != null)
        {
            gameManager.instance.AddKill();
        }
    }

    IEnumerator flashRed()
    {
        modelMat.color = Color.red;
        modelMat.SetColor("_EmissionColor", Color.red);
        yield return new WaitForSeconds(0.1f);
        modelMat.color = colorOrig;
        modelMat.SetColor("_EmissionColor", colorOrig);
    }

    IEnumerator PlayStep()
    {
        isPlayingStep = true;
        footstepAudio.PlaySound(audStepsVol);
        
        switch (currentState)
        {
            case State.Patrol:
                yield return new WaitForSeconds(0.5f);
                break;
            case State.InvestigateSound:
                yield return new WaitForSeconds(0.3f);
                break;
            case State.Attack:
                yield return new WaitForSeconds(0.3f);
                break;
        }
        isPlayingStep = false;
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit hitInfo;
        if (NavMesh.SamplePosition(randomDirection, out hitInfo, patrolRadius, NavMesh.AllAreas))
        {
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(hitInfo.position);
            }
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
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * attackRadius);
    }
}
