using System.Collections;
using UnityEngine;

public class playerController : MonoBehaviour, IPlayer, IDamage
{
    [SerializeField] CharacterController controller;

    [Header("Player Stats:")]
    [SerializeField][Min(1f)] float MaxHP = 100;
    [SerializeField] float BaseSpeed = 5;
    [SerializeField] float MaxSpeed = 15;
    [Range(8, 15)][SerializeField] int jumpSpeed = 10;
    [Range(1, 3)][SerializeField] int jumpMax = 2;
    [Range(15, 45)][SerializeField] int gravity = 35;

    PlayerUpgrades upgradeManager;


    [Header("Inventory")]
    [SerializeField]
    PlayerInventory Inventory;


    [Header("Audio")]
    public float walkHearingRadius = 5f;
    public float sprintHearingRadius = 10f;


    [Header("Weapon")]
    [SerializeField]
    ProjectileManager projectileManager;

    [SerializeField]
    GameObject WeaponGrip;


    GameObject ActiveItem;

    GameObject[] hotbar =
        new GameObject[4];


    int activeItemSlot =
        -1;


    public static System.Action<bool>
        ShowAmmoUI;


    int jumpCount;

    float currentHP;

    float currentSpeed;

    int points = 12345;


    Vector3 moveDir;

    Vector3 playerVel;


    bool isDead = false;


    bool stimulantMode =
        false;

    float stimulantMultiplier =
        1f;


    void Start()
    {
        currentHP =
            MaxHP;


        currentSpeed =
            BaseSpeed;


        upgradeManager =
            GetComponent<PlayerUpgrades>();


        if (upgradeManager != null)
        {
            upgradeManager
                .ApplyUpgrades();
        }


        UpdateWeaponUI();
    }


    void Update()
    {
        if (Input.GetKeyDown(
            KeyCode.U))
        {
            if (upgradeManager != null)
            {
                upgradeManager
                    .ModifyHPUpgrades(1);

                updatePlayerUI();
            }
        }
        else if (Input.GetKeyDown(
            KeyCode.I))
        {
            if (upgradeManager != null)
            {
                upgradeManager
                    .ModifySpeedUpgrades(1);
            }
        }
        else if (Input.GetKeyDown(
            KeyCode.O))
        {
            if (upgradeManager != null)
            {
                upgradeManager
                    .ModifyStaminaUpgrades(1);
            }
        }


        sprint();


        if (gameManager.instance != null &&
            gameManager.instance.isPaused)
        {
            return;
        }


        if (isDead)
        {
            return;
        }


        GadgetUse();

        CheckSwapItem();

        movement();


        if (Input.GetKey(
            KeyCode.LeftShift))
        {
            NoiseManager.MakeNoise(
                transform.position,
                sprintHearingRadius
            );
        }


        if (Input.GetKeyDown(
            KeyCode.K))
        {
            takeDamage(1);
        }
    }


    void movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;

