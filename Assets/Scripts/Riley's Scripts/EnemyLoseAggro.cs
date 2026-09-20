using UnityEngine;
using UnityEngine.AI;

public class EnemyLoseAggro : MonoBehaviour
{
    [Header("Enemy Blocking")]
    [SerializeField] private LayerMask enemyLayer;

    private int playerInsideCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideCount++;

            if (playerInsideCount == 1)
            {
                LoseAggroForAllEnemies();
            }

            return;
        }
        if (IsEnemy(other.gameObject))
        {
            PushEnemyOut(other.transform);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsEnemy(other.gameObject))
        {
            PushEnemyOut(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideCount--;

            if (playerInsideCount <= 0)
            {
                playerInsideCount = 0;
                ResumeAggroForAllEnemies();
            }
        }
    }

    private bool IsEnemy(GameObject obj)
    {
        return ((1 << obj.layer) & enemyLayer) != 0;
    }

    private void LoseAggroForAllEnemies()
    {
        MonoBehaviour[] allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour obj in allObjects)
        {
            if (obj is IEnemyAI enemy)
            {
                enemy.LosePlayer();
            }
        }
    }

    private void ResumeAggroForAllEnemies()
    {
        MonoBehaviour[] allObjects =
            FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour obj in allObjects)
        {
            if (obj is IEnemyAI enemy)
            {
                enemy.ResumePlayerDetection();
            }
        }
    }

    private void PushEnemyOut(Transform enemy)
    {
        Vector3 closestPoint = GetComponent<Collider>()
            .ClosestPoint(enemy.position);

        Vector3 direction = enemy.position - closestPoint;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.position - enemy.position;
            direction.y = 0f;
        }

        direction.Normalize();

        Vector3 newPosition =
            closestPoint + direction * 1.5f;

        NavMeshHit hit;

        if (UnityEngine.AI.NavMesh.SamplePosition(
            newPosition,
            out hit,
            3f,
            UnityEngine.AI.NavMesh.AllAreas))
        {
            enemy.position = hit.position;
        }

        UnityEngine.AI.NavMeshAgent agent =
            enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent != null)
        {
            agent.ResetPath();
        }
    }
}
