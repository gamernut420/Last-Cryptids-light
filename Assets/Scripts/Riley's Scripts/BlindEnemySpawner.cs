using UnityEngine;
using UnityEngine.AI;

public class BlindEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyBlindPrefab;
    [SerializeField] private float spawnDistance = 30f;
    [SerializeField] private float navMeshSearchRadius = 5f;

    private Collider[] colliders;
    private static BlindEnemySpawner previousSpawner;

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
            if (gameManager.instance != null && gameManager.instance.beacon != null)
            {
                RescueBeacon beaconScript = gameManager.instance.beacon.GetComponent<RescueBeacon>();
                if (beaconScript != null && beaconScript.isRepaired)
                {
                    if (previousSpawner == this)
                    {
                        previousSpawner = null;
                    }
                    Destroy(gameObject);
                    return;
                }
            }

            GameObject existingEnemy = GameObject.FindWithTag("BlindEnemy");
            if (existingEnemy != null)
            {
                EnemyAI_HearOnly enemyAI = existingEnemy.GetComponent<EnemyAI_HearOnly>();
                if (enemyAI != null)
                {
                    if (enemyAI.currentState == EnemyAI_HearOnly.State.InvestigateSound || enemyAI.currentState == EnemyAI_HearOnly.State.Attack)
                    {
                        if (previousSpawner != null && previousSpawner != this)
                        {
                            previousSpawner.ReEnableSpawner();
                        }
                        DisableTrigger();
                        return;
                    }
                }

                Destroy(existingEnemy);
            }

            if (previousSpawner != null && previousSpawner != this)
            {
                previousSpawner.ReEnableSpawner();
            }
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
        previousSpawner = this;

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        this.enabled = false;
    }

    public void ReEnableSpawner()
    {
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        this.enabled = true;
    }
}
