using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    public ObjectiveData targetObjective;
    public bool isCompeletionTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isCompeletionTrigger)
                ObjectiveManager.Instance.CompleteObjective(targetObjective.objectiveID);
            else
                ObjectiveManager.Instance.AddObjective(targetObjective);

            // Disable trigger so it only fires once
           gameObject.SetActive(false);
        }
    }
}
