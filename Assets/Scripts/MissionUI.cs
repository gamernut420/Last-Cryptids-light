using System.Text;
using UnityEngine;
using TMPro;


public class MissionUI : MonoBehaviour
{
    [Header("Main Objective")]

    [SerializeField]
    private TextMeshProUGUI headerText;

    [SerializeField]
    private TextMeshProUGUI primaryObjectiveText;

    [SerializeField]
    private TextMeshProUGUI checklistText;


    // ADDED:
    // Separate HUD fields for one side objective.
    [Header("Side Objective")]

    [SerializeField]
    private TextMeshProUGUI sideHeaderText;

    [SerializeField]
    private TextMeshProUGUI sideObjectiveText;


    [Header("Optional System Status")]

    [SerializeField]
    private TextMeshProUGUI statusText;


    private void Start()
    {
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


        // ADDED:
        // Find only one active MAIN objective.
        ObjectiveData mainObjective =
            GetCurrentObjective(
                ObjectiveDisplayType.Main
            );


        // ADDED:
        // Find only one active SIDE objective.
        ObjectiveData sideObjective =
            GetCurrentObjective(
                ObjectiveDisplayType.Side
            );


        if (headerText != null)
        {
            headerText.text =
                "MAIN OBJECTIVE";
        }


        if (primaryObjectiveText != null)
        {
            if (mainObjective != null)
            {
                primaryObjectiveText.text =
                    FormatObjective(
                        mainObjective
                    );
            }
            else
            {
                primaryObjectiveText.text =
                    "MISSION COMPLETE";
            }
        }


        if (checklistText != null)
        {
            // ADDED:
            // Requirements now only belong to the
            // currently displayed main objective.
            checklistText.text =
                BuildRequirements(
                    mainObjective
                );
        }


        if (sideHeaderText != null)
        {
            sideHeaderText.text =
                "SIDE OBJECTIVE";


            // ADDED:
            // Hide the entire side header when there
            // is no current side objective.
            sideHeaderText.gameObject
                .SetActive(
                    sideObjective != null
                );
        }


        if (sideObjectiveText != null)
        {
            if (sideObjective != null)
            {
                sideObjectiveText.gameObject
                    .SetActive(
                        true
                    );


                sideObjectiveText.text =
                    FormatObjective(
                        sideObjective
                    );
            }
            else
            {
                sideObjectiveText.gameObject
                    .SetActive(
                        false
                    );
            }
        }


        if (statusText != null)
        {
            if (mainObjective != null)
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


    // ADDED:
    // Returns only the FIRST unlocked and incomplete
    // objective matching the requested type.
    //
    // This prevents every active objective from
    // appearing on screen simultaneously.
    private ObjectiveData GetCurrentObjective(
        ObjectiveDisplayType type)
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


            if (obj.displayType !=
                type)
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


    // ADDED:
    // Displays only requirement objectives belonging
    // to the current Main objective.
    private string BuildRequirements(
        ObjectiveData mainObjective)
    {
        if (mainObjective == null)
        {
            return "";
        }


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


            if (obj.displayType !=
                ObjectiveDisplayType.Requirement)
            {
                continue;
            }


            // ADDED:
            // Don't show requirements belonging
            // to another main objective.
            if (obj.parentObjective !=
                mainObjective)
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
                builder.Append(
                    "<color=#62D8FF>✓</color> "
                );


                builder.Append(
                    "<color=#8299A6>"
                );


                builder.Append(
                    FormatObjective(
                        obj
                    )
                );


                builder.AppendLine(
                    "</color>"
                );
            }
            else
            {
                builder.Append(
                    "<color=#EAF7FF>□</color> "
                );


                builder.AppendLine(
                    FormatObjective(
                        obj
                    )
                );
            }
        }


        return builder.ToString();
    }


    // ADDED:
    // Automatically adds progress numbers when
    // Required Progress is greater than one.
    //
    // Example:
    // Locate Military Bases 1/3
    private string FormatObjective(
        ObjectiveData objective)
    {
        if (objective == null)
        {
            return "";
        }


        if (!objective.UsesProgress)
        {
            return
                objective.objectiveTitle;
        }


        return
            objective.objectiveTitle +
            "  " +
            objective.currentProgress +
            "/" +
            objective.requiredProgress;
    }
}