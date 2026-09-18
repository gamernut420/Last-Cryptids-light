using UnityEngine;


public class ObjectiveTrigger : MonoBehaviour
{
    [Header("Objective")]

    public ObjectiveData targetObjective;


    [Header("Trigger Type")]

    public bool isCompletionTrigger =
        false;


    // ADDED:
    // Enable this instead of Is Completion Trigger
    // when this trigger should add progress toward
    // a multi-part objective.
    public bool addsProgress =
        false;


    // ADDED:
    // Normally leave this at 1.
    [Min(1)]
    public int progressAmount =
        1;


    private void OnTriggerEnter(
        Collider other)
    {
        if (!other.CompareTag(
                "Player"))
        {
            return;
        }


        if (targetObjective == null ||
            ObjectiveManager.Instance ==
            null)
        {
            return;
        }


        // ADDED:
        // Example:
        // Walking into Base 1 changes
        // Locate Bases 0/3 -> 1/3.
        if (addsProgress)
        {
            ObjectiveManager.Instance
                .AddObjectiveProgress(
                    targetObjective
                        .objectiveID,
                    progressAmount
                );
        }
        else if (isCompletionTrigger)
        {
            ObjectiveManager.Instance
                .CompleteObjective(
                    targetObjective
                        .objectiveID
                );
        }
        else
        {
            ObjectiveManager.Instance
                .AddObjective(
                    targetObjective
                );
        }


        gameObject.SetActive(
            false
        );
    }
}