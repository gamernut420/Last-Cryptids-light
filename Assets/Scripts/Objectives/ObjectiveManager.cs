using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Active Objectives")]
    public List<ObjectiveData> activeObjectives =
        new List<ObjectiveData>();

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
        if (newObj == null)
        {
            return;
        }


        if (!activeObjectives.Contains(newObj))
        {
            activeObjectives.Add(newObj);


            // Set the compass target to this new objective
            // if it has a location.
            if (newObj.targetLocation != null)
            {
                // ADDED FOR COMPASS:
                SetCurrentCompassTarget(
                    newObj.targetLocation
                );
            }
        }
    }


    public void CompleteObjective(string objectiveID)
    {
        ObjectiveData targetObjective =
            activeObjectives.Find(
                o => o.objectiveID == objectiveID
            );


        if (targetObjective != null &&
            !targetObjective.isCompleted)
        {
            targetObjective.isCompleted = true;

            Debug.Log(
                $"Objective Completed: " +
                $"{targetObjective.objectiveTitle}"
            );


            // ADDED FOR COMPASS:
            // If the completed objective was the current
            // compass target, disable that marker and
            // look for another unfinished objective.
            if (targetObjective.targetLocation ==
                currentCompassTarget)
            {
                ClearCurrentCompassTarget();

                SelectNextCompassTarget();
            }
        }
    }


    // ADDED FOR COMPASS:
    // Changes the objective currently being tracked.
    private void SetCurrentCompassTarget(
        Transform newTarget)
    {
        // Hide the previous objective-controlled marker.
        if (currentCompassTarget != null &&
            currentCompassTarget != newTarget)
        {
            SetObjectivePOIState(
                currentCompassTarget,
                false
            );
        }


        currentCompassTarget =
            newTarget;


        if (currentCompassTarget != null)
        {
            SetObjectivePOIState(
                currentCompassTarget,
                true
            );
        }
    }


    // ADDED FOR COMPASS:
    // Clears the current marker.
    private void ClearCurrentCompassTarget()
    {
        if (currentCompassTarget != null)
        {
            SetObjectivePOIState(
                currentCompassTarget,
                false
            );
        }


        currentCompassTarget = null;
    }


    // ADDED FOR COMPASS:
    // Finds another unfinished objective after the
    // current objective has been completed.
    private void SelectNextCompassTarget()
    {
        for (int i = 0;
             i < activeObjectives.Count;
             i++)
        {
            ObjectiveData objective =
                activeObjectives[i];


            if (objective == null)
            {
                continue;
            }


            if (objective.isCompleted)
            {
                continue;
            }


            if (objective.targetLocation == null)
            {
                continue;
            }


            SetCurrentCompassTarget(
                objective.targetLocation
            );

            return;
        }


        // No unfinished objective with a location exists.
        currentCompassTarget = null;
    }


    // ADDED FOR COMPASS:
    // Finds a CompassPOI attached to the objective target.
    //
    // It checks:
    // 1. The target itself
    // 2. Its parent
    // 3. Its children
    //
    // This gives your level designers some flexibility
    // when setting up objective objects.
    private CompassPOI FindCompassPOI(
        Transform target)
    {
        if (target == null)
        {
            return null;
        }


        CompassPOI poi =
            target.GetComponent<CompassPOI>();


        if (poi == null)
        {
            poi =
                target.GetComponentInParent<CompassPOI>();
        }


        if (poi == null)
        {
            poi =
                target.GetComponentInChildren<CompassPOI>();
        }


        return poi;
    }


    // ADDED FOR COMPASS:
    // Only objective-controlled POIs are automatically
    // hidden/shown by ObjectiveManager.
    //
    // Normal world POIs remain independent.
    private void SetObjectivePOIState(
        Transform target,
        bool active)
    {
        CompassPOI poi =
            FindCompassPOI(target);


        if (poi == null)
        {
            return;
        }


        if (!poi.ControlledByObjective)
        {
            return;
        }


        poi.SetCompassActive(active);
    }
}