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

        for (int i = 0; i < currentWaveCount; i++)
        {
            bool spawned = false;

            while (!spawned)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                Vector3 randomPosition = transform.position + new Vector3(Mathf.Cos(angle) * spawnRadius, 0f, Mathf.Sin(angle) * spawnRadius);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPosition, out hit, navMeshSearchDistance, NavMesh.AllAreas))
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
                        agent.Warp(hit.position);
                    }

                    spawned = true;
                    yield return new WaitForSeconds(spawnRate);
                }
            }
        }
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