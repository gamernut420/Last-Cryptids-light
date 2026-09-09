using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BasicEnemy : MonoBehaviour, IDamage
{
    [Header("Health")]
    [SerializeField] private int maxHP = 50;
    private int currentHP;

    [Header("Movement")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private LayerMask sightBlocker;

    [Header("Attack")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDuration = 0.5f;

    private float attackTimer;
    private bool attacking;

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

    private void Start()
    {
        currentHP = maxHP;

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        agent.speed = moveSpeed;

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        if (PlayerTransform == null) return;

        attackTimer -= Time.deltaTime;

        if (CanSeePlayer())
        {
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

            ChasePlayer(distanceToPlayer);
        }
        else
        {
            StopChasing();
        }
    }

    private bool CanSeePlayer()
    {
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
                return true;
            }
            return false;
        }
        return false;
    }

    private void ChasePlayer(float distanceToPlayer)
    {
        if (attacking) return;

        if (distanceToPlayer <= attackRange)
        {
            Attack();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(PlayerTransform.position);
        }
    }

    private void StopChasing()
    {
        if (attacking) return;

        agent.isStopped = true;
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
        agent.isStopped = true;

        Vector3 direction = PlayerTransform.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        yield return new WaitForSeconds(0.2f);

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
            yield return new WaitForSeconds(attackDuration);
            attackHitbox.SetActive(false);
        }

        agent.isStopped = false;
        attacking = false;
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

