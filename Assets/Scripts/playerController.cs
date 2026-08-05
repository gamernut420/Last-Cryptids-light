using UnityEngine;

public class playerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [Range(1, 10)][SerializeField] int Hp;
    [Range(1f, 10f)][SerializeField] float speed;
    [Range(2f, 5f)][SerializeField] float sprintMod;
    [Range(8, 15)][SerializeField] int jumpSpeed;
    [Range(1, 3)][SerializeField] int jumpMax;
    [Range(15, 45)][SerializeField] int gravity;

    int jumpCount;
    int HPOrig;

    float shootTimer;

    Vector3 moveDir;
    Vector3 playerVel;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = Hp;

    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
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
        controller.Move(playerVel * speed * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint")) speed *= sprintMod;

        else if (Input.GetButtonUp("Sprint")) speed /= sprintMod;
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }
}
