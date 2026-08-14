using UnityEngine;

public class cameraController : MonoBehaviour, ICamera
{
    [SerializeField] int sens;
    [SerializeField] int lockVertMin, lockVertMax;

    ICamera camInterface;
    float camRotX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camInterface = this;

        camRotX = 45;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sens;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sens;

        camInterface.ModifyCameraPitch(mouseY);
        camInterface.ModifyCameraYaw(mouseX);
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
}
