using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class EnemyAI_HearOnly : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [Header("Enemy Settings")]
    [Range(1, 10)][SerializeField] int HP;
    
    public float hearingSensitivity = 1f;
    public float timeToForgetSound = 2f;

    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float investigationSpeed = 10f;
    public float patrolRadius = 15f;

    private NavMeshAgent agent;
    private float memoryTimer;
    private Vector3 lastHeardPosition;

    private enum State { Patrol, InvestigateSound }
    private State currentState = State.Patrol;

    Color colorOrig;

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
        colorOrig = model.material.color;
        agent = GetComponent<NavMeshAgent>();
        MoveToRandomPoint();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                break;
            case State.InvestigateSound:
                InvestigationLogic();
                break;
        }
    }

    void HearNoise(Vector3 noisePosition, float loudnessRange)
    {
        float distanceToNoise = Vector3.Distance(transform.position, noisePosition);
        float actualHearingRange = loudnessRange * hearingSensitivity;

        if (distanceToNoise <= actualHearingRange)
        {
            lastHeardPosition = noisePosition;
            memoryTimer = timeToForgetSound;
            
            if (currentState != State.InvestigateSound)
            {
                currentState = State.InvestigateSound;
            }
        }
    }

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MoveToRandomPoint();
        }
    }

    void InvestigationLogic()
    {
        agent.speed = investigationSpeed;
        agent.SetDestination(lastHeardPosition);
        if (agent.remainingDistance < .5f)
        {
            memoryTimer -= Time.deltaTime;
        }

        if (memoryTimer <= 0f)
        {
            currentState = State.Patrol;
            MoveToRandomPoint();
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

    public void takeDamage(int amount)
    {
        HP -= amount;
        agent.SetDestination(GameManager.instance.player.transform.position);
        if (HP <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    void OnDrawGizmosSelected()
    {
        if (currentState == State.InvestigateSound)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, patrolRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastHeardPosition, 1f);
            Gizmos.DrawLine(transform.position, lastHeardPosition);
        }
    }
}
