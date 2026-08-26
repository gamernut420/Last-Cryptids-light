using UnityEngine;
using UnityEngine.AI;

public class BlindEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyBlindPrefab;
    [SerializeField] private float spawnDistance = 30f;
    [SerializeField] private float navMeshSearchRadius = 5f;

    private Collider[] colliders;

    void Start()
    {
        colliders = GetComponents<Collider>();

        if (colliders.Length == 0)
        {
            Debug.LogWarning($"[TriggerEnemySpawner] {gameObject.name} has no Collider! Add a Box/Sphere Collider for the trigger to work.");
            return;
        }

        foreach (Collider col in colliders)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SpawnBlindEnemy();
        }
    }

    private void SpawnBlindEnemy()
    {
        if (enemyBlindPrefab == null)
        {
            Debug.LogWarning($"[TriggerEnemySpawner] ({gameObject.name}) Missing Prefab");
            return;
        }

        GameObject existingEnemy = GameObject.FindWithTag("BlindEnemy");
        if (existingEnemy != null) Destroy(existingEnemy);

        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y) * spawnDistance;
        Vector3 rawSpawnPosition = transform.position + offset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawSpawnPosition, out hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            Vector3 finalSpawnPoint = hit.position;

            Vector3 directionToTarget = transform.position - finalSpawnPoint;
            directionToTarget.y = 0;
            Quaternion spawnRotation = directionToTarget != Vector3.zero
                ? Quaternion.LookRotation(directionToTarget)
                : Quaternion.identity;

            Instantiate(enemyBlindPrefab, finalSpawnPoint, spawnRotation);
            DisableTrigger();
        }
    }

    private void DisableTrigger()
    {
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Destroy(this);
    }
}
