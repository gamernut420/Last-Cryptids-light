using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SensitivitySlider : MonoBehaviour
{
    
    [SerializeField] private cameraController playerCamera;
    [Min(0.01f)]
    [SerializeField] private float defaultSensitivity = 20f;

    private const string SensitivityKey = "MouseSensitivity";
    private Slider sensitivitySlider;

    private void Awake()
    {
        sensitivitySlider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, defaultSensitivity);
        savedSensitivity = Mathf.Clamp(savedSensitivity, sensitivitySlider.minValue, sensitivitySlider.maxValue);
        sensitivitySlider.SetValueWithoutNotify(savedSensitivity);
        ApplyToCamera(savedSensitivity);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    private void OnDisable()
    {
        sensitivitySlider.onValueChanged.RemoveListener(SetSensitivity);
    }

    public void SetSensitivity(float sensitivity)
    {
        sensitivity = Mathf.Clamp(sensitivity, sensitivitySlider.minValue, sensitivitySlider.maxValue);
        PlayerPrefs.SetFloat(SensitivityKey, sensitivity);
        PlayerPrefs.Save();

        ApplyToCamera(sensitivity);
    }

    private void ApplyToCamera(float sensitivity)
    {
        if (playerCamera == null)
        {
            playerCamera = FindAnyObjectByType<cameraController>();
        }
        if (playerCamera != null)
        {
            playerCamera.SetSensitivity(sensitivity);
        }
    }
}
