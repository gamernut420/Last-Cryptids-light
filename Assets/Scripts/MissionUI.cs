using UnityEngine;
using TMPro;

public class MissionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI missionObjectiveText;

    [Header("Target Reference")]
    [SerializeField] RescueBeacon rescueBeacon;
    
    void Update()
    {
        if(rescueBeacon != null && missionObjectiveText != null)
        {
            if(!rescueBeacon.isRepaired)
            {
                missionObjectiveText.text = "<b>Current Objactive:</b>\n" +
                                             "Collact required parts for the beacon.\n\n" +
                                             rescueBeacon.ScreenMessage();
            }
            else
            {
                missionObjectiveText.text = "<b>Current Objective:</b>\n" +
                                            "Beacon active! Survive the enemy waves until extraction arrives.";
            }
        }
        
    }
}
