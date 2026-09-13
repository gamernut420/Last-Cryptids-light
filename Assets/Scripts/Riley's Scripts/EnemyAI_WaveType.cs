using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_WaveType : MonoBehaviour, IDamage
{
    public enum AIType { Strafer }
    [Tooltip("Leave as is; it will randomize with weights automatically on spawn.")]
    [SerializeField] private AIType currentType;
    [SerializeField] Renderer model;
    private Material modelMat;

    [Header("Attack Settings")]
    [SerializeField] GameObject attackHitbox;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackWindup = 0.2f;
    [SerializeField] float attackHitboxDuration = 0.5f;

    private bool isAttacking;
    private float attackTimer;

    [Header("Targeting Settings")]
    [SerializeField] float playerAggroRadius = 8f;

    [Header("Strafer AI Stats")]
    [SerializeField] private float straferMaxSpeed = 4f;
    [SerializeField] private float straferMinSpeed = 3f;
    [SerializeField] private float straferMaxHP = 40f;
    [SerializeField] private float straferMinHP = 30f;

    [Header("Runtime Stats (Read Only)")]
    [SerializeField] private float maxHP;
    [SerializeField] private float currentHP;
    [SerializeField] private float currentSpeed;

    [Header("Audio and Material")]
    [Range(0f, 1f)][SerializeField] float audStepsVol;
    [SerializeField] Renderer leftEyeRenderer;
    [SerializeField] Renderer rightEyeRenderer;
    Color colorOrig;
    private Material leftEyeMat;
    private Material rightEyeMat;
    private Color leftEyeOrig;
    private Color rightEyeOrig;

    private NavMeshAgent agent;
    private float strafeTimer;
    private bool isStunned;
    private float stunTimer;
    
    private bool isPlayingStep;
    private AudioManager footstepAudio;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InitializeStats();
        SetEnemyColor();
        SetEyeColor();
        footstepAudio = GetComponent<AudioManager>();
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;
        SetEyesStun(isStunned);

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                agent.isStopped = false;
            }
            return;
        }

        switch (currentType)
        {
            case AIType.Strafer:
                MoveStrafer();
                break;
        }

        if (agent.velocity.sqrMagnitude > 0.3f && !isPlayingStep)
        {
            if (footstepAudio != null)
            {
                StartCoroutine(PlayStep());
            }
        }

        Attack();
    }

    void SetEnemyColor()
    {
        if (model != null)
        {
            modelMat = model.material;
            modelMat.EnableKeyword("_EMISSION");
            
            modelMat.color = Color.yellow;
            modelMat.SetColor("_EmissionColor", Color.yellow);

            colorOrig = modelMat.color;
        }
    }
    
    void SetEyeColor()
    {
        if (leftEyeRenderer != null)
        {
            leftEyeMat = leftEyeRenderer.material;
            leftEyeOrig = leftEyeMat.GetColor("_BaseColor");
            leftEyeMat.EnableKeyword("_EMISSION");
        }
        if (rightEyeRenderer != null)
        {
            rightEyeMat = rightEyeRenderer.material;
            rightEyeOrig = rightEyeMat.GetColor("_BaseColor");
            rightEyeMat.EnableKeyword("_EMISSION");
        }
    }

    void SetEyesStun(bool stun)
    {
        Color targetLeftColor = stun ? Color.white : leftEyeOrig;
        Color targetRightColor = stun ? Color.white : rightEyeOrig;

        Color emissionLeftColor = stun ? (Color.white * 1.5f) : leftEyeOrig;
        Color emissionRightColor = stun ? (Color.white * 1.5f) : rightEyeOrig;

        if (leftEyeMat != null)
        {
            leftEyeMat.SetColor("_BaseColor", targetLeftColor);
            leftEyeMat.SetColor("_EmissionColor", emissionLeftColor);
        }
        if (rightEyeMat != null)
        {
            rightEyeMat.SetColor("_BaseColor", targetRightColor);
            rightEyeMat.SetColor("_EmissionColor", emissionRightColor);
        }
    }

    private void InitializeStats()
    {
        maxHP = Random.Range(straferMinHP, straferMaxHP);
        currentSpeed = Random.Range(straferMinSpeed, straferMaxSpeed);
        //added by sean
        DifficultyManager difficultyManager = DifficultyManager.GetInstance();
        if (difficultyManager != null)
        {
            maxHP = difficultyManager.GetScaledEnemyHealth(maxHP);
            currentSpeed = difficultyManager.GetScaledEnemySpeed(currentSpeed);
        }
        // end added by sean
        currentHP = maxHP;

        if (agent != null)
        {
            agent.speed = currentSpeed;
        }
    }

    private void MoveStrafer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        strafeTimer += Time.deltaTime;

        if (distanceToPlayer > playerAggroRadius)
        {
            agent.stoppingDistance = 0;
            float strafeDirection = Mathf.Sin(strafeTimer * 1f) > 0 ? 1f : -1f;
            Vector3 strafeOffset = transform.right * strafeDirection * 6f;

            Vector3 targetPosition = PlayerTransform.position + strafeOffset;
            agent.SetDestination(targetPosition);
        }
        else
        {
            agent.SetDestination(PlayerTransform.position);
            agent.stoppingDistance = 2;
        }
    }

    void Attack()
    {
        if (isAttacking) return;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distanceToPlayer > attackRange)
            return;

        StartCoroutine(AttackRoutine());
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
        Debug.Log($"{gameObject.name} took {amount} damage. HP remaining: {currentHP}");
        StartCoroutine(flashRed());
        if (currentHP <= 0)
        {
            Destroy(gameObject);
            Die();
        }
    }

    void Die()
    {
        if(gameManager.instance != null)
        {
            gameManager.instance.AddKill();
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        agent.isStopped = true;

        FacePlayer();

        yield return new WaitForSeconds(attackWindup);

        if (PlayerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

            if (distanceToPlayer <= attackRange)
            {
                FacePlayer();
                if (attackHitbox != null)
                {
                    attackHitbox.SetActive(true);
                    yield return new WaitForSeconds(attackHitboxDuration);
                    attackHitbox.SetActive(false);
                }
            }
        }
        attackTimer = attackCooldown;
        isAttacking = false;
        agent.isStopped = false;
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

        yield return new WaitForSeconds(0.5f);

        isPlayingStep = false;
    }

    public void ApplyFlashLightStun(float duration)
    {
        if (isStunned == true) return;

        isStunned = true;
        stunTimer = duration;
        if (agent != null) agent.isStopped = true;
        Debug.Log("Enemy is blinded by flashlight");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerAggroRadius);
    }
}
