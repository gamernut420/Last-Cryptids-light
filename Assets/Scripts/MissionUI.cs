using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class MissionUI : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;

    private void Update()
    {
        if(ObjectiveManager.Instance != null && 
            ObjectiveManager.Instance.activeObjuctives.Count > 0)
        {
            string displayText = "<b>Objectives:</b>\n";
            bool hasIncompleteObjs = false;

            foreach (ObjectiveData obj in ObjectiveManager.Instance.activeObjuctives)
            {
                if (!obj.isCompleted)
                {
                    displayText += $"- {obj.objectiveTitle}\n";
                    hasIncompleteObjs = true;
                }
                else
                    displayText += $"<s>- {obj.objectiveTitle} (Complete)</s>\n";
            }
            if (!hasIncompleteObjs)
                displayText += "\nAll Objective Complete! Time to go home!";

                objectiveText.text = displayText;
        }
    }
}
