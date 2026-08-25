using UnityEngine;

public interface IInteract
{
    bool Interact(GameObject interactor);

    //Return >= 1 to interact. Do CurrectHold / HoldTimer
    float DoHold()
    {
        return 1;
    }

    void StopHold()
    {
        //Add code for stopping hold in script after adding this line
    }

    //Return an empty string or null to hide text
    string ScreenMessage();
}
