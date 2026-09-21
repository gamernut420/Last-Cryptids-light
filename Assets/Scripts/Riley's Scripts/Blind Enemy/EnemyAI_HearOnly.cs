using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class EnemyAI_HearOnly : MonoBehaviour, IDamage, IEnemyAI
{
    [Header("Hearing Settings")]
    public float hearingSensitivity = 1f;
    public float timeToForgetSound = 2f;

    [Header("Audio")]
    [Range(0, 1)][SerializeField] float audStepsVol;

    [Header("Health")]
    [SerializeField] private int maxHP = 50;
    private int currentHP;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [SerializeField] private string patrolIdleAnimation = "Demon|Idle1";
    [SerializeField] private string investigateIdleAnimation = "Demon|Idle2";
    [SerializeField] private string walkAnimation = "Demon|Walk1";
    [SerializeField] private string runAnimation = "Demon|Run1";
    [SerializeField] private string throwAnimation = "Demon|Throw";
    [SerializeField] private string hurtAnimation = "Demon|Get-Damage";
    [SerializeField] private string deathAnimation = "Demon|Death";
    [SerializeField] private string returnAnimation = "Demon|Throw-catch";
    [SerializeField] private string throwloopAnimation = "Demon|Throw-loop";

    private string currentAnimation = "";
    private bool projectileReleased;
    private bool returningProjectile;

    [Header("Movement & Combat")]
    public float attackSpeed = 10f;
    public float patrolSpeed = 2f;
    public float investigationSpeed = 10f;
    public float patrolRadius = 15f;
    public float attackRadius = 4f;
    [SerializeField] private GameObject punch1Hitbox;
    [SerializeField] private GameObject punch2Hitbox;
    [SerializeField] private GameObject punch3Hitbox;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] LayerMask playerLayer;

    private bool attacking;
    private bool animationFinished;

    [Header("Projectile")]
    [SerializeField] private GameObject projectile;
    private GameObject activeProjectile;
    [SerializeField] private Renderer handBall;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileCooldown = 5f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float projectileSpeed = 20f;

    private float projectileTimer;

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

    private bool dead;
    private bool throwing;
    private EnemyAudioManager enemyAudio;
    private int spawnAreaMask;
    private bool playerHiding;

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

    private void DetectSpawnNavMeshArea()
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            return;
        }
        agent.areaMask = hit.mask;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyAudio = GetComponent<EnemyAudioManager>();
        agent = GetComponent<NavMeshAgent>();
        DetectSpawnNavMeshArea();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        currentHP = maxHP;

        MoveToRandomPoint();
        footstepAudio = GetComponent<AudioManager>();

        DisableAllAttackHitboxes();

        if (PlayerTransform != null)
        {
            playerLastPosition = PlayerTransform.position;
        }

        PlayAnimation(patrolIdleAnimation);
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) return;

        if (PlayerTransform == null) return;

        if (playerHiding)
        {
            currentState = State.Patrol;
            PatrolLogic();
            return;
        }

        projectileTimer -= Time.deltaTime;

        TrackPlayerMovement();

        if (throwing)
            return;

        if (!attacking)
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

    public void LosePlayer()
    {
        if (dead)
            return;

        DisableAllAttackHitboxes();
        currentState = State.Patrol;
        playerHiding = true;
        attacking = false;
        throwing = false;
    }

    public void ResumePlayerDetection()
    {
        if (dead)
            return;

        agent.isStopped = false;
        playerHiding = false;
    }

    private void DisableAllAttackHitboxes()
    {
        if (punch1Hitbox != null)
            punch1Hitbox.SetActive(false);

        if (punch2Hitbox != null)
            punch2Hitbox.SetActive(false);

        if (punch3Hitbox != null)
            punch3Hitbox.SetActive(false);
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

    private Vector3 CalculateLaunchVelocity(Vector3 startPoint, Vector3 targetPoint, float speed)
    {
        Vector3 displacement = targetPoint - startPoint;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);

        float distance = horizontalDisplacement.magnitude;
        float heightDifference = displacement.y;

        if (distance < 0.01f)
        {
            return Vector3.up * speed;
        }

        float gravity = Mathf.Abs(Physics.gravity.y);
        float speedSquared = speed * speed;

        float discriminant = speedSquared * speedSquared - gravity * (gravity * distance * distance + 2f * heightDifference * speedSquared);

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

    private void ThrowProjectile(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (projectile == null || handBall == null) return;

        activeProjectile = Instantiate(projectile, spawnPosition, spawnRotation);
        Renderer[] renderers = activeProjectile.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
        
        HearOnlyProjectile projectileScript = activeProjectile.GetComponent<HearOnlyProjectile>();

        if (projectileScript != null)
        {
            projectileScript.Initialize(this);
        }
        else
        {
            Debug.LogError(
                "Projectile prefab is missing " +
                "HearOnlyProjectile!"
            );
        }

        Rigidbody rb = activeProjectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Vector3 launchVelocity = CalculateLaunchVelocity(activeProjectile.transform.position, lastHeardPosition, projectileSpeed);

            rb.linearVelocity = launchVelocity;
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

            if (currentState == State.Attack || attacking || throwing)
                return;

            if (projectileTimer <= 0f)
            {
                projectileTimer = projectileCooldown;
                StartCoroutine(ThrowAttackRoutine(noisePosition));
                return;
            }

            currentState = State.InvestigateSound;

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(lastHeardPosition);
            }
        }
    }

    public void ReleaseProjectile()
    {
        if (projectileReleased)
            return;

        projectileReleased = true;

        Vector3 spawnPosition = projectileSpawnPoint.transform.position;
        Quaternion spawnRotation = projectileSpawnPoint.transform.rotation;
        if (handBall != null)
            handBall.enabled = false;
        enemyAudio?.PlayProjectileAttack();
        ThrowProjectile(spawnPosition, spawnRotation);
        StartCoroutine(ProjectileReturnTimeout());
    }

    private IEnumerator ProjectileReturnTimeout()
    {
        yield return new WaitForSeconds(projectileLifetime);

        if (activeProjectile == null)
            yield break;

        Debug.Log("Projectile return timeout reached. Forcing return.");

        ProjectileHit();
    }

    public void ProjectileHit()
    {
        if (activeProjectile == null)
            return;

        if (!throwing)
            return;

        if (returningProjectile)
            return;

        returningProjectile = true;

        StartCoroutine(ReturnProjectile());
    }

    private IEnumerator ThrowAttackRoutine(Vector3 targetPosition)
    {
        if (throwing || attacking) 
            yield break;

        throwing = true;
        projectileReleased = false;
        agent.isStopped = true;

        agent.velocity = Vector3.zero;

        Vector3 targetDirection = targetPosition - transform.position;
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            while (Quaternion.Angle(transform.rotation, targetRotation) > 2f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
                yield return null;
            }

            transform.rotation = targetRotation;
        }

        PlayAnimation(throwAnimation);
        yield return new WaitUntil(() => projectileReleased);

        PlayAnimation(throwloopAnimation);
        yield return new WaitUntil(() => activeProjectile == null);

        yield return new WaitForSeconds(0.5f);

        agent.isStopped = false;
        throwing = false;

        currentState = State.InvestigateSound;
        PlayAnimation(runAnimation);
        agent.speed = investigationSpeed;

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(lastHeardPosition);
        }
    }

    private IEnumerator ReturnProjectile()
    {
        if (activeProjectile == null || handBall == null)
            yield break;
        
        Collider projectileCollider = activeProjectile.GetComponent<Collider>();
        if (projectileCollider != null)
            projectileCollider.enabled = false;

        Rigidbody rb = activeProjectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        float returnSpeed = 30f;
        float catchDistance = 0.05f;

        while (activeProjectile != null)
        {
            Vector3 target = projectileSpawnPoint.transform.position;
            activeProjectile.transform.position = Vector3.MoveTowards(activeProjectile.transform.position, target, returnSpeed * Time.deltaTime);

            if (Vector3.Distance(activeProjectile.transform.position, target) <= catchDistance)
            {
                activeProjectile.transform.position = target;
                GameObject projectileToDestroy = activeProjectile;
                activeProjectile = null;
                handBall.enabled = true;
                returningProjectile = false;
                Destroy(projectileToDestroy);
                PlayAnimation(returnAnimation);
                
                yield break;
            }
            yield return null;
        }
    }

    void PatrolLogic()
    {
        agent.stoppingDistance = 0;
        agent.speed = patrolSpeed;

        PlayAnimation(walkAnimation);

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
        agent.speed = investigationSpeed;

        PlayAnimation(runAnimation);

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

        if (agent.remainingDistance < 0.1f)
        {
            PlayAnimation(investigateIdleAnimation);
        }

        if (memoryTimer <= 0f)
        {
            currentState = State.Patrol;
            MoveToRandomPoint();
        }
    }

    void AttackLogic()
    {
        if (attacking)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return;
        }

        agent.stoppingDistance = 3.5f;
        agent.speed = attackSpeed;

        PlayAnimation(runAnimation);

        Vector3 targetDirection = new Vector3(PlayerTransform.position.x, transform.position.y, PlayerTransform.position.z);
        if (targetDirection.sqrMagnitude > 0.01f)
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
            agent.isStopped = false;
            agent.speed = investigationSpeed;
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(lastHeardPosition);
            }
            return;
        }

        if (attackTimer >= attackCooldown && distanceToPlayer < attackRadius)
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        if (distanceToPlayer > attackRadius)
        {
            agent.isStopped = false;
            lastHeardPosition = PlayerTransform.position;
            memoryTimer = timeToForgetSound;
            currentState = State.InvestigateSound;
            agent.speed = investigationSpeed;
            agent.SetDestination(lastHeardPosition);
        }
    }

    private IEnumerator AttackRoutine()
    {
        if (attacking || throwing || dead)
            yield break;

        attacking = true;
        animationFinished = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        PlayAnimation(investigateIdleAnimation);

        DisableAllAttackHitboxes();

        Vector3 targetDirection = PlayerTransform.position - transform.position;

        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation =
                Quaternion.LookRotation(targetDirection);
        }

        int attack = Random.Range(0, 3);

        switch (attack)
        {
            case 0:
                PlayAnimation("Demon|Punch1");
                break;

            case 1:
                PlayAnimation("Demon|Punch2");
                break;

            case 2:
                PlayAnimation("Demon|Punch3");
                break;
        }
        enemyAudio?.PlayMeleeAttack();

        yield return new WaitUntil(() => animationFinished);

        DisableAllAttackHitboxes();

        attackTimer = 0f;
        attacking = false;

        if (dead)
            yield break;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    public void EndAttack()
    {
        DisableAllAttackHitboxes();

        animationFinished = true;
    }

    public void takeDamage(int amount)
    {
        if (currentHP <= 0) return;

        currentHP -= amount;

        Debug.Log("Enemy takes" + amount + " damage");

        PlayAnimation(hurtAnimation);

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        if (currentState != State.Attack && PlayerTransform != null)
        {
            lastHeardPosition = PlayerTransform.position;
            memoryTimer = timeToForgetSound;
            currentState = State.InvestigateSound;
        }    
    }

    private void Die()
    {
        if (dead) return;

        dead = true;
        Debug.Log("Blind Enemy Defeated!");
        StopAllCoroutines();
        attacking = false;
        throwing = false;

        DisableAllAttackHitboxes();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        PlayAnimation(deathAnimation);
        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
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
        if (NavMesh.SamplePosition(randomDirection, out hitInfo, patrolRadius, agent.areaMask))
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
