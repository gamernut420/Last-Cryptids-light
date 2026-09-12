using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UpgradeCategory : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI CategoryName;

    GameObject ObjectToUpgrade;
    UnityAction<GameObject> SelectedFnc;

    public void Bind(GameObject gameObject, UnityAction<GameObject> selectedFunc)
    {
        ObjectToUpgrade = gameObject;
        SelectedFnc = selectedFunc;

        IWeapon wep;

        if(ObjectToUpgrade.TryGetComponent(out wep))
        {
            CategoryName.text = wep.GetWeaponName();
        }
        else
        {
            CategoryName.text = "Player";
        }
    }

    public void OnClicked()
    {
        if(SelectedFnc != null)
        {
            SelectedFnc.Invoke(ObjectToUpgrade);
        }
    }
}
