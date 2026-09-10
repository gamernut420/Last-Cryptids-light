using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WaveEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] GameObject enemyWavePrefab;
    [SerializeField] int spawnCount = 5;
    [SerializeField] private int extraEnemiesPerWave = 2;
    [SerializeField] private float timeBetweenWaves = 60f;
    [SerializeField] private float spawnRate = .5f;

    private float waveTimer;
    private float totalElapsedTime;
    private bool isSpawningActive = false;

    void Start()
    {
        waveTimer = timeBetweenWaves;
    }

    void Update()
    {
        if (gameManager.instance != null && gameManager.instance.beacon != null)
        {
            RescueBeacon beaconScript = gameManager.instance.beacon.GetComponent<RescueBeacon>();

            if (beaconScript != null && beaconScript.isRepaired)
            {
                totalElapsedTime += Time.deltaTime;
                if (!isSpawningActive)
                {
                    waveTimer += Time.deltaTime;
                    if (waveTimer >= timeBetweenWaves)
                    {
                        StartCoroutine(SpawnWaveRoutine());
                    }
                }
            }
        }
    }

    IEnumerator SpawnWaveRoutine()
    {
        isSpawningActive = true;
        int timePassed = Mathf.FloorToInt(totalElapsedTime / timeBetweenWaves);
        int currentWaveCount = spawnCount + (timePassed * extraEnemiesPerWave);
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
                    GameObject enemy = Instantiate(enemyWavePrefab, spawnPos, Quaternion.identity);

                    NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                    if (agent != null)
                    {
                        agent.Warp(spawnPos);
                    }

                    yield return new WaitForSeconds(spawnRate);
                    break;
                }
            }
        }

        waveTimer = 0f;
        isSpawningActive = false;
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

}