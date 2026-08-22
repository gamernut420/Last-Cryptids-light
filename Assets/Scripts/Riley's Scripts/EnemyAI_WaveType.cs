using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class EnemyAI_WaveType : MonoBehaviour
{
    public enum AIType { Tanky, Fast, Strafer}
    [Tooltip("Leave as is; it will randomize with weights automatically on spawn.")]
    [SerializeField] private AIType currentType;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float attackCooldown = 1.5f;

    [Header("Spawn Chance Weights")]
    [Tooltip("Higher numbers increase the chance of this type spawning.")]
    [SerializeField] public float tankyWeight = 25f;
    [SerializeField] public float fastWeight = 50f;
    [SerializeField] public float straferWeight = 25f;

    [Header("Tanky AI Stats")]
    [SerializeField] private float tankMaxSpeed = 2f;
    [SerializeField] private float tankMinSpeed = 1f;
    [SerializeField] private float tankMaxHP = 60f;
    [SerializeField] private float tankMinHP = 40f;

    [Header("Fast AI Stats")]
    [SerializeField] private float fastMaxSpeed = 7f;
    [SerializeField] private float fastMinSpeed = 5f;
    [SerializeField] private float fastMaxHP = 30f;
    [SerializeField] private float fastMinHP = 15f;

    [Header("Strafer AI Stats")]
    [SerializeField] private float straferMaxSpeed = 4f;
    [SerializeField] private float straferMinSpeed = 3f;
    [SerializeField] private float straferMaxHP = 40f;
    [SerializeField] private float straferMinHP = 30f;

    [Header("Runtime Stats (Read Only)")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float currentSpeed;

    private NavMeshAgent agent;
    private float strafeTimer;
    private bool isStunned;
    private float stunTimer;

    private float attackTimer;

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

    private Transform BeaconTransform
    {
        get
        {
            if (gameManager.instance != null && gameManager.instance.player != null)
            {
                return gameManager.instance.beacon.transform;
            }
            return null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        RandomizeEnemyType();
        InitializeStats();
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

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
            case AIType.Tanky:
            case AIType.Fast:
                MoveDirect();
                break;
            case AIType.Strafer:
                MoveStrafer();
                break;
        }

        Attack();
    }

    private void RandomizeEnemyType()
    {
        float totalWeight = tankyWeight + fastWeight + straferWeight;
        float randomRoll = Random.Range(0f, totalWeight);

        if (randomRoll < tankyWeight)
        {
            currentType = AIType.Tanky;
        }
        else if (randomRoll < tankyWeight + fastWeight)
        {
            currentType = AIType.Fast;
        }
        else
        {
            currentType = AIType.Strafer;
        }
    }

    private void InitializeStats()
    {
        switch (currentType)
        {
            case AIType.Tanky:
                currentHealth = Random.Range(tankMinHP, tankMaxHP);
                currentSpeed = Random.Range(tankMinSpeed, tankMaxSpeed);
                break;
            case AIType.Fast:
                currentHealth = Random.Range(fastMinHP, fastMaxHP);
                currentSpeed = Random.Range(fastMinSpeed, fastMaxSpeed);
                break;
            case AIType.Strafer:
                currentHealth = Random.Range(straferMinHP, straferMaxHP);
                currentSpeed = Random.Range(straferMinSpeed, straferMaxSpeed);
                break;
        }

        if (agent != null)
        {
            agent.speed = currentSpeed;
        }
    }

    private void MoveDirect()
    {
        agent.SetDestination(PlayerTransform.position);
    }

    private void MoveStrafer()
    {
        strafeTimer += Time.deltaTime;

        float strafeDirection = Mathf.Sin(strafeTimer * 1f) > 0 ? 1f : -1f;
        Vector3 strafeOffset = transform.right * strafeDirection * 15f;

        Vector3 targetPosition = PlayerTransform.position + strafeOffset;
        agent.SetDestination(targetPosition);
    }

    void Attack()
    {
        if (agent == null) return;

        RaycastHit hit;
        
    }

    public void ApplyFlashLightStun(float duration)
    {
        if (isStunned == true) return;

        isStunned = true;
        stunTimer = duration;
        if (agent != null) agent.isStopped = true;
        Debug.Log("Enemy is blinded by flashlight");
    }
}
