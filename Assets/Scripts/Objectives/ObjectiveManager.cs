using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Active Objectives")]
    public List<ObjectiveData> activeObjuctives = new List<ObjectiveData>();

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
    }

    public void AddObjective(ObjectiveData newObj)
    {
        if(!activeObjuctives.Contains(newObj))
        {
            activeObjuctives.Add(newObj);
            
            //Set the compass target to this new objecive if it has a location
            if(newObj.targetLocation != null) currentCompassTarget = newObj.targetLocation;
        }

    }

    public void CompleteObjective(string objectiveID)
    {
        ObjectiveData targetObjactive = activeObjuctives.Find(o => o.objectiveID == objectiveID);
        if (targetObjactive != null && !targetObjactive.isCompleted)
        {
            targetObjactive.isCompleted = true;
            Debug.Log($"Objective Completed: {targetObjactive.objectiveTitle}");

            // Remove from active list or transition to next objective here

        }
    }
}
