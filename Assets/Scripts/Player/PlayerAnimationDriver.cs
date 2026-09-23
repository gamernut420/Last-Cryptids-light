using UnityEngine;

/// <summary>
/// Drives the Space Soldier Animator using the Player's movement.
/// This script does not control gameplay movement.
/// It only updates animation parameters.
/// </summary>
public class PlayerAnimationDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private CharacterController characterController;

    [Header("Animator Parameters")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string groundedParameter = "OnGround";

    [Header("Smoothing")]
    [SerializeField] private float speedDampTime = 0.1f;

    private Vector3 previousPosition;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerRoot == null)
        {
            playerRoot = transform.parent;
        }

        if (characterController == null && playerRoot != null)
        {
            characterController = playerRoot.GetComponent<CharacterController>();
        }

        if (playerRoot != null)
        {
            previousPosition = playerRoot.position;
        }
    }

    private void Update()
    {
        if (animator == null || playerRoot == null)
            return;

        // -------------------------
        // Movement Speed
        // -------------------------

        Vector3 movement = playerRoot.position - previousPosition;

        // Ignore vertical movement for locomotion speed.
        movement.y = 0f;

        float movementSpeed =
            movement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        animator.SetFloat(
            speedParameter,
            movementSpeed,
            speedDampTime,
            Time.deltaTime
        );

        // -------------------------
        // Grounded State
        // -------------------------

        if (characterController != null)
        {
            animator.SetBool(
                groundedParameter,
                characterController.isGrounded
            );
        }

        previousPosition = playerRoot.position;
    }
}
