using UnityEngine;
using UnityEngine.AI;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] GameObject itemToSpawn;
    [SerializeField] int amountToSpawn;
    [SerializeField] int spawnDist;

    int spawnCount;

    private void Start()
    {
        for(int i= 0; i < amountToSpawn; ++i)
        {
            spawn();
        }
    }

    void spawn()
    {
        spawnCount++;

        Vector3 ranPos = Random.insideUnitSphere * spawnDist;
        ranPos += transform.position;
        ranPos.y = 6;

        Instantiate(itemToSpawn, ranPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
    }

}