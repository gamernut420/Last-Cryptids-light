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
    public GameObject beacon;
    public GameObject player;
    public PlayerInventory playerInventory;
    public playerController playerScript;
    public cameraController cameraScript;
    [Header("Audio")]
    [SerializeField] private AmbiencePlaylist ambiencePlaylist;

    public bool isPaused;
    public bool isExtracting;

    int enemiesRemaining = 0;

    float timeScaleOrig;
    

    int waveCounter;


    // Sets up references
    void Awake()
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        Transform ui = transform.parent;

        menuPause = ui.Find("PauseMenu")?.gameObject;
        menuWin = ui.Find("Win Menu")?.gameObject;
        menuLose = ui.Find("GameOverPanel")?.gameObject;
        menuExtractionWin = ui.Find("Win Menu EX")?.gameObject;

        hud = ui.Find("Hud")?.gameObject;

        if (hud != null)
        {
            Transform hpBar = hud.transform.Find("Player HP Bar");

            if (hpBar != null)
            {
                playerHPBar = hpBar.GetComponent<Image>();
                playerHPBar.fillAmount = 1f;
            }
        }

        countdownText = ui.Find("Countdown")?.gameObject;
        damageFlashPanel = ui.Find("FlashDamage")?.gameObject;
        
        beacon = GameObject.FindWithTag("Beacon");
        player = GameObject.FindWithTag("Player");

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

        if (countdownText != null)
        {
            countdownText.SetActive(false);
        }

        if (ambiencePlaylist == null)
        {
            ambiencePlaylist = GetComponent<AmbiencePlaylist>();
        }
    }


    void Start()
    {
        Debug.Log("HUD: " + hud);
        Debug.Log("HP BAR: " + playerHPBar);

        if (playerHPBar != null)
        {
            playerHPBar.fillAmount = 1f;
        }

        if (ambiencePlaylist != null)
        {
            ambiencePlaylist.StartPlaylist();
        }
    }



    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                // stateUnpause(); //Removed to move down, trust - Sean
                // pause the game
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

        if (ambiencePlaylist != null)
        {
            ambiencePlaylist.PausePlaylist();
        }

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

        if (ambiencePlaylist != null)
        {
            ambiencePlaylist.ResumePlaylist();
        }

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