using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("MainGameScene");
    }

    public void ExitGame()
    {
        Debug.Log("Exit button clicked!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
