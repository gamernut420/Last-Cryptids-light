using UnityEngine;

public interface IInteract
{
    void Interact();

    //Return an empty string or null to hide text
    string ScreenMessage();
}
