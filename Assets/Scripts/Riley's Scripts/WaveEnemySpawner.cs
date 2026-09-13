using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WaveEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] GameObject straferEnemyPrefab;
    [SerializeField] GameObject basicEnemyPrefab;

    [SerializeField] private float spawnRadius = 30f;
    [SerializeField] private float navMeshSearchDistance = 5f;
    [SerializeField] int spawnCount = 5;
    [SerializeField] private int extraEnemiesPerWave = 2;
    [SerializeField] private float timeBetweenWaves = 60f;
    [SerializeField] private float spawnRate = 0.5f;

    [Header("Enemy Spawn Weights")]
    [Range(0f, 100f)]
    [SerializeField] private float basicEnemyChance = 70f;
    [Range(0f, 100f)]
    [SerializeField] private float straferEnemyChance = 30;

    [Header("Trigger Settings")]
    [SerializeField] private bool onlyTriggerOnce = true;

    private float waveTimer;
    private float totalElapsedTime;
    private bool isSpawningActive = false;
    private bool hasBeenTriggered = false;

    void Start()
    {
        waveTimer = 0f;
        totalElapsedTime = 0f;
        isSpawningActive = false;
    }

    void Update()
    {
        if (!isSpawningActive) return;

        totalElapsedTime += Time.deltaTime;
        waveTimer += Time.deltaTime;

        if (waveTimer >= timeBetweenWaves)
        {
            StartCoroutine(SpawnWaveRoutine());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (onlyTriggerOnce && hasBeenTriggered)
            return;

        hasBeenTriggered = true;
        isSpawningActive = true;

        waveTimer = timeBetweenWaves;
    }

    IEnumerator SpawnWaveRoutine()
    {
        waveTimer = 0f;

        int timePassed = Mathf.FloorToInt(totalElapsedTime / timeBetweenWaves);
        int currentWaveCount = spawnCount + (timePassed * extraEnemiesPerWave);
        //added by sean
        DifficultyManager difficultyManager = DifficultyManager.GetInstance();
        if (difficultyManager != null)
        {
            currentWaveCount = difficultyManager.GetScaledEnemyCount(currentWaveCount);
        }
        //end added by sean

        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        if (navMeshData.indices.Length == 0)
        {
            Debug.LogError("No baked NavMesh found in the scene! Cannot spawn wave.");
            isSpawningActive = false;
            yield break;
        }

        for (int i = 0; i < currentWaveCount; i++)
        {
            while (true)
            {
                Vector3 spawnPos = GetRandomPointOnNavMesh(navMeshData);
                NavMeshHit hit;
                if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
                {
                    GameObject enemyPrefab = GetRandomEnemyPrefab();

                    if (enemyPrefab == null)
                    {
                        Debug.LogError("No enemy prefabs assigned to WaveEnemySpawner.");
                        isSpawningActive = false;
                        yield break;
                    }

                    GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);

                    if (enemyPrefab == basicEnemyPrefab)
                    {
                        BasicEnemy enemyAI = enemy.GetComponent<BasicEnemy>();
                        if (enemyAI != null)
                        {
                            enemyAI.SetBossEnemy();
                        }
                    }

                    NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

                    if (agent != null)
                    {
                        agent.Warp(spawnPos);
                    }
                    yield return new WaitForSeconds(spawnRate);
                    break;
                }
            
            }
            waveTimer = 0f;
            isSpawningActive = false;
        }
    }

    private Vector3 GetRandomPointOnNavMesh(NavMeshTriangulation data)
    {
        // Pick a random triangle index from the mesh data
        int randomTriangleIndex = Random.Range(0, data.indices.Length / 3) * 3;

        // Extract the three vertices that form that specific triangle
        Vector3 vertexA = data.vertices[data.indices[randomTriangleIndex]];
        Vector3 vertexB = data.vertices[data.indices[randomTriangleIndex + 1]];
        Vector3 vertexC = data.vertices[data.indices[randomTriangleIndex + 2]];

        // Generate a uniform random point within that triangle using barycentric coordinates
        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;

        Vector3 randomPoint = (1 - r1) * vertexA + (r1 * (1 - r2)) * vertexB + (r1 * r2) * vertexC;
        return randomPoint;
    }


    private GameObject GetRandomEnemyPrefab()
    {
        float totalWeight = basicEnemyChance + straferEnemyChance;

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < basicEnemyChance)
        {
            return basicEnemyPrefab;
        }

        return straferEnemyPrefab;
    }
}