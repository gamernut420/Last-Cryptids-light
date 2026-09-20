using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject inventoryPanel;

    [Header("Input Key")]
    public KeyCode toggleKey;


    void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInevntory();
        }
    }


    public void ToggleInevntory()
    {
        if (inventoryPanel == null)
        {
            return;
        }


        bool isActive =
            !inventoryPanel.activeSelf;


        // ADDED FOR CRAFTING:
        SetInventoryState(isActive);
    }


    // ADDED FOR CRAFTING:
    public void OpenInventory()
    {
        SetInventoryState(true);
    }


    // ADDED FOR CRAFTING:
    public void CloseInventory()
    {
        SetInventoryState(false);
    }


    // ADDED FOR CRAFTING:
    public bool IsInventoryOpen()
    {
        return inventoryPanel != null &&
               inventoryPanel.activeSelf;
    }


    // ADDED FOR CRAFTING:
    private void SetInventoryState(
        bool isActive)
    {
        if (inventoryPanel == null)
        {
            return;
        }


        inventoryPanel.SetActive(
            isActive
        );


        if (isActive)
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible =
                true;
        }
        else
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible =
                false;
        }
    }
}