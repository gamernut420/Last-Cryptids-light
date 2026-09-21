using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponHUDController : MonoBehaviour
{
    [Serializable]
    public class WeaponDisplayData
    {
        [Header("Weapon Match Name")]
        public string weaponName;

        [Header("HUD Display")]
        public string displayName;

        public Sprite weaponSprite;

        public Sprite ammoTypeSprite;
    }


    [Header("HUD References")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image weaponImage;
    [SerializeField] private Image ammoTypeImage;


    [Header("Weapon Display Data")]
    [SerializeField]
    private WeaponDisplayData[] weaponDisplayData;


    [Header("No Weapon")]
    [SerializeField]
    private string noWeaponText = "NO WEAPON";


    private void Start()
    {
        ClearWeapon();
    }


    public void SetWeapon(string weaponName)
    {
        // Nothing equipped.
        if (string.IsNullOrWhiteSpace(weaponName) ||
            weaponName.Equals(
                "None",
                StringComparison.OrdinalIgnoreCase))
        {
            ClearWeapon();
            return;
        }


        WeaponDisplayData data =
            FindWeaponData(weaponName);


        // We have an active weapon, but there is no
        // matching HUD entry.
        if (data == null)
        {
            if (weaponNameText != null)
            {
                weaponNameText.text =
                    weaponName;
            }

            if (weaponImage != null)
            {
                weaponImage.sprite = null;
                weaponImage.enabled = false;
            }

            if (ammoTypeImage != null)
            {
                ammoTypeImage.sprite = null;
                ammoTypeImage.enabled = false;
            }

            return;
        }


        // Weapon name.
        if (weaponNameText != null)
        {
            if (!string.IsNullOrWhiteSpace(
                data.displayName))
            {
                weaponNameText.text =
                    data.displayName;
            }
            else
            {
                weaponNameText.text =
                    weaponName;
            }
        }


        // Weapon picture.
        if (weaponImage != null)
        {
            weaponImage.sprite =
                data.weaponSprite;

            weaponImage.enabled =
                data.weaponSprite != null;

            if (data.weaponSprite != null)
            {
                weaponImage.gameObject
                    .SetActive(true);
            }
        }


        // Ammo-type picture.
        if (ammoTypeImage != null)
        {
            ammoTypeImage.sprite =
                data.ammoTypeSprite;

            ammoTypeImage.enabled =
                data.ammoTypeSprite != null;

            if (data.ammoTypeSprite != null)
            {
                ammoTypeImage.gameObject
                    .SetActive(true);
            }
        }
    }


    private WeaponDisplayData FindWeaponData(
        string weaponName)
    {
        if (weaponDisplayData == null)
        {
            return null;
        }


        for (int i = 0;
             i < weaponDisplayData.Length;
             i++)
        {
            WeaponDisplayData data =
                weaponDisplayData[i];


            if (data == null)
            {
                continue;
            }


            if (string.Equals(
                data.weaponName,
                weaponName,
                StringComparison.OrdinalIgnoreCase))
            {
                return data;
            }
        }


        return null;
    }


    // ADDED:
    // Allows the hotbar to use the same weapon
    // sprites already configured for this HUD.
    public Sprite GetWeaponSprite(
        string weaponName)
    {
        WeaponDisplayData data =
            FindWeaponData(weaponName);


        if (data == null)
        {
            return null;
        }


        return data.weaponSprite;
    }


    // Optional helper if another HUD later needs
    // the ammo-type sprite too.
    public Sprite GetAmmoTypeSprite(
        string weaponName)
    {
        WeaponDisplayData data =
            FindWeaponData(weaponName);


        if (data == null)
        {
            return null;
        }


        return data.ammoTypeSprite;
    }


    public void ClearWeapon()
    {
        if (weaponNameText != null)
        {
            weaponNameText.text =
                noWeaponText;
        }


        if (weaponImage != null)
        {
            weaponImage.sprite = null;
            weaponImage.enabled = false;
        }


        if (ammoTypeImage != null)
        {
            ammoTypeImage.sprite = null;
            ammoTypeImage.enabled = false;
        }
    }
}