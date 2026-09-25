using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject inventoryPanel;

    [Header("Input Key")]
    public KeyCode toggleKey;

    [Header("Other UI")]
    [SerializeField] private GameObject[] otherUI;


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
        if (isActive)
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible =
                true;
        }

        if (inventoryPanel == null)
        {
            return;
        }


        inventoryPanel.SetActive(
            isActive
        );


        if (isActive)
        {

            gameManager.instance.statePause();

            gameManager.instance.playerScript.enabled = false;

            for (int i = 0; i < otherUI.Length; i++)
            {
                otherUI[i].SetActive(false);
            }


            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible =
                true;
        }
        else
        {
            gameManager.instance.stateUnpause();

            gameManager.instance.playerScript.enabled = true;

            for (int i = 0; i < otherUI.Length; i++)
            {
                otherUI[i].SetActive(true);
            }


            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible =
                false;
        }
    }
}