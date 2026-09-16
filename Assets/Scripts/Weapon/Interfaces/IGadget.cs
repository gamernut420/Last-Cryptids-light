using UnityEngine;

public interface IGadget
{
    public string GetGadgetName();

    public bool UseGadget(GameObject player);
}
