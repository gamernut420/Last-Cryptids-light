using UnityEngine;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;


    [Header("Menu references")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuExtractionWin;
    [SerializeField] GameObject hud;

    [Header("Player")]
    public Image playerHPBar;
    public GameObject damageFlashPanel;

    [Header("Auto Set Variables (No need to touch)")]
    public GameObject player;
    public PlayerInventory playerInventory;
    public playerController playerScript;
    public cameraController cameraScript;

    public bool isPaused;
    public bool isExtracting;

    int enemiesRemaining = 0;

    float timeScaleOrig;
    

    int waveCounter;


    // Sets up references
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        timeScaleOrig = Time.timeScale;

        if (player != null)
        {
            playerInventory = player.GetComponent<PlayerInventory>();
            playerScript = player.GetComponent<playerController>();
            cameraScript = player.GetComponentInChildren<cameraController>();
        }
        else
        {
            Debug.LogError("GameManager: No GameObject found with the tag 'Player'!");
        }
        
    }


    
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


    // Pauses the game
    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        cameraScript.enabled = false;
        hud.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    // Unpauses the game
    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        cameraScript.enabled = true;
        hud.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }


    
    public void updateGameGoal(int amount)
    {
        // Update number of waver till you win
        waveCounter += amount;

        if (waveCounter <= 0)
        {
            // You Win!!
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }


    // completed beacon to win
    public void extractionWin()
    {
        statePause();
        hud.SetActive(false);
        menuActive = menuExtractionWin;
        menuActive.SetActive(true);
    }


    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        hud.SetActive(false);
        menuActive.SetActive(true);
    }


    // killed all ai to win
    public void ModifyEnemyCount(int ammount)
    {
        enemiesRemaining += ammount;

        if (enemiesRemaining <= 0)
        {
            statePause();
            menuActive = menuWin;
            hud.SetActive(false);
            menuActive.SetActive(true);
        }
    }

}