using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnLocation
    {
        public Transform spawnPoint;
        [Header("EnemyTypes Allowed At This Location")]
        public GameObject[] enemyPrefabs;
    }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Location")]
    [SerializeField] private SpawnLocation[] spawnLocations;

    [Header("Random Area Spawning")]
    [SerializeField] private bool useSpawnArea = false;
    [Tooltip("Enemies will randomly spawn inside this collider")]
    [SerializeField] private Collider spawnArea;
    [Tooltip("How many attempts to find a valid NavMesh position")]
    [SerializeField] private int maxSpawnAttempts = 10;
    [Tooltip("Maximum distance from the random point to find the NavMesh")]
    [SerializeField] private float navMeshSampleDistance = 5f;

    [Header("Spawn Settings")]
    [SerializeField] private int maxEnemiesAlive = 5;
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private int maxTotalSpawns = 10;
    [SerializeField] private bool randomEnemyType = true;
    [SerializeField] private bool randomSpawnLocation = true;

    [Header("Trigger Spawn")]
    [SerializeField] private bool useTrigger = false;
    [SerializeField] private bool triggerOnce = true;

    [Header("Encounter")]
    [SerializeField] private bool requireAllEnemiesDead = false;
    [SerializeField] private bool disableSpawnerAfterCompletion = true;

    [Header("Respawn/Reset")]
    [SerializeField] private bool resetOnPlayerDeath = true;
    [SerializeField] private bool resetWhenPlayerLeaves = true;

    [SerializeField] private float resetDistance = 200f;

    [SerializeField] private bool destroyEnemiesOnReset = true;
    [SerializeField] private float resetCheckRate = 0.5f;

    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool hasTriggered = false;
    private bool encounterComplete = false;
    private bool spawning;
    private bool isDistanceReset = false;
    private int totalSpawned;
    private Coroutine spawnCoroutine;
    private Coroutine resetCheckCoroutine;

    private Transform PlayerTransform
    {
        get
        {
            if (gameManager.instance != null &&
                gameManager.instance.player != null)
            {
                return gameManager.instance.player.transform;
            }

            return null;
        }
    }

    private void Start()
    {
        if (!useTrigger)
            StartSpawning();

        if (resetWhenPlayerLeaves || resetOnPlayerDeath)
        {
            resetCheckCoroutine = StartCoroutine(ResetCheckRoutine());
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (triggerOnce && hasTriggered)
            return;

        hasTriggered = true;

        StartSpawning();
    }

    private void StartSpawning()
    {
        if (spawning)
            return;

        if (encounterComplete)
            return;

        if (!useSpawnArea)
        {
            if (spawnLocations == null || spawnLocations.Length == 0)
                return;
        }
        else
        {
            if (spawnArea == null)
                return;
        }

        spawning = true;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (spawning)
        {
            CleanupSpawnedEnemies();

            if (maxTotalSpawns > 0 && totalSpawned >= maxTotalSpawns)
            {
                spawning = false;
                break;
            }

            if (spawnedEnemies.Count >= maxEnemiesAlive)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            SpawnEnemy();

            yield return new WaitForSeconds(spawnRate);
        }
        if (requireAllEnemiesDead)
        {
            yield return StartCoroutine(WaitForAllEnemiesDead());
        }
    }

    public void SpawnEnemy()
    {
        if (totalSpawned >= maxTotalSpawns && maxTotalSpawns > 0)
            return;

        GameObject prefab = null;
        Vector3 spawnPosition;
        Quaternion spawnRotation;

        if (useSpawnArea)
        {
            if (!TryGetRandomAreaSpawnPosition(out spawnPosition, out spawnRotation))
                return;

            prefab = GetRandomEnemyPrefab();
        }
        else
        {
            SpawnLocation location = GetSpawnLocation();

            if (location == null)
                return;

            if (location.spawnPoint == null)
                return;

            prefab = GetEnemyPrefab(location);

            if (prefab == null)
                return;

            spawnPosition = location.spawnPoint.position;
            spawnRotation = location.spawnPoint.rotation;
        }

        if (prefab == null)
            return;

        GameObject enemy = Instantiate(prefab, spawnPosition, spawnRotation);
        BasicEnemy enemyAI = enemy.GetComponent<BasicEnemy>();

        if (enemyAI != null && useTrigger)
            enemyAI.SetBossEnemy();

        spawnedEnemies.Add(enemy);
        totalSpawned++;
    }

    private bool TryGetRandomAreaSpawnPosition(out Vector3 spawnPosition, out Quaternion spawnRotation)
    {
        spawnPosition = Vector3.zero;
        spawnRotation = Quaternion.identity;

        if (spawnArea == null)
            return false;

        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z));

            if (!spawnArea.bounds.Contains(randomPoint))
                continue;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
                spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                return true;
            }
        }
        return false;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return null;

        if (randomEnemyType)
            return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        return enemyPrefabs[0];
    }

    private SpawnLocation GetSpawnLocation()
    {
        List<SpawnLocation> validLocations = new List<SpawnLocation>();

        foreach (SpawnLocation location in spawnLocations)
        {
            if (location == null)
                continue;

            if (location.spawnPoint == null)
                continue;

            if (location.enemyPrefabs == null || location.enemyPrefabs.Length == 0)
            {
                if (enemyPrefabs != null && enemyPrefabs.Length > 0)
                {
                    validLocations.Add(location);
                }
            }
            else
            {
                validLocations.Add(location);
            }
        }

        if (validLocations.Count == 0)
            return null;

        if (randomSpawnLocation)
            return validLocations[Random.Range(0, validLocations.Count)];

        return validLocations[0];
    }

    private GameObject GetEnemyPrefab(SpawnLocation location)
    {
        GameObject[] availablePrefabs;

        if (location.enemyPrefabs != null && location.enemyPrefabs.Length > 0)
        {
            availablePrefabs = location.enemyPrefabs;
        }
        else
        {
            availablePrefabs = enemyPrefabs;
        }

        if (availablePrefabs == null || availablePrefabs.Length == 0)
        {
            return null;
        }

        if (randomEnemyType)
        {
            return availablePrefabs[Random.Range(0, availablePrefabs.Length)];
        }
        return availablePrefabs[0];
    }



    public void SpawnSpecificEnemy(GameObject enemyPrefab, Transform spawnPoint)
    {
        if (enemyPrefab == null || spawnPoint == null)
            return;

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedEnemies.Add(enemy);
    }

    private void CleanupSpawnedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    private IEnumerator WaitForAllEnemiesDead()
    {
        while (true)
        {
            CleanupSpawnedEnemies();

            if (spawnedEnemies.Count == 0)
            {
                CompleteEncounter();
                yield break;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    public void ResetSpawner()
    {
        spawning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (destroyEnemiesOnReset)
        {
            DestroyAllSpawnedEnemies();
        }
        else
        {
            spawnedEnemies.Clear();
        }

        totalSpawned = 0;
        encounterComplete = false;

        if (!isDistanceReset)
        {
            hasTriggered = false;

            if (useTrigger)
            {
                return;
            }
        }

        isDistanceReset = false;
        StartSpawning();
    }

    private IEnumerator ResetCheckRoutine()
    {
        while (true)
        {
            if (PlayerTransform == null)
                continue;

            if (resetWhenPlayerLeaves)
            {
                float distance = Vector3.Distance(PlayerTransform.position, transform.position);

                if (distance >= resetDistance && GetAliveEnemyCount() < GetTotalSpawned())
                {
                    isDistanceReset = true;
                    ResetSpawner();
                }
            }
            yield return new WaitForSeconds(resetCheckRate);
        }
    }

    private void CompleteEncounter()
    {
        if (encounterComplete)
            return;

        encounterComplete = true;

        Debug.Log("Enemy encounter completed!");

        if (disableSpawnerAfterCompletion)
        {
            gameObject.SetActive(false);
        }
    }

    public void SpawnOneEnemy()
    {
        CleanupSpawnedEnemies();

        if (spawnedEnemies.Count >= maxEnemiesAlive)
            return;

        if (maxTotalSpawns > 0 &&
            totalSpawned >= maxTotalSpawns)
            return;

        SpawnEnemy();
    }

    public void DestroyAllSpawnedEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    public int GetAliveEnemyCount()
    {
        CleanupSpawnedEnemies();

        return spawnedEnemies.Count;
    }

    public int GetTotalSpawned()
    {
        return totalSpawned;
    }

    public void StopSpawning()
    {
        spawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
}
