using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class playerController : MonoBehaviour, IPlayer, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;
    [Header("Player Stats:")]
    [Range(1, 10)][SerializeField] int Hp;
    [Range(1f, 10f)][SerializeField] float speed;
    [Range(2f, 5f)][SerializeField] float sprintMod;
    [Range(8, 15)][SerializeField] int jumpSpeed;
    [Range(1, 3)][SerializeField] int jumpMax;

    [Header("Other:")]
    [Range(15, 45)][SerializeField] int gravity;

    [Header("Audio")]
    public float walkHearingRadius = 5f;
    public float sprintHearingRadius = 10f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("UI & Game Over")]
    public GameObject gameOverPanel;

    [Header("Weapon")]
    [SerializeField] GameObject ActiveWeapon;

    int jumpCount;
    int HPOrig;
    float speedOrig;

    private int sprintCount = 0;

    float shootTimer;

    Vector3 moveDir;
    Vector3 playerVel;


    bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = Hp;
        speedOrig = speed;
        updatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
 
        movement();
        sprint();
        if (sprintCount == 1)
        {
            NoiseManager.MakeNoise(transform.position, sprintHearingRadius);
        }
        ///////   Testing Logic    ///////

        // Testing key: 'K' to instantly kill the playerand test the death screen
        if (Input.GetKeyDown(KeyCode.K)) takeDamage(Hp);

        // Testing timer: 'T' to start timer
        if(Input.GetKeyDown(KeyCode.T)) FindAnyObjectByType<ExtractionCountdown>().StartExtractionTimer();
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = 0;
        }

        if (speed > speedOrig)
        {
            sprintCount = 1;
        }
        else
        {
            sprintCount = 0;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * speed * Time.deltaTime);
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

    public bool PlayerRefillAmmo(int amount)
    {
        IWeapon wep = ActiveWeapon.GetComponent<IWeapon>();

        if(wep != null)
        {
            return wep.WeaponRefillAmmo(amount);
        }
        else
        {
            return false;
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

    public void takeDamage(int amount)
    {
        Hp -= amount;
        if (isDead) return;
        updatePlayerUI();
        StartCoroutine(flashDamage());

        Debug.Log("Player took damge. Current HP: " + Hp);

        if (Hp <= 0) gameManager.instance.youLose();
    }
}
