using System.Collections.Generic;
using TMPro;
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
    [SerializeField] GameObject ItemHotbar;
    [SerializeField] GameObject ReloadPrompt;
    [SerializeField] GameObject ShopUI;
    [SerializeField] GameObject UpgradeUI;


    [Header("UI Tracking")]
    [SerializeField] TextMeshProUGUI killCounterText;

    [SerializeField]
    private GameObject exposurePromptObject;

    [SerializeField]
    private float promptDuration;

    private float promptTimer = 0f;

    private bool isShowingPrompt = false;


    [Header("Weapon HUD")]
    [SerializeField]
    private WeaponHUDController weaponHUDController;


    [HideInInspector]
    public int killCount = 0;


    [Header("Checkpoint")]
    [SerializeField]
    private CheckpointManager checkpointManager;


    [Header("Player")]
    public Image playerHPBar;

    public GameObject damageFlashPanel;

    // ADDED:
    // Orange arch that appears above the crosshair
    // when the player takes normal non-exposure damage.
    public GameObject damageIndicator;


    [Header("Stamina")]
    [SerializeField]
    Image staminaBar;


    [Header("Auto Set Variables (No need to touch)")]
    public GameObject beacon;

    public GameObject player;

    public PlayerInventory playerInventory;

    public playerController playerScript;

    public cameraController cameraScript;


    [Header("Audio")]
    [SerializeField]
    private AmbiencePlaylist ambiencePlaylist;

    [SerializeField]
    private PauseMenuMusic pauseMenuMusic;


    public bool isPaused;

    public bool isExtracting;


    int enemiesRemaining = 0;

    float timeScaleOrig;

    int waveCounter;


    void Awake()
    {
        instance = this;

        timeScaleOrig =
            Time.timeScale;


        Transform ui =
            transform.parent;


        if (hud != null)
        {
            Transform hpBar =
                hud.transform.Find(
                    "Player HP Bar"
                );


            if (hpBar != null)
            {
                playerHPBar =
                    hpBar.GetComponent<Image>();

                playerHPBar.fillAmount =
                    1f;
            }
        }


        countdownText =
            ui.Find("Countdown")
                ?.gameObject;


        damageFlashPanel =
            ui.Find("FlashDamage")
                ?.gameObject;


        beacon =
            GameObject.FindWithTag(
                "Beacon"
            );


        player =
            GameObject.FindWithTag(
                "Player"
            );


        if (player != null)
        {
            playerInventory =
                player.GetComponent<PlayerInventory>();


            playerScript =
                player.GetComponent<playerController>();


            cameraScript =
                player.GetComponentInChildren<
                    cameraController
                >();
        }
        else
        {
            Debug.LogError(
                "GameManager: No GameObject found with the tag 'Player'!"
            );
        }


        if (countdownText != null)
        {
            countdownText.SetActive(false);
        }


        if (damageIndicator != null)
        {
            damageIndicator.SetActive(false);
        }


        if (ambiencePlaylist == null)
        {
            ambiencePlaylist =
                GetComponent<AmbiencePlaylist>();
        }


        if (pauseMenuMusic == null)
        {
            pauseMenuMusic =
                GetComponentInChildren<
                    PauseMenuMusic
                >(true);
        }


        if (checkpointManager == null)
        {
            checkpointManager =
                GetComponent<CheckpointManager>();
        }
    }


    void Start()
    {
        UpateKillUI();


        Debug.Log(
            "HUD: " + hud
        );


        Debug.Log(
            "HP BAR: " + playerHPBar
        );


        if (playerHPBar != null)
        {
            playerHPBar.fillAmount =
                1f;
        }


        if (ambiencePlaylist != null)
        {
            ambiencePlaylist
                .StartPlaylist();
        }


        ShowReloadPrompt(false);


        if (checkpointManager != null)
        {
            checkpointManager
                .RestoreCheckpointIfNeeded(
                    player,
                    playerInventory
                );
        }
    }


    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();


                if (pauseMenuMusic != null)
                {
                    pauseMenuMusic
                        .PlayPauseMusic();
                }


                menuActive =
                    menuPause;


                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }


        if (isShowingPrompt)
        {
            promptTimer -=
                Time.deltaTime;


            if (promptTimer <= 0f)
            {
                isShowingPrompt =
                    false;


                if (exposurePromptObject != null)
                {
                    exposurePromptObject
                        .SetActive(false);
                }
            }
        }
    }


    public void statePause()
    {
        isPaused = true;

        Time.timeScale = 0;


        if (ambiencePlaylist != null)
        {
            ambiencePlaylist
                .PausePlaylist();
        }


        cameraScript.enabled =
            false;


        hud.SetActive(false);


        Cursor.visible =
            true;


        Cursor.lockState =
            CursorLockMode.None;
    }


    public void stateUnpause()
    {
        isPaused = false;

        Time.timeScale =
            timeScaleOrig;


        if (pauseMenuMusic != null)
        {
            pauseMenuMusic
                .StopPauseMusic();
        }


        if (ambiencePlaylist != null)
        {
            ambiencePlaylist
                .ResumePlaylist();
        }


        cameraScript.enabled =
            true;


        hud.SetActive(true);


        if (menuActive != null)
        {
            menuActive.SetActive(false);

            menuActive = null;
        }


        Cursor.visible =
            false;


        Cursor.lockState =
            CursorLockMode.Locked;
    }


    public void updateGameGoal(int amount)
    {
        waveCounter +=
            amount;


        if (waveCounter <= 0)
        {
            statePause();

            menuActive =
                menuWin;

            menuActive.SetActive(true);
        }
    }


    public void StartExtraction(float time)
    {
        countdownText.SetActive(true);


        ExtractionCountdown Timer =
            countdownText
                .GetComponent<
                    ExtractionCountdown
                >();


        Timer.SetTimer(time);


        isExtracting =
            true;
    }


    public void extractionWin()
    {
        statePause();


        hud.SetActive(false);


        menuActive =
            menuWin;


        menuActive.SetActive(true);
    }


    public void youLose()
    {
        statePause();


        menuActive =
            menuLose;


        hud.SetActive(false);


        menuActive.SetActive(true);
    }


    public void ModifyEnemyCount(int ammount)
    {
        enemiesRemaining +=
            ammount;


        if (enemiesRemaining <= 0)
        {
            statePause();


            menuActive =
                menuWin;


            hud.SetActive(false);


            menuActive.SetActive(true);
        }
    }


    public void AddKill()
    {
        killCount++;

        UpateKillUI();
    }


    void UpateKillUI()
    {
        if (killCounterText != null)
        {
            killCounterText.text =
                $"Kills: {killCount}";
        }
    }


    public void UpdateWeaponInv(
        GameObject[] inv,
        int slotInUse)
    {
        if (ItemHotbar == null)
        {
            return;
        }


        InventorySlot[] slots =
            ItemHotbar
                .GetComponentsInChildren<
                    InventorySlot
                >();


        for (int i = 0;
             i < slots.Length;
             i++)
        {
            if (i >= inv.Length)
            {
                slots[i].UpdateSlot(
                    null,
                    null,
                    0,
                    Color.gray2
                );

                continue;
            }


            if (inv[i] != null)
            {
                IWeapon wep =
                    inv[i]
                        .GetComponent<IWeapon>();


                if (wep != null)
                {
                    Sprite weaponSprite =
                        null;


                    if (weaponHUDController != null)
                    {
                        weaponSprite =
                            weaponHUDController
                                .GetWeaponSprite(
                                    wep.GetWeaponName()
                                );
                    }


                    if (i == slotInUse)
                    {
                        slots[i].UpdateSlot(
                            weaponSprite,
                            wep.GetWeaponName(),
                            1,
                            Color.darkRed
                        );
                    }
                    else
                    {
                        slots[i].UpdateSlot(
                            weaponSprite,
                            wep.GetWeaponName(),
                            1,
                            Color.gray2
                        );
                    }
                }
                else
                {
                    IGadget gadget =
                        inv[i]
                            .GetComponent<IGadget>();


                    if (gadget != null)
                    {
                        if (gadget.GetItemInfo() != null)
                        {
                            if (i == slotInUse)
                            {
                                slots[i].UpdateSlot(
                                    gadget
                                        .GetItemInfo()
                                        .itemIcon,

                                    gadget
                                        .GetGadgetName(),

                                    1,

                                    Color.darkRed
                                );
                            }
                            else
                            {
                                slots[i].UpdateSlot(
                                    gadget
                                        .GetItemInfo()
                                        .itemIcon,

                                    gadget
                                        .GetGadgetName(),

                                    1,

                                    Color.gray2
                                );
                            }
                        }
                        else
                        {
                            if (i == slotInUse)
                            {
                                slots[i].UpdateSlot(
                                    null,
                                    gadget.GetGadgetName(),
                                    1,
                                    Color.darkRed
                                );
                            }
                            else
                            {
                                slots[i].UpdateSlot(
                                    null,
                                    gadget.GetGadgetName(),
                                    1,
                                    Color.gray2
                                );
                            }
                        }
                    }
                }
            }
            else
            {
                slots[i].UpdateSlot(
                    null,
                    null,
                    0,
                    Color.gray2
                );
            }
        }
    }


    public void ShowReloadPrompt(bool show)
    {
        if (ReloadPrompt != null)
        {
            ReloadPrompt.SetActive(show);
        }
    }


    public void SaveCheckpoint(
        Transform respawnPoint)
    {
        if (checkpointManager != null)
        {
            checkpointManager
                .SaveCheckpoint(
                    respawnPoint,
                    playerInventory
                );
        }
    }


    public void LoadCheckpoint()
    {
        if (checkpointManager != null)
        {
            checkpointManager
                .LoadCheckpoint();
        }
    }


    public void ShowExposurePrompt()
    {
        if (exposurePromptObject != null)
        {
            exposurePromptObject
                .SetActive(true);


            promptTimer =
                promptDuration;


            isShowingPrompt =
                true;
        }
    }


    public void ShowShopUI(
        bool show,
        List<ShopItem> items,
        Vector3 _spawnLocation)
    {
        if (show)
        {
            statePause();
        }
        else
        {
            stateUnpause();
        }


        ShopUI.SetActive(show);


        ShopUI
            .GetComponent<ShopUI>()
            .SetStation(
                player.GetComponent<IPlayer>(),
                items,
                _spawnLocation
            );
    }


    public void ShowUpgradeUI(bool show)
    {
        if (show)
        {
            statePause();
        }
        else
        {
            stateUnpause();
        }


        UpgradeUI.SetActive(show);
    }


    public void UpdateStaminaBar(
        float ammount,
        bool show)
    {
        staminaBar.fillAmount =
            ammount;
    }
}