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
    [SerializeField] GameObject countdownText;

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

        countdownText.SetActive(false);
    }



    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (isPaused)
            {
                stateUnpause();
            }
            else
            {
                statePause();
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

        menuActive = menuPause;
        menuActive.SetActive(true);

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

        if (menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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

    public void StartExtraction(float time)
    {
        countdownText.SetActive(true);

        ExtractionCountdown Timer = countdownText.GetComponent<ExtractionCountdown>();

        Timer.SetTimer(time);

        isExtracting = true;
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