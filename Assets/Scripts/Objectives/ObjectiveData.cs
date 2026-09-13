
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectiveData", menuName = "Scriptable Objects/New Objective")]
public class ObjectiveData : ScriptableObject
{
    [Header("Progression")]
    public ObjectiveData prerequisiteObjective; // Drag the mission that must be finished first here
    [HideInInspector] public bool isUnlocked = false; // Set to true by default only if there's no prerequisite   
    [HideInInspector] public bool shouldHideFromUI = false; // Set to true by default only if there's no prerequisite   
    public string objectiveID;
    public string objectiveTitle;
    [TextArea] public string objectiveDescription;
    public Transform targetLocation;
    public bool isCompleted = false;


    private void Awake()
    {
        isCompleted = false;
        // If there is no prerequisite, unlock it by default at the start of the game
        if (prerequisiteObjective == null)
            isUnlocked = true;
        else
            isUnlocked = false;
    }
}
