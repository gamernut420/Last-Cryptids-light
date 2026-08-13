using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;


    // Menu references
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject hud;


    // Player camera and gun
    [SerializeField] cameraController cameraScript;
    [SerializeField] GameObject menuExtractionWin;
    [SerializeField] GunController gunScript;


    // death tracking for ai placed or spawned to trigger win
    [SerializeField] int enemiesRemaining = 2;

    public Image playerHPBar;
    public GameObject player;
    public playerController playerScript;
    public GameObject damageFlashPanel;


    float timeScaleOrig;
    public bool isPaused;

    int waveCounter;


    // Sets up references
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        timeScaleOrig = Time.timeScale;

        if (player != null)
        {
            playerScript = player.GetComponent<playerController>();
            cameraScript = player.GetComponentInChildren<cameraController>();
            gunScript = player.GetComponentInChildren<GunController>();
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
        gunScript.enabled = false;
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
        gunScript.enabled = true;
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
        gunScript.enabled = false;
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
    public void AllEnemysKilled()
    {
        enemiesRemaining--;

        if (enemiesRemaining <= 0)
        {
            statePause();
            menuActive = menuWin;
            hud.SetActive(false);
            menuActive.SetActive(true);
        }
    }

}