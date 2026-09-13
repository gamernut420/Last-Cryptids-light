using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Active Objectives")]
    public List<ObjectiveData> activeObjectives = new List<ObjectiveData>();
    public int hideUIin;

    public Transform currentCompassTarget;

    private void Awake()
    {
        // Ensures only one instance of this manager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        foreach (var obj in activeObjectives)
        {
            if (obj != null)
            {
                obj.shouldHideFromUI = false;
                obj.isCompleted = false;
                // if it has no prerequisite, unlock it immadiately. Otherwise, lock it.
                if (obj.prerequisiteObjective == null)
                    obj.isUnlocked = true;
                else
                    obj.isUnlocked = false;


            }
        }
    }

    public void AddObjective(ObjectiveData newObj)
    {
        if(!activeObjectives.Contains(newObj))
        {
            activeObjectives.Add(newObj);
            
            //Set the compass target to this new objecive if it has a location
            if(newObj.targetLocation != null) currentCompassTarget = newObj.targetLocation;
        }

    }

    public void CompleteObjective(string objectiveID)
    {
        ObjectiveData targetObjective = activeObjectives.Find(o => o.objectiveID == objectiveID);
        if (targetObjective != null && targetObjective.isUnlocked && !targetObjective.isCompleted)
        {
            targetObjective.isCompleted = true;
            Debug.Log($"Objective Completed: {targetObjective.objectiveTitle}");

            // Call this right here to unlock any objectives waiting on this one
            UnlockNextObjective(targetObjective);
            // Wait x seconds, turn hide it from the UI
            StartCoroutine(HideObjectiveRoutine(targetObjective, hideUIin));
        }
    }

    private System.Collections.IEnumerator HideObjectiveRoutine(ObjectiveData objectiveToHide, float delay)
    {
        Debug.Log($"Objective hide in 2 secs: {objectiveToHide.objectiveTitle}");

        yield return new WaitForSeconds(delay);
        objectiveToHide.shouldHideFromUI = true;
    }
    private void UnlockNextObjective(ObjectiveData completedObjective)
    {
        // Loop through your active objectives (or a master list of all game objectives)
        foreach(var obj in activeObjectives)
        {
            if(!obj.isUnlocked && obj.prerequisiteObjective == completedObjective)
            {
                obj.isUnlocked = true;
                Debug.Log($"Objective Unlocked: { obj.objectiveTitle}");

            }
        }
    }
}
