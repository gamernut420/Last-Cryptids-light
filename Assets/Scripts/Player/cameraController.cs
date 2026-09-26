using UnityEngine;

public class cameraController : MonoBehaviour, ICamera
{
    [SerializeField] float sens = 2f;
    [SerializeField] int lockVertMin, lockVertMax;

    ICamera camInterface;
    float camRotX;
    private const string SensitivityKey = "MouseSensitivity";
    private const float MinimumSensitivity = 0.5f;
    private const float MaximumSensitivity = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camInterface = this;
        sens = PlayerPrefs.GetFloat(SensitivityKey, sens);
        camRotX = 45;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * sens;
            float mouseY = Input.GetAxisRaw("Mouse Y") * sens;

            camInterface.ModifyCameraPitch(mouseY);
            camInterface.ModifyCameraYaw(mouseX);
        }
    }
    
    //Left Right
    void ICamera.ModifyCameraYaw(float yaw)
    {
        transform.parent.Rotate(Vector3.up * yaw);
    }

    //Up Down
    void ICamera.ModifyCameraPitch(float pitch)
    {
        camRotX -= pitch;
        camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(camRotX, 0, 0);
    }

    public void SetSensitivity(float sensitivity)
    {
        sens = Mathf.Clamp(sensitivity, MinimumSensitivity, MaximumSensitivity);
    }
}
