using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompassPOIMarkerUI : MonoBehaviour
{
    [Header("UI References")]

    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI distanceText;


    private CompassPOI trackedPOI;


    public CompassPOI TrackedPOI =>
        trackedPOI;


    public void Initialize(CompassPOI poi)
    {
        trackedPOI = poi;

        if (trackedPOI == null)
        {
            return;
        }


        // Apply the icon stored on the POI.
        if (iconImage != null)
        {
            iconImage.sprite =
                trackedPOI.Icon;

            iconImage.enabled =
                trackedPOI.Icon != null;
        }


        // Set the POI's display name.
        if (nameText != null)
        {
            nameText.text =
                trackedPOI.POIName;

            nameText.gameObject.SetActive(
                trackedPOI.ShowName
            );
        }


        if (distanceText != null)
        {
            distanceText.gameObject.SetActive(
                trackedPOI.ShowDistance
            );
        }
    }


    public void UpdateDistance(
        float distance)
    {
        if (distanceText == null ||
            trackedPOI == null ||
            !trackedPOI.ShowDistance)
        {
            return;
        }


        // Round so the player sees:
        //
        // 128m
        //
        // instead of:
        //
        // 128.437m
        distanceText.text =
            Mathf.RoundToInt(distance) +
            "m";
    }
}