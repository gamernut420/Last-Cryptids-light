using UnityEngine;

public interface IInteract
{
    bool Interact(GameObject interactor);

    //Return an empty string or null to hide text
    string ScreenMessage();
}
