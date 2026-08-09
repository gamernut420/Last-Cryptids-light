using System.Linq;
using TMPro;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject WeaponUI;
    [SerializeField] TextMeshProUGUI AmmoCounter;

    public bool isPaused;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
    }

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = 1;
        menuActive.SetActive(false);
        menuActive = null;
    }

    //If the player does not start with a weapon the UI can be shown when they get one
    public void ToggleWeaponInfo(bool active)
    {
        WeaponUI.SetActive(active);
    }
}
