using UnityEngine;

public class AmmoBox : MonoBehaviour, IInteract
{
    public void Interact()
    {
        Debug.Log("Interacted with ammo");
    }

    public string ScreenMessage()
    {
        return "Pickup Ammo";
    }
}
