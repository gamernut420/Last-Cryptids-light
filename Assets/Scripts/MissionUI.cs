using System.Text;
using UnityEngine;
using TMPro;

public class MissionUI : MonoBehaviour
{
    [Header("HUD Objective Display")]

    [SerializeField]
    private TextMeshProUGUI headerText;

    [SerializeField]
    private TextMeshProUGUI primaryObjectiveText;

    [SerializeField]
    private TextMeshProUGUI checklistText;


    [Header("Optional System Status")]

    [SerializeField]
    private TextMeshProUGUI statusText;


    private void Start()
    {
        if (headerText != null)
        {
            headerText.text =
                "OBJECTIVE";
        }


        RefreshUI();
    }


    private void Update()
    {
        RefreshUI();
    }


    private void RefreshUI()
    {
        if (ObjectiveManager.Instance ==
            null)
        {
            return;
        }


        if (headerText != null)
        {
            headerText.text =
                "OBJECTIVE";
        }


        ObjectiveData primary =
            GetPrimaryObjective();


        if (primaryObjectiveText != null)
        {
            if (primary != null)
            {
                primaryObjectiveText.text =
                    primary.objectiveTitle;
            }
            else
            {
                primaryObjectiveText.text =
                    "MISSION COMPLETE";
            }
        }


        if (checklistText != null)
        {
            checklistText.text =
                BuildChecklist();
        }


        if (statusText != null)
        {
            if (primary != null)
            {
                statusText.text =
                    "NAV DATA // ACTIVE";
            }
            else
            {
                statusText.text =
                    "NAV DATA // COMPLETE";
            }
        }
    }


    private ObjectiveData GetPrimaryObjective()
    {
        foreach (
            ObjectiveData obj
            in ObjectiveManager.Instance
                .activeObjectives)
        {
            if (obj == null)
            {
                continue;
            }


            if (!obj.isUnlocked)
            {
                continue;
            }


            if (obj.shouldHideFromUI)
            {
                continue;
            }


            if (obj.isCompleted)
            {
                continue;
            }


            return obj;
        }


        return null;
    }


    private string BuildChecklist()
    {
        StringBuilder builder =
            new StringBuilder();


        foreach (
            ObjectiveData obj
            in ObjectiveManager.Instance
                .activeObjectives)
        {
            if (obj == null)
            {
                continue;
            }


            if (!obj.isUnlocked)
            {
                continue;
            }


            if (obj.shouldHideFromUI)
            {
                continue;
            }


            if (obj.isCompleted)
            {
                builder.AppendLine(
                    "<color=#55D6F5>✓</color> " +
                    "<color=#8299A6>" +
                    obj.objectiveTitle +
                    "</color>"
                );
            }
            else
            {
                builder.AppendLine(
                    "<color=#EAF7FF>□</color> " +
                    obj.objectiveTitle
                );
            }
        }


        return builder.ToString();
    }
}