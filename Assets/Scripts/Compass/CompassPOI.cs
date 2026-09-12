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


    // Static list containing every active CompassPOI in the scene.
    // This lets the compass find POIs without repeatedly using
    // FindObjectsByType every frame.
    private static readonly List<CompassPOI> activePOIs =
        new List<CompassPOI>();


    public static IReadOnlyList<CompassPOI> ActivePOIs =>
        activePOIs;

    public string POIName => poiName;

    public Sprite Icon => icon;

    public bool ShowDistance => showDistance;

    public bool ShowName => showName;


    private void OnEnable()
    {
        // Register this POI when it becomes active.
        if (!activePOIs.Contains(this))
        {
            activePOIs.Add(this);
        }
    }


    private void OnDisable()
    {
        // Remove it when disabled or destroyed.
        activePOIs.Remove(this);
    }
}