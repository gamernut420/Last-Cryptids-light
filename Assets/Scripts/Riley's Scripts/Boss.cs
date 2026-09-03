using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;
    public Transform throwPoint;

    [Header("Attack Settings")]
    public float throwCooldown = 3f;
    public float attackRange = 15f;
    public float launchAngle = 45f;

    private float throwTimer;
    private NavMeshAgent agent;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            FaceTarget();

            throwTimer += Time.deltaTime;
            if (throwTimer >= throwCooldown)
            {
                ThrowObject();
                throwTimer = 0f;
            }
        }
    }

    private void FaceTarget()
    {
        Vector3 direction = (PlayerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void ThrowObject()
    {
        if (projectilePrefab == null || throwPoint == null) return;

        GameObject thrownObj = Instantiate(projectilePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rb = thrownObj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 velocity = CalculateLaunchVelocity(throwPoint.position, PlayerTransform.position, launchAngle);
            rb.linearVelocity = velocity;
        }

        Destroy(thrownObj, throwCooldown);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

