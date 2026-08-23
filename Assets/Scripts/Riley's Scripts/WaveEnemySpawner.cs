using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WaveEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] GameObject enemyWavePrefab;
    [SerializeField] float spawnDistance = 30f;
    [SerializeField] int spawnCount = 5;
    [SerializeField] private float navMeshSearchRadius = 5f;
    [SerializeField] private int extraEnemiesPerMinute = 2;

    private float timeBetweenWaves = 60f;
    private float spawnRate = .5f;
    private float waveTimer;
    private float totalElapsedTime;
    private bool isSpawningActive = false;

    private Transform BeaconTransform
    {
        get
        {
            if (gameManager.instance != null && gameManager.instance.player != null)
            {
                return gameManager.instance.beacon.transform;
            }
            return null;
        }
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
                    waveTimer -= Time.deltaTime;
                    if (waveTimer <= 0f)
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
        int minutesPassed = Mathf.FloorToInt(totalElapsedTime / 60f);
        int currentWaveCount = spawnCount + (minutesPassed * extraEnemiesPerMinute);

        for (int i = 0; i < currentWaveCount; i++)
        {
            SpawnEnemyAtDistance();
            yield return new WaitForSeconds(spawnRate);
        }

        waveTimer = timeBetweenWaves;
        isSpawningActive = false;
    }

    public void SpawnEnemyAtDistance()
    {
        if (enemyWavePrefab == null || BeaconTransform == null)
        {
            Debug.LogWarning("Spawner missing Prefab or Center Target reference.");
            return;
        }

        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y) * spawnDistance;
        Vector3 rawSpawnPosition = BeaconTransform.position + offset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawSpawnPosition, out hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            Vector3 finalSpawnPoint = hit.position;
            Vector3 directionToTarget = BeaconTransform.position - finalSpawnPoint;
            directionToTarget.y = 0;

            Quaternion spawnRotation = Quaternion.LookRotation(directionToTarget);
        }
        else
        {
            Debug.LogWarning("Could not find valid NavMesh position at spawn point");
        }
    }

}