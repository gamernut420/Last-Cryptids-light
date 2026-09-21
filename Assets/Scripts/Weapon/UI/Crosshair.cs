using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair References")]
    [SerializeField] private RectTransform pivotUp;
    [SerializeField] private RectTransform pivotRight;
    [SerializeField] private RectTransform pivotDown;
    [SerializeField] private RectTransform pivotLeft;

    [Header("Normal Crosshair")]
    [SerializeField]
    [Min(0f)]
    private float baseDistance = 18f;

    [Header("ADS Crosshair")]
    [SerializeField]
    [Min(0f)]
    private float adsDistance = 8f;

    [SerializeField]
    private float adsMoveSpeed = 15f;

    [Header("Weapon Spread")]
    [SerializeField]
    private float spreadToScale = 0.1f;

    [SerializeField]
    private float maxSpreadSize = 2.5f;

    [SerializeField]
    private float spreadReturnSpeed = 10f;

    [SerializeField]
    private float returnDelay = 0.25f;

    private float spreadDistance;
    private float returnDelayTimer;

    private bool isAiming;


    private void OnEnable()
    {
        GunController.SendReticleSpread += ReceiveSpread;
        GunController.ChangedAim += OnAimChanged;
    }


    private void OnDisable()
    {
        GunController.SendReticleSpread -= ReceiveSpread;
        GunController.ChangedAim -= OnAimChanged;
    }


    private void Start()
    {
        SetPivotRotations();
        UpdateCrosshairImmediate();
    }


    private void Update()
    {
        returnDelayTimer -= Time.deltaTime;

        if (returnDelayTimer <= 0f)
        {
            spreadDistance =
                Mathf.Lerp(
                    spreadDistance,
                    0f,
                    spreadReturnSpeed *
                    Time.deltaTime
                );
        }


        float targetBaseDistance =
            isAiming
            ? adsDistance
            : baseDistance;


        float finalDistance =
            targetBaseDistance +
            spreadDistance;


        UpdatePivot(
            pivotUp,
            Vector2.up * finalDistance
        );

        UpdatePivot(
            pivotRight,
            Vector2.right * finalDistance
        );

        UpdatePivot(
            pivotDown,
            Vector2.down * finalDistance
        );

        UpdatePivot(
            pivotLeft,
            Vector2.left * finalDistance
        );
    }


    private void UpdatePivot(
        RectTransform pivot,
        Vector2 targetPosition)
    {
        if (pivot == null)
            return;


        pivot.anchoredPosition =
            Vector2.Lerp(
                pivot.anchoredPosition,
                targetPosition,
                adsMoveSpeed *
                Time.deltaTime
            );
    }


    private void ReceiveSpread(
        float spreadAmount)
    {
        returnDelayTimer =
            returnDelay;


        float spread =
            1f +
            spreadAmount *
            spreadToScale;


        spreadDistance =
            Mathf.Clamp(
                Mathf.Max(
                    spreadDistance,
                    spread
                ),
                0f,
                maxSpreadSize
            ) * 10f;
    }


    private void OnAimChanged(
        bool aiming)
    {
        isAiming =
            aiming;
    }


    private void SetPivotRotations()
    {
        if (pivotUp != null)
        {
            pivotUp.localRotation =
                Quaternion.identity;
        }

        if (pivotRight != null)
        {
            pivotRight.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    -90f
                );
        }

        if (pivotDown != null)
        {
            pivotDown.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    180f
                );
        }

        if (pivotLeft != null)
        {
            pivotLeft.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    90f
                );
        }
    }


    private void UpdateCrosshairImmediate()
    {
        if (pivotUp != null)
        {
            pivotUp.anchoredPosition =
                Vector2.up *
                baseDistance;
        }

        if (pivotRight != null)
        {
            pivotRight.anchoredPosition =
                Vector2.right *
                baseDistance;
        }

        if (pivotDown != null)
        {
            pivotDown.anchoredPosition =
                Vector2.down *
                baseDistance;
        }

        if (pivotLeft != null)
        {
            pivotLeft.anchoredPosition =
                Vector2.left *
                baseDistance;
        }
    }
}