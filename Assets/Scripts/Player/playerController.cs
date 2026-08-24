using UnityEngine;
using System.Collections;
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
    [SerializeField] GameObject WeaponGrip;
    GameObject ActiveWeapon;
    GameObject[] weapons = new GameObject[3];
    int activeWeaponSlot;
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

        ShowAmmoUI?.Invoke(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.instance.isPaused) return;

        if (isDead) return;


        SwapWeapon();
 
        movement();
        sprint();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            NoiseManager.MakeNoise(transform.position, sprintHearingRadius);
        }
        ///////   Testing Logic    ///////

        // Testing key: 'K' to instantly kill the playerand test the death screen
        if (Input.GetKeyDown(KeyCode.K)) takeDamage(Hp);
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

    public void PlayerAddWeapon(GameObject Weapon)
    {
        IWeapon wep = Weapon.GetComponent<IWeapon>();

        if (wep != null)
        {
            bool hadEmpty = false;

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] == null)
                {
                    hadEmpty = true;

                    Debug.Log("Found Slot");

                    if(ActiveWeapon != null)
                    {
                        ActiveWeapon.SetActive(false);
                    }

                    weapons[i] = Weapon;

                    ActiveWeapon = weapons[i];

                    activeWeaponSlot = i;

                    break;
                }
            }

            if (hadEmpty == false)
            {
                DropWeapon();

                weapons[activeWeaponSlot] = Weapon;

                ActiveWeapon = weapons[activeWeaponSlot];
            }

            wep.SetPlayerVariables(GetComponent<IPlayer>(), Camera.main.GetComponent<ICamera>(), WeaponGrip.transform.localPosition);
            wep.SetWeaponUse(true);

            Weapon.transform.SetParent(Camera.main.transform);

            Weapon.transform.localPosition = WeaponGrip.transform.localPosition;

            Weapon.transform.localRotation = Quaternion.identity;

            ActiveWeapon.SetActive(false);
            ActiveWeapon.SetActive(true);

            ShowAmmoUI?.Invoke(ActiveWeapon != null);
        }
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
                ActiveWeapon.transform.localRotation = Quaternion.identity;

                ActiveWeapon = null;
                weapons[activeWeaponSlot] = null;

                ShowAmmoUI?.Invoke(false);
            }
        }
    }

    void SwapWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && weapons[0] != null && activeWeaponSlot != 0)
        {
            ActiveWeapon.SetActive(false);

            ActiveWeapon = weapons[0];

            activeWeaponSlot = 0;

            ActiveWeapon.SetActive(true);

            ShowAmmoUI?.Invoke(ActiveWeapon != null);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && weapons[1] != null && activeWeaponSlot != 1)
        {
            ActiveWeapon.SetActive(false);

            ActiveWeapon = weapons[1];

            activeWeaponSlot = 1;

            ActiveWeapon.SetActive(true);

            ShowAmmoUI?.Invoke(ActiveWeapon != null);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && weapons[2] != null && activeWeaponSlot != 2)
        {
            ActiveWeapon.SetActive(false);

            ActiveWeapon = weapons[2];

            activeWeaponSlot = 2;

            ActiveWeapon.SetActive(true);

            ShowAmmoUI?.Invoke(ActiveWeapon != null);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            DropWeapon();
        }
    }
}