            playerVel.y = 0;
        }


        moveDir =
            Input.GetAxis("Horizontal") *
            transform.right +

            Input.GetAxis("Vertical") *
            transform.forward;


        controller.Move(
            moveDir *
            currentSpeed *
            Time.deltaTime
        );


        jump();


        controller.Move(
            playerVel *
            Time.deltaTime
        );


        playerVel.y -=
            gravity *
            Time.deltaTime;
    }


    void sprint()
    {
        float targetSpeed;


        if (Input.GetKey(
            KeyCode.LeftShift))
        {
            targetSpeed =
                MaxSpeed;
        }
        else
        {
            targetSpeed =
                BaseSpeed;
        }


        if (stimulantMode)
        {
            targetSpeed *=
                stimulantMultiplier;
        }


        currentSpeed =
            targetSpeed;
    }


    void jump()
    {
        if (Input.GetButtonDown(
                "Jump") &&
            jumpCount < jumpMax)
        {
            jumpCount++;

            playerVel.y =
                jumpSpeed;
        }
    }


    public void takeDamage(int amount)
    {
        takeDamage(
            amount,
            true
        );
    }


    public void takeDamage(
        int amount,
        bool showFlash = true)
    {
        currentHP -=
            amount;


        updatePlayerUI();


        // Normal physical/enemy damage triggers this.
        // Exposure-style damage can pass false.
        if (showFlash)
        {
            StartCoroutine(
                flashDamage()
            );
        }


        if (currentHP <= 0)
        {
            if (gameManager.instance != null)
            {
                gameManager.instance
                    .youLose();
            }
        }
    }


    IEnumerator flashDamage()
    {
        if (gameManager.instance != null)
        {
            if (gameManager.instance
                .damageFlashPanel != null)
            {
                gameManager.instance
                    .damageFlashPanel
                    .SetActive(true);
            }


            // ADDED:
            // Shows orange damage arc above crosshair.
            if (gameManager.instance
                .damageIndicator != null)
            {
                gameManager.instance
                    .damageIndicator
                    .SetActive(true);
            }
        }


        yield return new WaitForSeconds(
            0.15f
        );


        if (gameManager.instance != null)
        {
            if (gameManager.instance
                .damageFlashPanel != null)
            {
                gameManager.instance
                    .damageFlashPanel
                    .SetActive(false);
            }


            if (gameManager.instance
                .damageIndicator != null)
            {
                gameManager.instance
                    .damageIndicator
                    .SetActive(false);
            }
        }
    }


    public void updatePlayerUI()
    {
        if (gameManager.instance != null &&
            gameManager.instance
                .playerHPBar != null)
        {
            gameManager.instance
                .playerHPBar
                .fillAmount =
                    currentHP /
                    MaxHP;
        }
    }


    public void PlayerAddItem(
        ScriptableItem itemName,
        int amount)
    {
        if (Inventory != null)
        {
            Inventory.AddItem(
                itemName,
                amount
            );
        }
    }


    public bool PlayerRefillAmmo(
        int amount)
    {
        if (ActiveItem == null)
        {
            return false;
        }


        IWeapon wep =
            ActiveItem
                .GetComponent<IWeapon>();


        if (wep != null)
        {
            return wep
                .WeaponRefillAmmo(
                    amount
                );
        }


        return false;
    }


    public void PlayerAddItem(
        GameObject Item)
    {
        if (Item == null)
        {
            return;
        }


        if (WeaponGrip == null)
        {
            Debug.LogError(
                "playerController: WeaponGrip is not assigned."
            );

            return;
        }


        int arrayStart = -1;

        int arrayEnd = -1;


        IWeapon wep =
            Item.GetComponent<IWeapon>();


        IGadget gadget =
            Item.GetComponent<IGadget>();


        if (wep != null)
        {
            arrayStart = 0;
            arrayEnd = 1;
        }
        else if (gadget != null)
        {
            arrayStart = 2;
            arrayEnd = 3;
        }
        else
        {
            Debug.LogWarning(
                "playerController: Item does not implement IWeapon or IGadget."
            );

            return;
        }


        bool hadEmpty =
            false;


        for (int i = arrayStart;
             i <= arrayEnd;
             i++)
        {
            if (hotbar[i] == null)
            {
                hadEmpty =
                    true;


                if (ActiveItem != null)
                {
                    ActiveItem
                        .SetActive(false);
                }


                hotbar[i] =
                    Item;


                ActiveItem =
                    Item;


                activeItemSlot =
                    i;


                break;
            }
        }


        if (!hadEmpty)
        {
            int slotToUse =
                activeItemSlot;


            if (slotToUse < arrayStart ||
                slotToUse > arrayEnd)
            {
                slotToUse =
                    arrayStart;
            }


            if (ActiveItem != null)
            {
                DropWeapon();
            }


            hotbar[slotToUse] =
                Item;


            ActiveItem =
                Item;


            activeItemSlot =
                slotToUse;
        }


        Item.transform.SetParent(
            WeaponGrip.transform,
            false
        );


        Item.transform.localPosition =
            Vector3.zero;


        Item.transform.localRotation =
            Quaternion.identity;


        if (wep != null)
        {
            Camera playerCamera =
                Camera.main;


            if (playerCamera != null)
            {
                wep.SetPlayerVariables(
                    GetComponent<IPlayer>(),
                    playerCamera
                        .GetComponent<ICamera>(),
                    projectileManager,
                    Vector3.zero
                );
            }


            wep.SetWeaponUse(
                true
            );
        }


        ActiveItem.SetActive(
            false
        );


        ActiveItem.SetActive(
            true
        );


        UpdateWeaponUI();
    }


    void DropWeapon()
    {
        if (ActiveItem == null)
        {
            return;
        }


        IWeapon wep =
            ActiveItem
                .GetComponent<IWeapon>();


        if (wep != null)
        {
            wep.SetWeaponUse(
                false
            );


            wep.SetPlayerVariables();
        }


        RaycastHit frontRay;

        RaycastHit downRay;


        Vector3 traceStart =
            transform.position;


        Vector3 traceEnd =
            traceStart +
            transform.forward *
            3;


        Vector3 dropLocation;


        if (Physics.Linecast(
            traceStart,
            traceEnd,
            out frontRay))
        {
            traceStart =
                frontRay.point;
        }
        else
        {
            traceStart =
                traceEnd;
        }


        traceEnd =
            traceStart +
            Vector3.down *
            100;


        if (Physics.Linecast(
            traceStart,
            traceEnd,
            out downRay))
        {
            dropLocation =
                downRay.point;
        }
        else
        {
            dropLocation =
                traceEnd;
        }


        ActiveItem.transform
            .SetParent(
                null,
                true
            );


        ActiveItem.transform.position =
            dropLocation;


        ActiveItem.transform.rotation =
            Quaternion.Euler(
                0,
                transform.eulerAngles.y,
                0
            );


        Collider itemCollider =
            ActiveItem
                .GetComponent<Collider>();


        if (itemCollider != null)
        {
            itemCollider.enabled =
                true;


            float posOffset =
                itemCollider
                    .bounds
                    .extents
                    .y;


            ActiveItem.transform.position +=
                new Vector3(
                    0,
                    posOffset,
                    0
                );
        }


        if (activeItemSlot >= 0 &&
            activeItemSlot <
            hotbar.Length)
        {
            hotbar[
                activeItemSlot
            ] = null;
        }


        ActiveItem =
            null;


        activeItemSlot =
            -1;


        UpdateWeaponUI();
    }


    void CheckSwapItem()
    {
        if (Input.GetKeyDown(
            KeyCode.Alpha1))
        {
            SwapItem(0);
        }
        else if (Input.GetKeyDown(
            KeyCode.Alpha2))
        {
            SwapItem(1);
        }
        else if (Input.GetKeyDown(
            KeyCode.Alpha3))
        {
            SwapItem(2);
        }
        else if (Input.GetKeyDown(
            KeyCode.Alpha4))
        {
            SwapItem(3);
        }
        else if (Input.GetKeyDown(
            KeyCode.Backspace))
        {
            DropWeapon();
        }
    }


    void SwapItem(int index)
    {
        if (index < 0 ||
            index >= hotbar.Length)
        {
            return;
        }


        if (hotbar[index] == null ||
            activeItemSlot == index)
        {
            return;
        }


        if (ActiveItem != null)
        {
            ActiveItem.SetActive(
                false
            );
        }


        ActiveItem =
            hotbar[index];


        activeItemSlot =
            index;


        ActiveItem.SetActive(
            true
        );


        UpdateWeaponUI();
    }


    void GadgetUse()
    {
        if (!Input.GetKeyDown(
            KeyCode.Mouse0))
        {
            return;
        }


        if (ActiveItem == null)
        {
            return;
        }


        IGadget gadget =
            ActiveItem
                .GetComponent<IGadget>();


        if (gadget == null)
        {
            return;
        }


        if (gadget.UseGadget(
            gameObject))
        {
            ActiveItem.transform
                .SetParent(
                    null
                );


            ActiveItem =
                null;


            if (activeItemSlot >= 0 &&
                activeItemSlot <
                hotbar.Length)
            {
                hotbar[
                    activeItemSlot
                ] = null;
            }


            activeItemSlot =
                -1;


            UpdateWeaponUI();
        }
    }


    void UpdateWeaponUI()
    {
        if (ActiveItem != null)
        {
            IWeapon wep =
                ActiveItem
                    .GetComponent<IWeapon>();


            if (wep != null)
            {
                ShowAmmoUI
                    ?.Invoke(true);
            }
            else
            {
                IGadget gadget =
                    ActiveItem
                        .GetComponent<IGadget>();


                if (gadget != null)
                {
                    ShowAmmoUI
                        ?.Invoke(false);
                }
            }
        }
        else
        {
            activeItemSlot =
                -1;


            if (gameManager.instance != null)
            {
                gameManager.instance
                    .ShowReloadPrompt(
                        false
                    );
            }


            ShowAmmoUI
                ?.Invoke(false);
        }


        if (gameManager.instance != null)
        {
            gameManager.instance
                .UpdateWeaponInv(
                    hotbar,
                    activeItemSlot
                );
        }
    }


    public GameObject[]
        GetPlayerHotbar()
    {
        return
            (GameObject[])
            hotbar.Clone();
    }


    public GameObject[]
        GetWeaponsForCheckpoint()
    {
        return
            (GameObject[])
            hotbar.Clone();
    }


    public string
        GetActiveItemNameForCheckpoint()
    {
        if (ActiveItem == null)
        {
            return
                string.Empty;
        }


        IWeapon weapon =
            ActiveItem
                .GetComponent<IWeapon>();


        if (weapon != null)
        {
            return weapon
                .GetWeaponName();
        }


        return string.Empty;
    }


    public bool
        RestoreWeaponSlotForCheckpoint(
            GameObject weaponObject,
            int slot)
    {
        if (weaponObject == null ||
            slot < 0 ||
            slot >= hotbar.Length)
        {
            return false;
        }


        IWeapon weapon =
            weaponObject
                .GetComponent<IWeapon>();


        Camera playerCamera =
            Camera.main;


        if (weapon == null ||
            playerCamera == null ||
            WeaponGrip == null)
        {
            return false;
        }


        hotbar[slot] =
            weaponObject;


        weaponObject.transform
            .SetParent(
                WeaponGrip.transform,
                false
            );


        weaponObject.transform
            .localPosition =
                Vector3.zero;


        weaponObject.transform
            .localRotation =
                Quaternion.identity;


        weapon.SetPlayerVariables(
            GetComponent<IPlayer>(),
            playerCamera
                .GetComponent<ICamera>(),
            projectileManager,
            Vector3.zero
        );


        weapon.SetWeaponUse(
            true
        );


        weaponObject.SetActive(
            false
        );


        return true;
    }


    public void
        EquipWeaponForCheckpoint(
            string weaponName)
    {
        if (string.IsNullOrEmpty(
            weaponName))
        {
            return;
        }


        for (int slot = 0;
             slot < hotbar.Length;
             slot++)
        {
            if (hotbar[slot] != null)
            {
                hotbar[slot]
                    .SetActive(false);
            }
        }


        for (int slot = 0;
             slot < hotbar.Length;
             slot++)
        {
            if (hotbar[slot] == null)
            {
                continue;
            }


            IWeapon weapon =
                hotbar[slot]
                    .GetComponent<IWeapon>();


            if (weapon == null ||
                weapon.GetWeaponName() !=
                weaponName)
            {
                continue;
            }


            if (ActiveItem != null)
            {
                ActiveItem
                    .SetActive(false);
            }


            ActiveItem =
                hotbar[slot];


            activeItemSlot =
                slot;


            ActiveItem.SetActive(
                true
            );


            ShowAmmoUI
                ?.Invoke(true);


            UpdateWeaponUI();


            return;
        }
    }


    public void
        RefreshWeaponUIForCheckpoint()
    {
        ShowAmmoUI
            ?.Invoke(
                ActiveItem != null
            );


        UpdateWeaponUI();
    }


    public ProjectileManager
        GetProjectileManager()
    {
        return
            projectileManager;
    }


    public bool HealPlayer(
        int amount,
        bool overHeal = false)
    {
        if (currentHP < MaxHP &&
            !overHeal)
        {
            currentHP +=
                Mathf.Clamp(
                    amount,
                    0,
                    MaxHP -
                    currentHP
                );


            updatePlayerUI();


            return true;
        }
        else if (overHeal)
        {
            currentHP +=
                amount;


            updatePlayerUI();


            return true;
        }


        return false;
    }


    public void SetMaxJumps(
        int amount)
    {
        jumpMax =
            Mathf.Clamp(
                amount,
                1,
                3
            );
    }


    public void SetStimulantMode(
        bool enabled,
        float speedMultiplier)
    {
        stimulantMode =
            enabled;


        if (enabled)
        {
            stimulantMultiplier =
                Mathf.Max(
                    1f,
                    speedMultiplier
                );
        }
        else
        {
            stimulantMultiplier =
                1f;
        }
    }


    public float GetCurrentHP()
    {
        return currentHP;
    }


    public void SetCurrentHP(
        float ammount)
    {
        currentHP =
            ammount;
    }


    public float GetMaxHP()
    {
        return MaxHP;
    }


    public void SetMaxHP(
        float ammount)
    {
        MaxHP =
            ammount;
    }


    public float GetBaseSpeed()
    {
        return BaseSpeed;
    }


    public void SetBaseSpeed(
        float speed)
    {
        BaseSpeed =
            speed;
    }


    public float GetMaxSpeed()
    {
        return MaxSpeed;
    }


    public void SetMaxSpeed(
        float speed)
    {
        MaxSpeed =
            speed;
    }


    public float GetMaxStamina()
    {
        return 1;
    }


    public void SetMaxStamina(
        float ammount)
    {
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public void SetStimulantMode(bool active, float speedMult)
    {
        isStimed = active;

        stimMult = speedMult;

        stimMult = Mathf.Clamp(stimMult, 0, float.MaxValue);

        if (!active)
        {
            currentSpeed = isSprinting ? MaxSpeed : BaseSpeed;
        }
        else
        {
            currentSpeed *= stimMult;
        }
    }

    public void SetMaxJumps(int jumps)
    {
        jumpMax = jumps;
    }

    public int GetPlayerFunds()
    {
        return points;
    }


    public void ModifyPlayerFunds(
        int ammount)
    {
        points +=
            ammount;
    }
}