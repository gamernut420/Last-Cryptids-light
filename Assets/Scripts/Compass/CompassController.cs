using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompassController : MonoBehaviour
{
    [Header("Player References")]

    [Tooltip(
        "Camera whose rotation controls the compass. " +
        "If left empty, Camera.main will be used."
    )]
    [SerializeField]
    private Transform playerCamera;

    [Tooltip(
        "Transform used to calculate distance to POIs. " +
        "Usually the player's transform."
    )]
    [SerializeField]
    private Transform player;


    [Header("Compass UI")]

    [Tooltip(
        "RectTransform containing compass ticks and direction labels."
    )]
    [SerializeField]
    private RectTransform compassArea;

    [Tooltip(
        "RectTransform where POI markers will be created."
    )]
    [SerializeField]
    private RectTransform poiContainer;


    [Header("Compass Prefabs")]

    [SerializeField]
    private GameObject tickPrefab;

    [SerializeField]
    private CompassPOIMarkerUI poiMarkerPrefab;


    [Header("Compass Settings")]

    [Tooltip(
        "How many degrees are visible across the entire compass."
    )]
    [SerializeField]
    [Range(30f, 180f)]
    private float visibleDegrees = 120f;

    [Tooltip(
        "Degrees between minor compass ticks."
    )]
    [SerializeField]
    [Range(1, 45)]
    private int tickSpacing = 5;


    // ADDED FOR SIGNAL INTERFERENCE:
    [Header("Signal Interference")]

    [Tooltip(
        "How far POI markers can jitter horizontally " +
        "while the compass signal is disrupted."
    )]
    [SerializeField]
    [Min(0f)]
    private float signalJitterAmount = 8f;

    [Tooltip(
        "Speed of signal interference jitter."
    )]
    [SerializeField]
    [Min(0.1f)]
    private float signalJitterSpeed = 18f;


    // ADDED FOR SIGNAL INTERFERENCE:
    private bool signalDisrupted;


    // Stores every compass tick so we can reposition them
    // without recreating UI objects every frame.
    private readonly List<CompassTick>
        compassTicks =
            new List<CompassTick>();


    // Associates each world POI with its UI marker.
    private readonly Dictionary<
        CompassPOI,
        CompassPOIMarkerUI>
        poiMarkers =
            new Dictionary<
                CompassPOI,
                CompassPOIMarkerUI>();


    private void Awake()
    {
        // Use the tagged Main Camera automatically when
        // a camera wasn't manually assigned.
        if (playerCamera == null &&
            Camera.main != null)
        {
            playerCamera =
                Camera.main.transform;
        }


        // Try to find the tagged Player automatically.
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindWithTag("Player");


            if (playerObject != null)
            {
                player =
                    playerObject.transform;
            }
        }


        CreateCompassTicks();
    }


    private void Start()
    {
        RefreshPOIs();
    }


    private void Update()
    {
        if (playerCamera == null)
        {
            return;
        }


        UpdateCompass();

        UpdatePOIs();
    }


    /// <summary>
    /// Creates the compass markings one time.
    ///
    /// Generates the complete 360 degrees and repositions
    /// the visible portion based on camera rotation.
    /// </summary>
    private void CreateCompassTicks()
    {
        if (tickPrefab == null ||
            compassArea == null)
        {
            return;
        }


        for (
            int angle = 0;
            angle < 360;
            angle += tickSpacing)
        {
            GameObject newTick =
                Instantiate(
                    tickPrefab,
                    compassArea
                );


            RectTransform rect =
                newTick.GetComponent<RectTransform>();


            TextMeshProUGUI text =
                newTick.GetComponentInChildren<
                    TextMeshProUGUI>();


            CompassTick tick =
                new CompassTick();


            tick.angle =
                angle;

            tick.rect =
                rect;

            tick.gameObject =
                newTick;


            if (text != null)
            {
                text.text =
                    GetDirectionName(angle);
            }


            compassTicks.Add(tick);
        }
    }


    /// <summary>
    /// Updates tick positions based on camera rotation.
    /// </summary>
    private void UpdateCompass()
    {
        if (compassArea == null)
        {
            return;
        }


        float cameraYaw =
            playerCamera.eulerAngles.y;


        float width =
            compassArea.rect.width;


        float halfVisibleDegrees =
            visibleDegrees * 0.5f;


        for (
            int i = 0;
            i < compassTicks.Count;
            i++)
        {
            CompassTick tick =
                compassTicks[i];


            float angleDifference =
                Mathf.DeltaAngle(
                    cameraYaw,
                    tick.angle
                );


            bool visible =
                Mathf.Abs(angleDifference)
                <= halfVisibleDegrees;


            tick.gameObject.SetActive(
                visible
            );


            if (!visible)
            {
                continue;
            }


            float normalizedPosition =
                angleDifference /
                halfVisibleDegrees;


            float xPosition =
                normalizedPosition *
                (width * 0.5f);


            Vector2 anchoredPosition =
                tick.rect.anchoredPosition;


            anchoredPosition.x =
                xPosition;


            tick.rect.anchoredPosition =
                anchoredPosition;
        }
    }


    /// <summary>
    /// Creates UI markers for active POIs.
    /// </summary>
    private void RefreshPOIs()
    {
        IReadOnlyList<CompassPOI> pois =
            CompassPOI.ActivePOIs;


        for (
            int i = 0;
            i < pois.Count;
            i++)
        {
            AddPOI(
                pois[i]
            );
        }
    }


    /// <summary>
    /// Creates a UI marker for a world POI.
    /// </summary>
    private void AddPOI(
        CompassPOI poi)
    {
        if (poi == null ||
            poiMarkerPrefab == null ||
            poiContainer == null)
        {
            return;
        }


        if (poiMarkers.ContainsKey(poi))
        {
            return;
        }


        CompassPOIMarkerUI marker =
            Instantiate(
                poiMarkerPrefab,
                poiContainer
            );


        marker.Initialize(poi);


        poiMarkers.Add(
            poi,
            marker
        );
    }


    /// <summary>
    /// Updates all POI markers.
    /// </summary>
    private void UpdatePOIs()
    {
        IReadOnlyList<CompassPOI> activePOIs =
            CompassPOI.ActivePOIs;


        // Add newly activated POIs.
        for (
            int i = 0;
            i < activePOIs.Count;
            i++)
        {
            if (!poiMarkers.ContainsKey(
                    activePOIs[i]))
            {
                AddPOI(
                    activePOIs[i]
                );
            }
        }


        List<CompassPOI> removeList =
            null;


        foreach (
            KeyValuePair<
                CompassPOI,
                CompassPOIMarkerUI>
            pair in poiMarkers)
        {
            CompassPOI poi =
                pair.Key;

            CompassPOIMarkerUI marker =
                pair.Value;


            // ADDED:
            // Remove the marker not only when the world
            // object is destroyed, but also when the POI
            // has intentionally been removed from ActivePOIs.
            if (poi == null ||
                !CompassPOI.IsActivePOI(poi))
            {
                if (marker != null)
                {
                    Destroy(
                        marker.gameObject
                    );
                }


                if (removeList == null)
                {
                    removeList =
                        new List<CompassPOI>();
                }


                removeList.Add(
                    pair.Key
                );


                continue;
            }


            UpdatePOIMarker(
                poi,
                marker
            );
        }


        if (removeList != null)
        {
            for (
                int i = 0;
                i < removeList.Count;
                i++)
            {
                poiMarkers.Remove(
                    removeList[i]
                );
            }
        }
    }


    /// <summary>
    /// Determines where one POI appears on the compass.
    /// </summary>
    private void UpdatePOIMarker(
        CompassPOI poi,
        CompassPOIMarkerUI marker)
    {
        if (player == null ||
            marker == null ||
            poiContainer == null)
        {
            return;
        }


        Vector3 direction =
            poi.transform.position -
            player.position;


        // Ignore vertical height differences.
        direction.y = 0f;


        if (direction.sqrMagnitude <= 0.001f)
        {
            marker.gameObject.SetActive(
                false
            );

            return;
        }


        float poiAngle =
            Mathf.Atan2(
                direction.x,
                direction.z
            ) *
            Mathf.Rad2Deg;


        if (poiAngle < 0f)
        {
            poiAngle += 360f;
        }


        float cameraYaw =
            playerCamera.eulerAngles.y;


        float angleDifference =
            Mathf.DeltaAngle(
                cameraYaw,
                poiAngle
            );


        float halfVisibleDegrees =
            visibleDegrees * 0.5f;


        bool visible =
            Mathf.Abs(angleDifference)
            <= halfVisibleDegrees;


        marker.gameObject.SetActive(
            visible
        );


        if (!visible)
        {
            return;
        }


        float width =
            poiContainer.rect.width;


        float normalizedPosition =
            angleDifference /
            halfVisibleDegrees;


        float xPosition =
            normalizedPosition *
            (width * 0.5f);


        // ADDED FOR SIGNAL INTERFERENCE:
        // Adds localized jitter to POI markers without
        // making the cardinal compass directions unusable.
        if (signalDisrupted)
        {
            // ADDED FOR SIGNAL INTERFERENCE:
            // Use world position as a stable noise offset.
            // This avoids deprecated GetInstanceID /
            // GetEntityId conversions in newer Unity versions.
            float poiNoiseOffset =
                (poi.transform.position.x * 0.01f) +
                (poi.transform.position.z * 0.013f);


            float noise =
                Mathf.PerlinNoise(
                    Time.time *
                    signalJitterSpeed,

                    poiNoiseOffset
                );


            float jitter =
                (noise * 2f - 1f) *
                signalJitterAmount;


            xPosition +=
                jitter;
        }


        RectTransform markerRect =
            marker.GetComponent<RectTransform>();


        Vector2 position =
            markerRect.anchoredPosition;


        position.x =
            xPosition;


        markerRect.anchoredPosition =
            position;


        float distance =
            Vector3.Distance(
                player.position,
                poi.transform.position
            );


        marker.UpdateDistance(
            distance
        );
    }


    // ADDED FOR SIGNAL INTERFERENCE:
    // Enemy/environment systems can call:
    //
    // SetSignalDisrupted(true)
    //
    // when interference begins, and false when it ends.
    public void SetSignalDisrupted(
        bool disrupted)
    {
        signalDisrupted =
            disrupted;
    }


    // ADDED:
    // Useful for debugging or other gameplay systems.
    public bool IsSignalDisrupted()
    {
        return signalDisrupted;
    }


    /// <summary>
    /// Converts compass angles into readable labels.
    /// </summary>
    private string GetDirectionName(
        int angle)
    {
        switch (angle)
        {
            case 0:
                return "N";

            case 45:
                return "NE";

            case 90:
                return "E";

            case 135:
                return "SE";

            case 180:
                return "S";

            case 225:
                return "SW";

            case 270:
                return "W";

            case 315:
                return "NW";

            default:
                return string.Empty;
        }
    }


    /// <summary>
    /// Internal data for generated compass ticks.
    /// </summary>
    private class CompassTick
    {
        public float angle;

        public RectTransform rect;

        public GameObject gameObject;
    }
}