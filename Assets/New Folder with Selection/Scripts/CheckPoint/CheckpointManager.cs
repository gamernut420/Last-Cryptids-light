using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    private static bool hasCheckpoint;
    private static bool restoreAfterSceneLoad;
    private static string checkpointSceneName;
    private static Vector3 checkpointPosition;
    private static Quaternion checkpointRotation;
    private static int savedFuel;
    private static int savedBatteries;
    private static int savedRadios;
    private const int WeaponSlotCount = 3;
    private static readonly string[] savedWeaponNames = new string[WeaponSlotCount];
    private static readonly int[] savedMagazineAmmo = new int[WeaponSlotCount];
    private static readonly int[] savedReserveAmmo = new int[WeaponSlotCount];
    private static string savedActiveWeaponName;
    private static float suppressPlateUntilUnscaledTime;

    public static bool ShouldIgnorePlateActivation()
    {
        return Time.unscaledTime < suppressPlateUntilUnscaledTime;
    }

    public void SaveCheckpoint(Transform respawnPoint, PlayerInventory inventory)
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("CheckpointManager: No respawn point was supplied.", this);
            return;
        }

        if (inventory == null)
        {
            Debug.LogWarning("CheckpointManager: PlayerInventory was not found.", this);
            return;
        }

        checkpointSceneName = SceneManager.GetActiveScene().name;
        checkpointPosition = respawnPoint.position;
        checkpointRotation = respawnPoint.rotation;

        savedFuel = inventory.GetAmount("Fuel");
        savedBatteries = inventory.GetAmount("Battery");
        savedRadios = inventory.GetAmount("Radio");
        SaveWeapons(inventory.GetComponent<playerController>());
        hasCheckpoint = true;

        Debug.Log(
            $"Checkpoint saved. Fuel: {savedFuel}, " +
            $"Battery: {savedBatteries}, Radio: {savedRadios}",
            this
        );
    }

    public void LoadCheckpoint()
    {
        restoreAfterSceneLoad = hasCheckpoint;
        Time.timeScale = 1f;

        if (hasCheckpoint && !string.IsNullOrEmpty(checkpointSceneName))
        {
            SceneManager.LoadScene(checkpointSceneName);
        }
        else
        {
            Debug.LogWarning(
                "No checkpoint has been activated. Restarting the current scene.",
                this
            );
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void RestoreCheckpointIfNeeded(GameObject player, PlayerInventory inventory)
    {
        if (!restoreAfterSceneLoad || !hasCheckpoint) return;

        if (player == null || inventory == null)
        {
            Debug.LogWarning(
                "CheckpointManager: The player or inventory was not ready to restore.",
                this
            );
            return;
        }

        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.SetPositionAndRotation(checkpointPosition, checkpointRotation);
        suppressPlateUntilUnscaledTime = Time.unscaledTime + 1f;

        if (controller != null)
        {
            controller.enabled = true;
        }

        // A freshly reloaded scene begins with an empty runtime inventory.
        AddSavedAmount(inventory, "Fuel", savedFuel);
        AddSavedAmount(inventory, "Battery", savedBatteries);
        AddSavedAmount(inventory, "Radio", savedRadios);

        playerController weaponController = player.GetComponent<playerController>();
        StartCoroutine(RestoreWeaponsNextFrame(weaponController));

        restoreAfterSceneLoad = false;
        Physics.SyncTransforms();

        Debug.Log("Checkpoint loaded successfully.", this);
    }

    private void AddSavedAmount(
        PlayerInventory inventory,
        string itemName,
        int amount
    )
    {
        if (amount > 0)
        {
            inventory.AddItem(itemName, amount);
        }
    }

    private void SaveWeapons(playerController weaponController)
    {
      
        for (int slot = 0; slot < WeaponSlotCount; slot++)
        {
            savedWeaponNames[slot] = string.Empty;
            savedMagazineAmmo[slot] = 0;
            savedReserveAmmo[slot] = 0;
        }

        savedActiveWeaponName = string.Empty;

        if (weaponController == null) return;

        GameObject[] carriedWeapons = weaponController.GetWeaponsForCheckpoint();
        savedActiveWeaponName = weaponController.GetActiveWeaponNameForCheckpoint();

        int slotsToSave = Mathf.Min(carriedWeapons.Length, WeaponSlotCount);

        for (int slot = 0; slot < slotsToSave; slot++)
        {
            if (carriedWeapons[slot] == null) continue;

            GunController gun = carriedWeapons[slot].GetComponent<GunController>();
            if (gun == null) continue;

            savedWeaponNames[slot] = gun.GetWeaponName();
            savedMagazineAmmo[slot] = gun.GetCurrentAmmoForCheckpoint();
            savedReserveAmmo[slot] = gun.GetReserveAmmoForCheckpoint();

            Debug.Log($"Checkpoint weapon slot {slot + 1}: " + $"{savedWeaponNames[slot]}, " + $"ammo {savedMagazineAmmo[slot]} / {savedReserveAmmo[slot]}", this);
        }

        if (string.IsNullOrEmpty(savedActiveWeaponName))
        {
            Debug.LogWarning(
                "CheckpointManager: The player had no registered active weapon " +
                "when the checkpoint was activated.",
                this
            );
        }
        else
        {
            Debug.Log(
                $"Checkpoint active weapon: {savedActiveWeaponName}",
                this
            );
        }
    }

    private IEnumerator RestoreWeaponsNextFrame(playerController weaponController)
    {
        yield return null;

        if (weaponController == null) yield break;

        GunController[] sceneGuns = FindObjectsByType<GunController>(FindObjectsInactive.Include);

        bool[] gunAlreadyUsed = new bool[sceneGuns.Length];

        for (int slot = 0; slot < WeaponSlotCount; slot++)
        {
            string savedName = savedWeaponNames[slot];
            if (string.IsNullOrEmpty(savedName)) continue;

            bool foundGun = false;

            for (int gunIndex = 0; gunIndex < sceneGuns.Length; gunIndex++)
            {
                if (gunAlreadyUsed[gunIndex]) continue;

                GunController gun = sceneGuns[gunIndex];

                if (gun.GetWeaponName() != savedName) continue;

                gunAlreadyUsed[gunIndex] = true;
                foundGun = true;

                if (weaponController.RestoreWeaponSlotForCheckpoint(gun.gameObject, slot))
                {
                    gun.RestoreAmmoForCheckpoint(
                        savedMagazineAmmo[slot],
                        savedReserveAmmo[slot]
                    );
                }
                else
                {
                    foundGun = false;
                }

                break;
            }

            if (!foundGun)
            {
                Debug.LogWarning(
                    $"CheckpointManager: Could not find a scene gun named {savedName}.",
                    this
                );
            }
        }

        weaponController.EquipWeaponForCheckpoint(savedActiveWeaponName);
        weaponController.RefreshWeaponUIForCheckpoint();
    }
}