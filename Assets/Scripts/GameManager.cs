using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class gameManager : MonoBehaviour
{
<<<<<<< HEAD
<<<<<<< HEAD
    public GameObject player;
    public static GameManager instance;
=======
    public static gameManager instance;
=======

>>>>>>> parent of 8b07894 (Take damage in progress)

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuLose;

    [SerializeField] Image playerHPBar;
    public GameObject player;
    public playerController playerScript;
    public GameObject damageFlashPanel;

>>>>>>> 483c77463df4decf38286182c7090723f5577009

    float timeScaleOrig;
    public bool isPaused;

    int waveCounter;

    void Awake()
    {
        instance = this; 
        player = GameObject.FindWithTag("Player");
        timeScaleOrig = Time.timeScale;
    }
<<<<<<< HEAD
    private GameState currentState;
    private void Awake()
    {
        currentState = GameState.Playing;
    }
    public void SetGameState(GameState newState)
    {
        currentState = newState;
=======
>>>>>>> 483c77463df4decf38286182c7090723f5577009

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
    }

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }
    public void updateGameGoal(int amount)
    {
        // Update number of waver till you win

    }
    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
}