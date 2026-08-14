using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableItem
    {
        public GameObject itemPrefab;
        public int minQuantity = 1;
        public int maxQuantity = 3;
    }

    [Header("Spawn Settings")]
    [SerializeField] List<SpawnableItem> itemsToSpawn = new List<SpawnableItem>();
    [SerializeField] Transform[] spawnLocations;
    [SerializeField] int totalItemsToSpawn = 10;

    void Start()
    {
        SpawnRandomItems();
    }

    void SpawnRandomItems()
    {
        if (spawnLocations.Length == 0 || itemsToSpawn.Count == 0)
        {
            Debug.LogWarning("ItemSpawner is missing spawn locations or items!");
            return;
        }

        for (int i = 0; i < totalItemsToSpawn; i++)
        {
            Transform randomSpot = spawnLocations[Random.Range(0, spawnLocations.Length)];
            SpawnableItem randomSpawnData = itemsToSpawn[Random.Range(0, itemsToSpawn.Count)];

            if (randomSpawnData.itemPrefab != null)
            {
                GameObject spawnedItem = Instantiate(
                    randomSpawnData.itemPrefab,
                    randomSpot.position,
                    randomSpot.rotation
                );
            }
        }
    }
}