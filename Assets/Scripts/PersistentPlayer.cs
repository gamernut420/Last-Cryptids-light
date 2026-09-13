using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentPlayer : MonoBehaviour
{
    public static string nextSpawnPoint;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        Debug.Log("Next spawn: " + nextSpawnPoint);

        if (string.IsNullOrEmpty(nextSpawnPoint))
            return;

        GameObject spawn = GameObject.Find(nextSpawnPoint);

        if (spawn != null)
        {
            Debug.Log("Found spawn at: " + spawn.transform.position);

            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;

            Debug.Log("Player moved to: " + transform.position);
        }
        else
        {
            Debug.LogError("Spawn not found: " + nextSpawnPoint);
        }

        nextSpawnPoint = null;
    }
}
