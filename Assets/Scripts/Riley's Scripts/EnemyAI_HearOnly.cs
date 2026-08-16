using UnityEngine.AI;
using UnityEngine;
using UnityEngine.InputSystem.Android;

public class EnemyAI_HearOnly : MonoBehaviour, IDamage
{
    [Header("Enemy Health")]
    [SerializeField] int hpMax = 40;
    private int hpCurrent;

    [Header("Hearing Settings")]
    public float hearingSensitivity = 1f;
    public float timeToForgetSound = 2f;

    [Header("Movement & Combat")]
    public float attackSpeed = 10f;
    public float patrolSpeed = 2f;
    public float investigationSpeed = 10f;
    public float patrolRadius = 15f;
    public float attackRadius = 3f;
    [SerializeField] int attackDamage = 3;
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] LayerMask playerLayer;

    [SerializeField] Renderer leftEarRenderer;
    [SerializeField] Renderer rightEarRenderer;
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

    private enum State { Patrol, InvestigateSound, Attack }
    private State currentState = State.Patrol;

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
        hpCurrent = hpMax;

        SetupEarMaterials();
        MoveToRandomPoint();
        
        if (PlayerTransform != null)
        {
            playerLastPosition = PlayerTransform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

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
    }

    //Ear goes red when alert of sound and back to original color when it loses aggro
    void SetupEarMaterials()
    {
        // Cache original colors using universal property keys
        if (leftEarRenderer != null && leftEarRenderer.gameObject.activeInHierarchy)
        {
            // Checks for URP standard properties first, falls back to legacy color properties
            leftEarOrig = leftEarRenderer.material.HasProperty("_BaseColor") ?
                leftEarRenderer.material.GetColor("_BaseColor") : leftEarRenderer.material.color;
        }
        else
        {
            Debug.LogError($"[Ear Setup Failed] Left Ear Renderer reference missing on {gameObject.name}!", this);
        }

        if (rightEarRenderer != null && rightEarRenderer.gameObject.activeInHierarchy)
        {
            rightEarOrig = rightEarRenderer.material.HasProperty("_BaseColor") ?
                rightEarRenderer.material.GetColor("_BaseColor") : rightEarRenderer.material.color;
        }
        else
        {
            Debug.LogError($"[Ear Setup Failed] Right Ear Renderer reference missing on {gameObject.name}!", this);
        }
    }

    void SetEarsAlert(bool isAlert)
    {
        Color targetLeftColor = isAlert ? Color.red : leftEarOrig;
        Color targetRightColor = isAlert ? Color.red : rightEarOrig;

        if (leftEarRenderer != null)
        {
            if (leftEarRenderer.material.HasProperty("_BaseColor"))
                leftEarRenderer.material.SetColor("_BaseColor", targetLeftColor); // URP Layout
            else
                leftEarRenderer.material.color = targetLeftColor; // Standard Layout
        }

        if (rightEarRenderer != null)
        {
            if (rightEarRenderer.material.HasProperty("_BaseColor"))
                rightEarRenderer.material.SetColor("_BaseColor", targetRightColor); // URP Layout
            else
                rightEarRenderer.material.color = targetRightColor; // Standard Layout
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

        float enemyRadius = agent.radius + 0.2f;
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

            if (currentState != State.Attack)
            {
                currentState = State.InvestigateSound;
                agent.speed = investigationSpeed;
                agent.SetDestination(lastHeardPosition);
            }
        }
    }

    void PatrolLogic()
    {
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
        agent.speed = attackSpeed;
        Vector3 targetDirection = new Vector3(PlayerTransform.position.x, transform.position.y, PlayerTransform.position.z);
        transform.LookAt(targetDirection);

        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= 0.2f)
        {
            pathUpdateTimer = 0f;
            agent.SetDestination(PlayerTransform.position);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (!isPlayerMoving && !isPlayerTouchingMe)
        {
            lastHeardPosition = PlayerTransform.position;
            memoryTimer = timeToForgetSound;
            currentState = State.InvestigateSound;
            agent.speed = investigationSpeed;
            agent.SetDestination(lastHeardPosition);
            return;
        }

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;

            Vector3 rayOrigin = transform.position + Vector3.up * 1f;
            Vector3 rayDirection = transform.forward;
            RaycastHit hitInfo;

            if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, 3f, playerLayer))
            {
                if (hitInfo.transform == PlayerTransform)
                {
                    IDamage dmg = hitInfo.transform.GetComponent<IDamage>();
                    if (dmg != null)
                    {
                        dmg.takeDamage(attackDamage);
                    }
                }
            }
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

    public void takeDamage(int amount)
    {
        hpCurrent -= amount;

        SetEarsAlert(true);

        if (currentState != State.Attack && PlayerTransform != null)
        {
            lastHeardPosition = PlayerTransform.position;
            memoryTimer = timeToForgetSound;
            currentState = State.InvestigateSound;
        }    

        if (hpCurrent <= 0)
        {
            Destroy (gameObject);
        }
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit hitInfo;
        if (NavMesh.SamplePosition(randomDirection, out hitInfo, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hitInfo.position);
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
