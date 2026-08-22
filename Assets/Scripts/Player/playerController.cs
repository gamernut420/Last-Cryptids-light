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

    int jumpCount;
    int HPOrig;
    float speedOrig;

    float shootTimer;

    Vector3 moveDir;
    Vector3 playerVel;


    bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = Hp;
        speedOrig = speed;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.instance.isPaused) return;

        if (isDead) return;
 
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

    public void PlayerAddWeapon(GameObject Weapon)
    {
        IWeapon wep = Weapon.GetComponent<IWeapon>();

        if (wep != null)
        {
            wep.SetPlayerVariables(GetComponent<IPlayer>(), Camera.main.GetComponent<ICamera>(), WeaponGrip.transform.localPosition);
            wep.SetWeaponUse(true);

            Weapon.transform.SetParent(Camera.main.transform);

            Weapon.transform.localPosition = Vector3.zero;
        }
    }

    void DropWeapon()
    {
        IWeapon wep = ActiveWeapon.GetComponent<IWeapon>();

        if(wep != null)
        {
            wep.SetWeaponUse(false);
            wep.SetPlayerVariables();

            
        }
    }

    void SwapWeapon()
    {
        if (Input.GetButtonDown("1") && weapons[0] != null)
        {
            ActiveWeapon.SetActive(false);

            ActiveWeapon = weapons[0];

            ActiveWeapon.SetActive(true);
        }
        else if (Input.GetButtonDown("2") && weapons[1] != null)
        {
            ActiveWeapon.SetActive(false);

            ActiveWeapon = weapons[1];

            ActiveWeapon.SetActive(true);
        }
        else if (Input.GetButtonDown("3") && weapons[2] != null)
        {
            ActiveWeapon.SetActive(false);

            ActiveWeapon = weapons[2];

            ActiveWeapon.SetActive(true);
        }
    }
}
