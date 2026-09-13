
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectiveData", menuName = "Scriptable Objects/New Objective")]
public class ObjectiveData : ScriptableObject
{
    public string objectiveID;
    public string objectiveTitle;
    [TextArea] public string objectiveDescription;
    public Transform targetLocation;
    public bool isCompleted = false;


    private void Awake()
    {
        isCompleted = false;
    }
}
