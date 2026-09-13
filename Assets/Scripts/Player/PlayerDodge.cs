using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(playerController))]
public class PlayerDodge : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode dodgeKey = KeyCode.LeftControl;

    [Header("Dodge Settings")]
    [Min(1f)]
    [SerializeField] private float speedMultiplier = 1.6f;
    [Min(0.01f)]
    [SerializeField] private float dodgeDuration = 0.2f;
    [Min(0f)]
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private bool requireGrounded = true;

    [Header("Invincibility")]
    [Min(0f)]
    [SerializeField] private float invincibilityDuration = 0.4f;

    public bool IsDodging { get; private set; }
    public bool IsInvincible { get; private set; }

    private CharacterController characterController;
    private playerController player;
    private float nextAllowedDodgeTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        player = GetComponent<playerController>();
    }

    void Update()
    {
        if (IsDodging)
        {
            return;
        }

        if (gameManager.instance != null && gameManager.instance.isPaused)
        {
            return;
        }

        if (Input.GetKeyDown(dodgeKey))
        {
            TryDodge();
        }
    }

    private void TryDodge()
    {
        if (Time.time < nextAllowedDodgeTime)
        {
            return;
        }

        if (requireGrounded && !characterController.isGrounded)
        {
            return;
        }

        Vector3 dodgeDirection = GetDodgeDirection();
        nextAllowedDodgeTime = Time.time + cooldown;
        StartCoroutine(DodgeRoutine(dodgeDirection));
    }

    private Vector3 GetDodgeDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = transform.right * horizontal + transform.forward * vertical;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }

        direction.y = 0f;
        return direction.normalized;
    }

    private IEnumerator DodgeRoutine(Vector3 direction)
    {
        IsDodging = true;
        IsInvincible = true;

        float elapsed = 0f;
        float dodgeSpeed = player.GetMaxSpeed() * speedMultiplier;

        while (elapsed < dodgeDuration)
        {
            characterController.Move(direction * dodgeSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        IsDodging = false;

        float remainingInvincibility = invincibilityDuration - elapsed;
        if (remainingInvincibility > 0f)
        {
            yield return new WaitForSeconds(remainingInvincibility);
        }

        IsInvincible = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        IsDodging = false;
        IsInvincible = false;
    }
}
