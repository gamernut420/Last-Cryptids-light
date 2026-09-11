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
    [SerializeField] PlayerInventory Inventory;

    [Header("Audio")]
    public float walkHearingRadius = 5f;
    public float sprintHearingRadius = 10f;

    [Header("Weapon")]
    [SerializeField] ProjectileManager projectileManager;
    [SerializeField] GameObject WeaponGrip;
    GameObject ActiveItem;
    GameObject[] hotbar = new GameObject[4];
    int activeItemSlot;
    public static System.Action<bool> ShowAmmoUI;

    int jumpCount;
    float currentHP;
    float currentSpeed;

    //Set for testing this will be used alongside kills
    int points = 12345;

    Vector3 moveDir;
    Vector3 playerVel;


    bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = MaxHP;
        currentSpeed = BaseSpeed;

        upgradeManager = GetComponent<PlayerUpgrades>();

        upgradeManager.ApplyUpgrades();

        UpdateWeaponUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            upgradeManager.ModifyHPUpgrades(1);

            updatePlayerUI();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            upgradeManager.ModifySpeedUpgrades(1);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            upgradeManager.ModifyStaminaUpgrades(1);
        }

        sprint();

        if (gameManager.instance.isPaused) return;

        if (isDead) return;

        GadgetUse();

        CheckSwapItem();
 
        movement();
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            NoiseManager.MakeNoise(transform.position, sprintHearingRadius);
        }
        ///////   Testing Logic    ///////

        // Testing key: 'K' to instantly kill the playerand test the death screen
        if (Input.GetKeyDown(KeyCode.K)) takeDamage(1);
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = 0;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
    }

    void sprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentSpeed = MaxSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            currentSpeed = BaseSpeed;
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }
    public void takeDamage(int amount)
    {
        takeDamage(amount, true);
    }

    public void takeDamage(int amount, bool showFlash = true)
    {
        currentHP -= amount;
        updatePlayerUI();
        if(showFlash) 
            StartCoroutine(flashDamage());

        if (currentHP <= 0)
        {
            // you i'm dead!!!
            gameManager.instance.youLose();
        }
    }

    IEnumerator flashDamage()
    {
        gameManager.instance.damageFlashPanel.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.damageFlashPanel.SetActive(false);
    }

    public void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)currentHP / MaxHP;
    }

    public void PlayerAddItem(ScriptableItem itemName, int amount)
    {
        Inventory.AddItem(itemName, amount);
    }

    public bool PlayerRefillAmmo(int amount)
    {
        if(ActiveItem != null)
        {
            IWeapon wep = ActiveItem.GetComponent<IWeapon>();

            if (wep != null)
            {
                return wep.WeaponRefillAmmo(amount);
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public void PlayerAddItem(GameObject Item)
    {
        int arrayStart = -1;
        int arrayEnd = -1;

        IWeapon wep = Item.GetComponent<IWeapon>();
        IGadget gadget = Item.GetComponent<IGadget>();

        if (wep != null)
        {
            wep.SetPlayerVariables(
                GetComponent<IPlayer>(),
                Camera.main.GetComponent<ICamera>(),
                projectileManager,
                WeaponGrip.transform.localPosition);

            wep.SetWeaponUse(true);

            arrayStart = 0;
            arrayEnd = 1;
        }
        else if (gadget != null)
        {
            arrayStart = 2;
            arrayEnd = 3;
        }

        bool hadEmpty = false;

        for (int i = arrayStart; i <= arrayEnd; i++)
        {
            if (hotbar[i] == null)
            {
                hadEmpty = true;

                if (ActiveItem != null)
                {
                    ActiveItem.SetActive(false);
                }

                hotbar[i] = Item;

                SwapItem(i);

                break;
            }
        }

        if (hadEmpty == false)
        {
            int slotToUse = activeItemSlot;
            
            DropWeapon();

            hotbar[slotToUse] = Item;

            ActiveItem = hotbar[slotToUse];

            activeItemSlot = slotToUse;
        }

        Item.transform.SetParent(Camera.main.transform);

        Item.transform.localPosition = WeaponGrip.transform.localPosition;

        Item.transform.localRotation = Quaternion.identity;

        ActiveItem.SetActive(false);
        ActiveItem.SetActive(true);

        UpdateWeaponUI();
    }

    void DropWeapon()
    {
        if (ActiveItem != null)
        {
            IWeapon wep = ActiveItem.GetComponent<IWeapon>();

            if (wep != null)
            {
                wep.SetWeaponUse(false);
                wep.SetPlayerVariables();
            }

            RaycastHit frontRay;
            RaycastHit downRay;

            Vector3 traceStart = transform.position;
            Vector3 traceEnd = traceStart + (transform.forward * 3);

            Vector3 dropLocation;

            if (Physics.Linecast(traceStart, traceEnd, out frontRay))
            {
                traceStart = frontRay.point;
            }
            else
            {
                traceStart = traceEnd;

            }

            traceEnd = traceStart + (Vector3.down * 100);

            if (Physics.Linecast(traceStart, traceEnd, out downRay))
            {
                dropLocation = downRay.point;
            }
            else
            {
                dropLocation = traceEnd;
            }

            ActiveItem.transform.SetParent(null);
            ActiveItem.transform.position = dropLocation;
            ActiveItem.transform.localRotation = Quaternion.Euler(0, ActiveItem.transform.localEulerAngles.y, 0);

            ActiveItem.GetComponent<Collider>().enabled = true;

            float posOffset = ActiveItem.GetComponent<Collider>().bounds.extents.y;

            ActiveItem.transform.position += new Vector3(0, posOffset, 0);

            ActiveItem = null;
            hotbar[activeItemSlot] = null;
            activeItemSlot = -1;

            UpdateWeaponUI();
        }
    }

    void CheckSwapItem()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwapItem(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwapItem(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwapItem(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwapItem(3);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            DropWeapon();
        }
    }

    void SwapItem(int index)
    {
        if (hotbar[index] != null && activeItemSlot != index)
        {
            if (ActiveItem != null)
            {
                ActiveItem.SetActive(false);
            }

            ActiveItem = hotbar[index];

            activeItemSlot = index;

            ActiveItem.SetActive(true);

            UpdateWeaponUI();
        }
    }

    void GadgetUse()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (ActiveItem != null)
            {
                IGadget gadget = ActiveItem.GetComponent<IGadget>();

                if (gadget != null)
                {
                    if (gadget.UseGadget(this))
                    {
                        ActiveItem.transform.SetParent(null);
                        ActiveItem = null;
                        hotbar[activeItemSlot] = null;

                        UpdateWeaponUI();
                    }
                }
            }
        }
    }

    void UpdateWeaponUI()
    {
        if (ActiveItem != null)
        {
            IWeapon wep = ActiveItem.GetComponent<IWeapon>();

            if (wep != null)
            {
                gameManager.instance.UpdateActiveWep(wep.GetWeaponName());

                ShowAmmoUI?.Invoke(true);
            }
            else
            {
                IGadget gadget = ActiveItem.GetComponent<IGadget>();

                if (gadget != null)
                {
                    gameManager.instance.UpdateActiveWep(gadget.GetGadgetName());

                    ShowAmmoUI?.Invoke(false);
                }
            }
        }
        else
        {
            activeItemSlot = -1;
            gameManager.instance.ShowReloadPrompt(false);
            gameManager.instance.UpdateActiveWep("None");
            ShowAmmoUI?.Invoke(false);
        }

        gameManager.instance.UpdateWeaponInv(hotbar, activeItemSlot);
    }

    public GameObject[] GetWeaponsForCheckpoint()
    {
        return (GameObject[])hotbar.Clone();
    }

    public string GetActiveItemNameForCheckpoint()
    {
        if (ActiveItem == null) return string.Empty;

        IWeapon weapon = ActiveItem.GetComponent<IWeapon>();
        return weapon != null ? weapon.GetWeaponName() : string.Empty;
    }

    public bool RestoreWeaponSlotForCheckpoint(GameObject weaponObject, int slot)
    {
        if (weaponObject == null || slot < 0 || slot >= hotbar.Length)
            return false;
        IWeapon weapon = weaponObject.GetComponent<IWeapon>();
        Camera playerCamera = Camera.main;
        if (weapon == null || playerCamera == null || WeaponGrip == null)
            return false;
        hotbar[slot] = weaponObject;

        weapon.SetPlayerVariables(
            GetComponent<IPlayer>(),
            playerCamera.GetComponent<ICamera>(),
            projectileManager,
            WeaponGrip.transform.localPosition
        );

        weapon.SetWeaponUse(true);
        weaponObject.transform.SetParent(playerCamera.transform);
        weaponObject.transform.localPosition = WeaponGrip.transform.localPosition;
        weaponObject.transform.localRotation = Quaternion.identity;
        weaponObject.SetActive(false);
        return true;
    }

    public void EquipWeaponForCheckpoint(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName)) return;

        for (int slot = 0; slot < hotbar.Length; slot++)
        {
            if (hotbar[slot] != null) 
            {
                hotbar[slot].SetActive(false);
            }
        }
        for (int slot = 0; slot < hotbar.Length; slot++ ) 
        {
            if (hotbar[slot] == null) continue;
            IWeapon weapon = hotbar[slot].GetComponent<IWeapon>();

            if (weapon == null || weapon.GetWeaponName() != weaponName)
                continue;

            if (ActiveItem != null)
            {
                ActiveItem.SetActive(false);
            }

            ActiveItem = hotbar[slot];
            activeItemSlot = slot;
            ActiveItem.SetActive(true);

            ShowAmmoUI?.Invoke(true);
            UpdateWeaponUI();
            return;
        }
    }

    public void RefreshWeaponUIForCheckpoint()
    {
        ShowAmmoUI?.Invoke(ActiveItem != null);
        UpdateWeaponUI();
    }

    public ProjectileManager GetProjectileManager()
    {
        return projectileManager;
    }

    public bool HealPlayer(int amount, bool overHeal = false)
    {
        if(currentHP < MaxHP && !overHeal)
        {
            currentHP += Mathf.Clamp(amount, 0, MaxHP - currentHP);

            updatePlayerUI();

            return true;
        }
        else if (overHeal)
        {
            currentHP += amount;

            updatePlayerUI();

            return true;
        }

        return false;
    }

    //HP getters and setters
    public float GetCurrentHP()
    {
        return (float)currentHP;
    }

    public void SetCurrentHP(float ammount)
    {
        currentHP = (int)ammount;
    }

    public float GetMaxHP()
    {
        return (float)MaxHP;
    }

    public void SetMaxHP(float ammount)
    {
        MaxHP = (int)ammount;
    }

    //Speed getters and setters
    public float GetBaseSpeed()
    {
        return BaseSpeed;
    }

    public void SetBaseSpeed(float speed)
    {
        BaseSpeed = speed;
    }

    public float GetMaxSpeed()
    {
        return MaxSpeed;
    }

    public void SetMaxSpeed(float speed)
    {
        MaxSpeed = speed;
    }

    //Stamina getters and setters
    public float GetMaxStamina()
    {
        return 1;
    }

    public void SetMaxStamina(float ammount)
    {
        //set max stamina
    }

    public int GetPlayerFunds()
    {
        return points;
    }

    public void ModifyPlayerFunds(int ammount)
    {
        points += ammount;
    }
}
