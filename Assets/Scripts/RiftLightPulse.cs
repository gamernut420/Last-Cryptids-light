using UnityEngine;

[RequireComponent(typeof(Light))]
public class RiftLightPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float minIntensity = 1.2f;
    [SerializeField] private float maxIntensity = 2.0f;
    [SerializeField] private float pulseSpeed = 2.0f;

    [Header("Optional Range Pulse")]
    [SerializeField] private bool pulseRange = true;
    [SerializeField] private float rangePulseAmount = 0.25f;

    [Header("Variation")]
    [SerializeField] private bool randomizeStart = true;

    private Light portalLight;
    private float baseRange;
    private float timeOffset;

    private void Awake()
    {
        portalLight = GetComponent<Light>();
        baseRange = portalLight.range;

        if (randomizeStart)
            timeOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float pulse = (Mathf.Sin((Time.time + timeOffset) * pulseSpeed) + 1f) * 0.5f;

        portalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);

        if (pulseRange)
        {
            portalLight.range = baseRange + Mathf.Sin((Time.time + timeOffset) * pulseSpeed) * rangePulseAmount;
        }
    }
}