using UnityEngine;

public class WeaponHUDBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponHUDController weaponHUD;
    [SerializeField] private playerController player;

    private string lastWeaponName = "";

    private void Start()
    {
        FindReferences();

        RefreshHUD();
    }

    private void Update()
    {
        if (weaponHUD == null || player == null)
            return;

        string currentWeaponName =
            player.GetActiveItemNameForCheckpoint();

        if (string.IsNullOrEmpty(currentWeaponName))
        {
            currentWeaponName = "None";
        }

        // Only update the HUD when the equipped weapon changes.
        if (currentWeaponName != lastWeaponName)
        {
            lastWeaponName = currentWeaponName;

            weaponHUD.SetWeapon(currentWeaponName);
        }
    }

    private void FindReferences()
    {
        // Find the HUD controller on this object if it
        // wasn't assigned manually.
        if (weaponHUD == null)
        {
            weaponHUD = GetComponent<WeaponHUDController>();
        }

        // Find the player automatically if it wasn't assigned.
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player =
                    playerObject.GetComponent<playerController>();
            }
        }
    }

    private void RefreshHUD()
    {
        if (weaponHUD == null || player == null)
            return;

        string currentWeaponName =
            player.GetActiveItemNameForCheckpoint();

        if (string.IsNullOrEmpty(currentWeaponName))
        {
            currentWeaponName = "None";
        }

        lastWeaponName = currentWeaponName;

        weaponHUD.SetWeapon(currentWeaponName);
    }
}