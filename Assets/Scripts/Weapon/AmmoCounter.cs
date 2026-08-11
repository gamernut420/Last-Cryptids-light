using TMPro;
using UnityEngine;

public class AmmoCounter : MonoBehaviour
{
    TextMeshProUGUI ammoCounter;
    int currentAmmo;
    int reserveAmmo;

    private void OnEnable()
    {
        ammoCounter = GetComponent<TextMeshProUGUI>();

        GunController.CurrentAmmoChanged += UpdateCurrentAmmo;
        GunController.ReserveAmmoChanged += UpdateReserveAmmo;
    }

    private void OnDisable()
    {
        GunController.CurrentAmmoChanged -= UpdateCurrentAmmo;
        GunController.ReserveAmmoChanged -= UpdateReserveAmmo;
    }

    void UpdateAmmoText()
    {
        ammoCounter.text = string.Format("{0} / {1}", currentAmmo, reserveAmmo);
    }

    void UpdateCurrentAmmo(int ammount)
    {
        currentAmmo = ammount;

        UpdateAmmoText();
    }

    void UpdateReserveAmmo(int ammount)
    {
        reserveAmmo = ammount;

        UpdateAmmoText();
    }
}
