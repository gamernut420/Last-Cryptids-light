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
    public float projectileLifetime = 5f;

    [Header("Prediction Settings")]
    public bool usePlayerRigidbody = true;
    public float estimatedPlayerSpeed = 5f;

    private float throwTimer;
    private Rigidbody playerRb;
    private Vector3 lastPlayerPosition;
    private Vector3 calculatedPlayerVelocity;
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
        if (PlayerTransform != null)
        {
            playerRb = PlayerTransform.GetComponent<Rigidbody>();
            lastPlayerPosition = PlayerTransform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

        if (!usePlayerRigidbody || playerRb == null)
        {
            calculatedPlayerVelocity = (PlayerTransform.position - lastPlayerPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastPlayerPosition = PlayerTransform.position;
        }
        else
        {
            calculatedPlayerVelocity = playerRb.linearVelocity;
        }

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

        Vector3 targetPosition = PredictTargetPosition();

        GameObject thrownObj = Instantiate(projectilePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rb = thrownObj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 velocity = CalculateLaunchVelocity(throwPoint.position, targetPosition, launchAngle);
            rb.linearVelocity = velocity;
        }

        Destroy(thrownObj, projectileLifetime);
    }

    private Vector3 PredictTargetPosition()
    {
        Vector3 targetPos = PlayerTransform.position;
        Vector3 targetVelocity = (usePlayerRigidbody && playerRb != null) ? playerRb.linearVelocity : calculatedPlayerVelocity;

        targetVelocity.y = 0;

        for (int i = 0; i < 3; i++)
        {
            float distanceXZ = Vector3.Distance(new Vector3(throwPoint.position.x, 0, throwPoint.position.z), new Vector3(targetPos.x, 0, targetPos.z));

            float radAngle = launchAngle * Mathf.Deg2Rad;
            float gravity = Mathf.Abs(Physics.gravity.y);

            float estimatedFlightTime = distanceXZ / (Mathf.Sqrt(distanceXZ * gravity / Mathf.Sin(2 * radAngle)) * Mathf.Cos(radAngle));
            if (float.IsNaN(estimatedFlightTime) || estimatedFlightTime <= 0) estimatedFlightTime = 1f;

            targetPos = PlayerTransform.position + targetVelocity * estimatedFlightTime;
        }
        return targetPos;
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

