using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    public GameObject[] weaponPrefabs;

    private void Start()
    {
        SpawnWeapon();
    }

    public void SpawnWeapon()
    {
        if (weaponPrefabs.Length == 0)
        {
            Debug.LogWarning("No weapons assigned to this spawn point.");
            return;
        }

        int randomIndex = Random.Range(0, weaponPrefabs.Length);

        Instantiate(
            weaponPrefabs[randomIndex],
            transform.position,
            transform.rotation
        );
    }
}