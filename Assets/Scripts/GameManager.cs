using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public GameObject player;
    public static GameManager instance;

    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Won
    }
    private GameState currentState;
    private void Awake()
    {
        instance = this;
        currentState = GameState.Playing;
        player = GameObject.FindWithTag("Player");
    }
    public void SetGameState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                break;

            case GameState.Won:
                Time.timeScale = 0f;
                break;
        }
    }
    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
    
