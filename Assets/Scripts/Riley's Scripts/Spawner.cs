using UnityEngine;
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

    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool hasTriggered = false;
    private bool encounterComplete = false;
    private bool spawning;
    private int totalSpawned;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        if (!useTrigger)
            StartSpawning();
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

        if (spawnLocations == null || spawnLocations.Length == 0)
            return;

        spawning = true;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (spawning)
        {
            CleanupSpawnedEnemies();

            if (maxTotalSpawns > 0 &&
                totalSpawned >= maxTotalSpawns)
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
        {
            return;
        }

        SpawnLocation location = GetSpawnLocation();

        if (location == null)
            return;

        if (location.spawnPoint == null)
            return;

        GameObject prefab = GetEnemyPrefab(location);

        if (prefab == null)
            return;

        GameObject enemy = Instantiate(prefab, location.spawnPoint.position, location.spawnPoint.rotation);

        if (enemy.GetComponent<BasicEnemy>())
        {
            BasicEnemy enemyAI = enemy.GetComponent<BasicEnemy>();

            if (enemyAI != null)
            {
                enemyAI.SetBossEnemy();
            }
        }

        spawnedEnemies.Add(enemy);
        totalSpawned++;
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
