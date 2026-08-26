using UnityEngine;
using TMPro;

public class MissionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI missionObjectiveText;

    [Header("Target Reference")]
    [SerializeField] GameObject beacon;
    [SerializeField] RescueBeacon rescueBeaconScript;

    private void Awake()
    {
        beacon = GameObject.FindWithTag("Beacon");

        if(beacon != null )
        {
            rescueBeaconScript = beacon.GetComponent<RescueBeacon>();
        }
    }
    void Update()
    {
        if(rescueBeaconScript != null && missionObjectiveText != null)
        {
            if(!rescueBeaconScript.isRepaired)
            {
                missionObjectiveText.text = "<b>Current Objactive:</b>\n" +
                                             "Collact required parts for the beacon.\n\n" +
                                             rescueBeaconScript.ScreenMessage();
            }
            else
            {
                missionObjectiveText.text = "<b>Current Objective:</b>\n" +
                                            "Beacon active! Survive the enemy waves until extraction arrives.";
            }
        }
        
    }
}
