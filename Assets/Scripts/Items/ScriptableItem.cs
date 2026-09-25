using UnityEngine;

[CreateAssetMenu]

public class ScriptableItem : ScriptableObject
{
    public GameObject itemModel;

    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
    public int stackCount;
    [Header("Objective Settings")]
    public ObjectiveData targetObjective;
    public int amountPicked;
}
