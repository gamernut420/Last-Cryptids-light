using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleport : MonoBehaviour
{
    [SerializeField] string sceneName;
    [SerializeField] string spawnPointName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PersistentPlayer.nextSpawnPoint = spawnPointName;
            SceneManager.LoadScene(sceneName);
        }
    }
}
