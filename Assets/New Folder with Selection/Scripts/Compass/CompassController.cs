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
    /// We generate the complete 360 degrees and simply
    /// reposition/hide the appropriate markings while rotating.
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
                newTick.GetComponent<
                    RectTransform>();


            TextMeshProUGUI text =
                newTick.GetComponentInChildren<
                    TextMeshProUGUI>();


            CompassTick tick =
                new CompassTick();

            tick.angle = angle;
            tick.rect = rect;
            tick.gameObject = newTick;


            // Only certain angles receive direction text.
            if (text != null)
            {
                text.text =
                    GetDirectionName(angle);
            }


            compassTicks.Add(tick);
        }
    }


    /// <summary>
    /// Updates all tick positions based on the camera's
    /// current Y rotation.
    /// </summary>
    private void UpdateCompass()
    {
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


            // DeltaAngle handles wrapping automatically.
            //
            // Example:
            //
            // Camera = 359 degrees
            // Marker = 1 degree
            //
            // Delta = 2 instead of -358.
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


            // Convert angular difference into normalized
            // horizontal screen position.
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
    /// Looks at the active world POIs and creates
    /// UI markers for any POIs that don't currently
    /// have one.
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
    /// Updates every compass POI marker.
    /// </summary>
    private void UpdatePOIs()
    {
        // Pick up any POIs that became active after Start().
        IReadOnlyList<CompassPOI> activePOIs =
            CompassPOI.ActivePOIs;


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


        // Make a temporary cleanup list only when necessary.
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


            if (poi == null)
            {
                if (removeList == null)
                {
                    removeList =
                        new List<CompassPOI>();
                }

                removeList.Add(poi);

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
    /// Determines where one POI should appear
    /// on the compass.
    /// </summary>
    private void UpdatePOIMarker(
        CompassPOI poi,
        CompassPOIMarkerUI marker)
    {
        if (player == null ||
            marker == null)
        {
            return;
        }


        Vector3 direction =
            poi.transform.position -
            player.position;


        // Remove vertical difference.
        //
        // A POI on a mountain should still point toward
        // the mountain horizontally instead of affecting
        // the compass direction.
        direction.y = 0f;


        if (direction.sqrMagnitude <= 0.001f)
        {
            marker.gameObject.SetActive(
                false
            );

            return;
        }


        // Convert world direction into an angle.
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


        RectTransform markerRect =
            marker.GetComponent<
                RectTransform>();


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


    /// <summary>
    /// Converts compass angles into readable labels.
    ///
    /// Minor angles return an empty string,
    /// allowing the tick itself to remain visible.
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
    /// Internal data used for generated compass ticks.
    ///
    /// This avoids needing a separate MonoBehaviour
    /// on every minor tick.
    /// </summary>
    private class CompassTick
    {
        public float angle;

        public RectTransform rect;

        public GameObject gameObject;
    }
}