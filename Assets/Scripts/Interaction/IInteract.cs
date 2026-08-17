using UnityEngine;

public interface IInteract
{
    bool Interact(GameObject interactor);

    bool DoHold()
    {
        //Return true to interact
        return true;
    }

    void StopHold()
    {
        //Add code for stopping hold in script after adding this line
    }

    //Return an empty string or null to hide text
    string ScreenMessage();
}
