using TMPro;
using UnityEngine;

public class AmmoCounter : MonoBehaviour
{
    TextMeshProUGUI ammoCounter;

    private void OnEnable()
    {
        ammoCounter = GetComponent<TextMeshProUGUI>();

        GunController.UpdateAmmoText += UpdateAmmoText;

        playerController.ShowAmmoUI += ToggleAmmoText;
    }

    private void OnDisable()
    {
        GunController.UpdateAmmoText -= UpdateAmmoText;
    }

    void UpdateAmmoText(int current, int reserve)
    {
        ammoCounter.text = $"{current} / {reserve}";
    }

    void ToggleAmmoText(bool show)
    {
        ammoCounter.enabled = show;
    }
}
