using TMPro;
using UnityEngine;

public class AmmoCounter : MonoBehaviour
{
    TextMeshProUGUI ammoCounter;

    private void Awake()
    {
        ammoCounter = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (ammoCounter == null)
        {
            ammoCounter = GetComponent<TextMeshProUGUI>();

        }
        if (ammoCounter == null)
        {
            Debug.LogError("AmmoCounter must be attached to a TextMeshPro UI object.", this); return;
        }
        GunController.UpdateAmmoText += UpdateAmmoText;

        playerController.ShowAmmoUI += ToggleAmmoText;
    }

    private void OnDisable()
    {
        GunController.UpdateAmmoText -= UpdateAmmoText;
        playerController.ShowAmmoUI -= ToggleAmmoText;
    }

    private void UpdateAmmoText(int current, int reserve)
    {
        ammoCounter.text = $"{current} / {reserve}";
    }

    private void ToggleAmmoText(bool show)
    {
        ammoCounter.enabled = show;
    }
}
