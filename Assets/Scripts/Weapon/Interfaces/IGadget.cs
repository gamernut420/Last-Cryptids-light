using UnityEngine;

public interface IGadget
{
    public string GetGadgetName();

    public ScriptableItem GetItemInfo();

    public bool UseGadget(GameObject player);
}
