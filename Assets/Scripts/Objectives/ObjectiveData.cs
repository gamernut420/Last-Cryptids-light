using UnityEngine;


// ADDED:
// Determines where this objective should appear on the HUD.
public enum ObjectiveDisplayType
{
    Main,
    Requirement,
    Side
}


[CreateAssetMenu(
    fileName = "ObjectiveData",
    menuName = "Scriptable Objects/New Objective")]
public class ObjectiveData : ScriptableObject
{
    [Header("Progression")]

    public ObjectiveData prerequisiteObjective;

    [HideInInspector]
    public bool isUnlocked = false;

    [HideInInspector]
    public bool shouldHideFromUI = false;

    public string objectiveID;

    public string objectiveTitle;

    [TextArea]
    public string objectiveDescription;

    public Transform targetLocation;

    public bool isCompleted = false;


    // ADDED:
    // Main = story objective
    // Requirement = sub-objective shown underneath a main objective
    // Side = optional/secondary objective
    [Header("HUD Display")]

    public ObjectiveDisplayType displayType = ObjectiveDisplayType.Main;


    // ADDED:
    // Requirements use this to determine which main objective
    // they should appear underneath.
    //
    // Leave this empty for Main and Side objectives.
    public ObjectiveData parentObjective;


    // ADDED:
    // Supports objectives such as:
    // Locate Military Bases 0/3
    // Clear Enemies 0/3
    [Header("Progress")]

    [Min(1)]
    public int requiredProgress = 1;

    [HideInInspector]
    public int currentProgress = 0;


    // ADDED:
    // Makes it easy for the HUD to determine whether
    // progress text should be displayed.
    public bool UsesProgress =>
        requiredProgress > 1;


    private void Awake()
    {
        isCompleted = false;

        // ADDED:
        currentProgress = 0;


        // If there is no prerequisite,
        // unlock it at the start of the game.
        if (prerequisiteObjective == null)
        {
            isUnlocked = true;
        }
        else
        {
            isUnlocked = false;
        }
    }
}