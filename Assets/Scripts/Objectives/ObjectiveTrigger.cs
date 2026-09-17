using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [Header("Objective")]
    public ObjectiveData targetObjective;


    [Header("Trigger Type")]
    public bool isCompletionTrigger =
        false;


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


        if (isCompletionTrigger)
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