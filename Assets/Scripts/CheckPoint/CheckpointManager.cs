using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    // Static data survives a scene reload during the current play session.
    private static bool hasCheckpoint;
    private static bool restoreAfterSceneLoad;
    private static string checkpointSceneName;
    private static Vector3 checkpointPosition;
    private static Quaternion checkpointRotation;

    private static int savedFuel;
    private static int savedBatteries;
    private static int savedRadios;

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
        savedRadios = inventory.GetAmount("Radio Tube");

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

        // The lose screen pauses with timeScale 0. Restore time before loading.
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

    public void RestoreCheckpointIfNeeded(
        GameObject player,
        PlayerInventory inventory
    )
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

        // CharacterController can block direct transform movement while enabled.
        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.SetPositionAndRotation(
            checkpointPosition,
            checkpointRotation
        );

        if (controller != null)
        {
            controller.enabled = true;
        }

        // A freshly reloaded scene begins with an empty runtime inventory.
        AddSavedAmount(inventory, "Fuel", savedFuel);
        AddSavedAmount(inventory, "Battery", savedBatteries);
        AddSavedAmount(inventory, "Radio Tube", savedRadios);

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
}