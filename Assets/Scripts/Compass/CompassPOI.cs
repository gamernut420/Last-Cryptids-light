using System.Collections.Generic;
using UnityEngine;

public class CompassPOI : MonoBehaviour
{
    [Header("Compass Information")]

    [SerializeField]
    private string poiName = "Point of Interest";

    [SerializeField]
    private Sprite icon;


    [Header("Display Settings")]

    [SerializeField]
    private bool showDistance = true;

    [SerializeField]
    private bool showName = true;


    // ADDED FOR OBJECTIVES:
    // When enabled, this POI will normally stay hidden
    // until ObjectiveManager activates it.
    [Header("Objective Settings")]

    [SerializeField]
    private bool controlledByObjective = false;

    // ADDED FOR OBJECTIVES:
    // Usually leave this false for objective-only markers.
    [SerializeField]
    private bool startVisibleWhenObjectiveControlled = false;


    // Stores every compass POI that should currently
    // appear on the compass.
    private static readonly List<CompassPOI> activePOIs =
        new List<CompassPOI>();


    // ADDED:
    // Tracks whether this specific POI is currently
    // registered with the compass.
    private bool compassActive;


    public static IReadOnlyList<CompassPOI> ActivePOIs =>
        activePOIs;

    public string POIName =>
        poiName;

    public Sprite Icon =>
        icon;

    public bool ShowDistance =>
        showDistance;

    public bool ShowName =>
        showName;


    // ADDED FOR OBJECTIVES:
    public bool ControlledByObjective =>
        controlledByObjective;


    // ADDED:
    public bool CompassActive =>
        compassActive;


    private void OnEnable()
    {
        // ADDED:
        // Normal POIs appear immediately.
        //
        // Objective-controlled POIs can remain hidden
        // until ObjectiveManager activates them.
        compassActive =
            !controlledByObjective ||
            startVisibleWhenObjectiveControlled;


        if (compassActive)
        {
            RegisterPOI();
        }
    }


    private void OnDisable()
    {
        UnregisterPOI();
    }


    // ADDED:
    // Allows ObjectiveManager or another gameplay system
    // to dynamically show/hide this marker without
    // disabling the actual world object.
    public void SetCompassActive(bool active)
    {
        compassActive = active;


        if (compassActive)
        {
            RegisterPOI();
        }
        else
        {
            UnregisterPOI();
        }
    }


    // ADDED:
    private void RegisterPOI()
    {
        if (!activePOIs.Contains(this))
        {
            activePOIs.Add(this);
        }
    }


    // ADDED:
    private void UnregisterPOI()
    {
        activePOIs.Remove(this);
    }


    // ADDED:
    // CompassController uses this to determine whether an
    // existing marker should still be displayed.
    public static bool IsActivePOI(CompassPOI poi)
    {
        if (poi == null)
        {
            return false;
        }

        return activePOIs.Contains(poi);
    }
}