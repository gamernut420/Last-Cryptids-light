using System.Collections.Generic;
using UnityEngine;


public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance
    {
        get;
        private set;
    }


    [Header("Active Objectives")]
    public List<ObjectiveData> activeObjectives = new List<ObjectiveData>();


    [Tooltip("Seconds a completed objective remains visible.")]
    public int hideUIin = 2;

    [Header("Compass")]
    public Transform currentCompassTarget;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (ObjectiveData obj in activeObjectives)
        {
            if (obj == null)
            {
                continue;
            }

            obj.shouldHideFromUI = false;
            obj.isCompleted = false;

            // ADDED:
            // Reset runtime objective progress whenever
            // the scene starts.
            obj.currentProgress = 0;


            if (obj.prerequisiteObjective == null)
            {
                obj.isUnlocked = true;
            }
            else
            {
                obj.isUnlocked = false;
            }
        }
    }


    private void Start()
    {
        UpdateCompassTarget();
    }


    public void AddObjective(
        ObjectiveData newObj)
    {
        if (newObj == null)
        {
            return;
        }


        if (!activeObjectives.Contains(newObj))
        {
            activeObjectives.Add(newObj);
        }

        if (newObj.prerequisiteObjective == null)
        {
            newObj.isUnlocked = true;
        }


        UpdateCompassTarget();
    }


    // ADDED:
    // Used for multi-step objectives such as:
    //
    // Locate Military Bases 0/3
    // Clear Bases 0/3
    //
    // Each trigger can add one or more points of progress.
    public void AddObjectiveProgress(string objectiveID, int amount = 1)
    {
        ObjectiveData targetObjective = activeObjectives.Find(o => o != null && o.objectiveID == objectiveID);
        
        if (targetObjective == null)
        {
            return;
        }

        if (!targetObjective.isUnlocked || targetObjective.isCompleted)
        {
            return;
        }


        targetObjective.currentProgress = Mathf.Clamp(targetObjective.currentProgress + amount, 0, targetObjective.requiredProgress);

        // ADDED:
        // Automatically finish the objective once
        // the required amount has been reached.
        if (targetObjective.currentProgress >=
            targetObjective.requiredProgress)
        {
            CompleteObjective(objectiveID);
        }
    }


    public void CompleteObjective(string objectiveID)
    {
        ObjectiveData targetObjective = activeObjectives.Find(o => o != null && o.objectiveID == objectiveID);

        if (targetObjective == null)
        {
            return;
        }

        if (!targetObjective.isUnlocked || targetObjective.isCompleted)
        {
            return;
        }


        // ADDED:
        // If this is a progress-based objective and something
        // completes it directly, make sure its progress also
        // reaches the required amount.
        targetObjective.currentProgress = targetObjective.requiredProgress;

        targetObjective.isCompleted = true;
        
        UnlockNextObjective(targetObjective);
        UpdateCompassTarget();
        StartCoroutine(HideObjectiveRoutine(targetObjective,hideUIin));
    }

    private System.Collections.IEnumerator
        HideObjectiveRoutine(ObjectiveData objectiveToHide,float delay)
    {
        yield return new WaitForSeconds(delay);


        if (objectiveToHide != null)
        {
            objectiveToHide.shouldHideFromUI =true;
        }


        UpdateCompassTarget();
    }


    private void UnlockNextObjective(
        ObjectiveData completedObjective)
    {
        foreach (ObjectiveData obj in activeObjectives)
        {
            if (obj == null)
            {
                continue;
            }


            if (!obj.isUnlocked && obj.prerequisiteObjective == completedObjective)
            {
                obj.isUnlocked = true;
            }
        }
    }


    private void UpdateCompassTarget()
    {
        currentCompassTarget = null;


        foreach (ObjectiveData obj in activeObjectives)
        {
            if (obj == null)
            {
                continue;
            }


            if (!obj.isUnlocked || obj.isCompleted || obj.shouldHideFromUI)
            {
                continue;
            }


            if (obj.targetLocation == null)
            {
                continue;
            }

            currentCompassTarget = obj.targetLocation;

            return;
        }
    }


    public ObjectiveData GetCurrentObjective()
    {
        foreach (ObjectiveData obj in activeObjectives)
        {
            if (obj == null)
            {
                continue;
            }


            if (obj.isUnlocked && !obj.isCompleted && !obj.shouldHideFromUI)
            {
                return obj;
            }
        }
        return null;
    }
}
