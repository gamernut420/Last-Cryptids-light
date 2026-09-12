using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject inventoryPanel;

    [Header("Input Key")]
    public KeyCode toggleKey;

    
    void Start()
    {
        if(inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleInevntory();
    }

    public void ToggleInevntory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        if (isActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState= CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
