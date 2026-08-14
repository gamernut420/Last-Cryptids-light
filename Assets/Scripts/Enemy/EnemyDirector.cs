using UnityEngine;

public class EnemyDirector : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Transform[] spawnPoints;

    [Header("Spawn Rates (Seconds per spawn)")]
    [SerializeField] float normalSpawnRate = 5f;
    [SerializeField] float defendPhaseSpawnRate = 1.5f;

    private float spawnTimer;
    private bool isDefendPhaseActive = false;

    void Update()
    {
        if (!isDefendPhaseActive) return;

        spawnTimer += Time.deltaTime;

        float currentRate = isDefendPhaseActive ? defendPhaseSpawnRate : normalSpawnRate;

        if (spawnTimer >= currentRate)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    public void StartDefendPhase()
    {
        isDefendPhaseActive = true;
        Debug.Log("Defend Phase Active! Enemy attack intensity increased.");
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0) return;

        Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(enemyPrefab, randomSpawn.position, randomSpawn.rotation);
    }
}