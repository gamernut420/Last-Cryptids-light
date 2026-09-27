using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject inventoryPanel;

    [Header("Input Key")]
    public KeyCode toggleKey;

    //References to the HUD and weapon UI
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject weaponUI;


// ADDED: Automatically finds the UI objects
void Awake()
    {
        Transform ui = transform.parent;

        if (ui != null)
        {
            if (inventoryPanel == null)
            {
                inventoryPanel = ui.Find("InventoryPanel")?.gameObject;
            }

            //Finds WeaponUI under UI.
            weaponUI = ui.Find("WeaponUI")?.gameObject;

            //Finds HUD_Root outside UI
            if (ui.parent != null)
            {
                hudRoot = ui.parent.Find("HUD_Root")?.gameObject;
            }
        }
    }


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

            gameManager.instance.statePause();

            gameManager.instance.playerScript.enabled = false;

            //Hides the HUD and weapon UI
            if (hudRoot != null)
            {
                hudRoot.SetActive(false);
            }

            if (weaponUI != null)
            {
                weaponUI.SetActive(false);
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

            //Shows the HUD and weapon UI again
            if (hudRoot != null)
            {
                hudRoot.SetActive(true);
            }

            if (weaponUI != null)
            {
                weaponUI.SetActive(true);
            }


            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible =
                false;
        }
    }
}