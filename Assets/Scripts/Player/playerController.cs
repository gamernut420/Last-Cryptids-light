using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playerController : MonoBehaviour, IPlayer, IDamage
{
    [SerializeField] CharacterController controller;

    [Header("Player Stats:")]
    [Range(1, 10)][SerializeField] int Hp;
    [Range(1f, 10f)][SerializeField] float speed;
    [Range(2f, 5f)][SerializeField] float sprintMod;
    [Range(8, 15)][SerializeField] int jumpSpeed;
    [Range(1, 3)][SerializeField] int jumpMax;
    [Range(15, 45)][SerializeField] int gravity;

    [Header("Inventory")]
    [SerializeField] PlayerInventory Inventory;

    [Header("Audio")]
    public float walkHearingRadius = 5f;
    public float sprintHearingRadius = 10f;

    [Header("Weapon")]
    [SerializeField] ProjectileManager projectileManager;
    [SerializeField] GameObject WeaponGrip;
    GameObject ActiveWeapon;
    GameObject[] hotbar = new GameObject[4];
    int activeItemSlot;
    public static System.Action<bool> ShowAmmoUI;

    int jumpCount;
    int HPOrig;
    float speedOrig;

    Vector3 moveDir;
    Vector3 playerVel;


    bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = Hp;
        speedOrig = speed;

        UpdateWeaponUI();

        ShowAmmoUI?.Invoke(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.instance.isPaused) return;

        if (isDead) return;

        GadgetUse();

        CheckSwapItem();
 
        movement();
        sprint();
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
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
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
        Hp -= amount;
        updatePlayerUI();
        StartCoroutine(flashDamage());
        if (Hp <= 0)
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
        gameManager.instance.playerHPBar.fillAmount = (float)Hp / HPOrig;
    }

    public void PlayerAddItem(string itemName, int amount)
    {
        Inventory.AddItem(itemName, amount);
    }

    public bool PlayerRefillAmmo(int amount)
    {
        if(ActiveWeapon != null)
        {
            IWeapon wep = ActiveWeapon.GetComponent<IWeapon>();

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

            ShowAmmoUI?.Invoke(ActiveWeapon != null);
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

                if (ActiveWeapon != null)
                {
                    ActiveWeapon.SetActive(false);
                }

                hotbar[i] = Item;

                ActiveWeapon = hotbar[i];

                activeItemSlot = i;

                break;
            }
        }

        if (hadEmpty == false)
        {
            DropWeapon();

            hotbar[activeItemSlot] = Item;

            ActiveWeapon = hotbar[activeItemSlot];
        }

        Item.transform.SetParent(Camera.main.transform);

        Item.transform.localPosition = WeaponGrip.transform.localPosition;

        Item.transform.localRotation = Quaternion.identity;

        ActiveWeapon.SetActive(false);
        ActiveWeapon.SetActive(true);

        UpdateWeaponUI();
    }

    void DropWeapon()
    {
        if (ActiveWeapon != null)
        {
            IWeapon wep = ActiveWeapon.GetComponent<IWeapon>();

            if (wep != null)
            {
                wep.SetWeaponUse(false);
                wep.SetPlayerVariables();
            }

            ShowAmmoUI?.Invoke(false);

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

            ActiveWeapon.transform.SetParent(null);
            ActiveWeapon.transform.position = dropLocation;
            ActiveWeapon.transform.localRotation = Quaternion.Euler(0, ActiveWeapon.transform.localEulerAngles.y, 0);

            ActiveWeapon = null;
            hotbar[activeItemSlot] = null;

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
            if (ActiveWeapon != null)
            {
                ActiveWeapon.SetActive(false);
            }

            ActiveWeapon = hotbar[index];

            activeItemSlot = index;

            ActiveWeapon.SetActive(true);

            ShowAmmoUI?.Invoke(ActiveWeapon != null);

            // weapon UI
            UpdateWeaponUI();
        }
    }

    void GadgetUse()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (ActiveWeapon != null)
            {
                IGadget gadget = ActiveWeapon.GetComponent<IGadget>();

                if (gadget != null)
                {
                    if (gadget.UseGadget(this))
                    {
                        ActiveWeapon.transform.SetParent(null);
                        ActiveWeapon = null;
                        hotbar[activeItemSlot] = null;

                        UpdateWeaponUI();
                    }
                }
            }
        }
    }

    void UpdateWeaponUI()
    {
        gameManager.instance.UpdateWeaponInv(hotbar);

        if (ActiveWeapon != null)
        {
            IWeapon wep = ActiveWeapon.GetComponent<IWeapon>();

            if (wep != null)
            {
                gameManager.instance.UpdateActiveWep(wep.GetWeaponName());
            }
            else
            {
                IGadget gadget = ActiveWeapon.GetComponent<IGadget>();

                if (gadget != null)
                {
                    gameManager.instance.UpdateActiveWep(gadget.GetGadgetName());
                }
            }
        }
        else
        {
            gameManager.instance.ShowReloadPrompt(false);
            gameManager.instance.UpdateActiveWep("None");
        }
    }

    public GameObject[] GetWeaponsForCheckpoint()
    {
        return (GameObject[])hotbar.Clone();
    }

    public string GetActiveWeaponNameForCheckpoint()
    {
        if (ActiveWeapon == null) return string.Empty;

        IWeapon weapon = ActiveWeapon.GetComponent<IWeapon>();
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

            if (ActiveWeapon != null)
            {
                ActiveWeapon.SetActive(false);
            }

            ActiveWeapon = hotbar[slot];
            activeItemSlot = slot;
            ActiveWeapon.SetActive(true);

            ShowAmmoUI?.Invoke(true);
            UpdateWeaponUI();
            return;
        }
    }

    public void RefreshWeaponUIForCheckpoint()
    {
        ShowAmmoUI?.Invoke(ActiveWeapon != null);
        UpdateWeaponUI();
    }

    public ProjectileManager GetProjectileManager()
    {
        return projectileManager;
    }

    public bool HealPlayer(int amount, bool overHeal = false)
    {
        if(Hp < HPOrig && !overHeal)
        {
            Hp += Mathf.Clamp(amount, 0, HPOrig - Hp);

            updatePlayerUI();

            return true;
        }
        else if (overHeal)
        {
            Hp += amount;

            updatePlayerUI();

            return true;
        }

        return false;
    }
}
