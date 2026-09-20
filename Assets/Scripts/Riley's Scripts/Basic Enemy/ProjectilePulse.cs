using UnityEngine;

public class ProjectilePulse : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 8f;
    [SerializeField] private float pulseAmount = 0.15f;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        float pulse = 1f +
            Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        transform.localScale = originalScale * pulse;
    }
}
