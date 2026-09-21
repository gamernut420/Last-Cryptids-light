using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossTeleportVFXController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Renderer[] bossRenderers;
    [SerializeField] private GameObject teleportVfxPrefab;
    [SerializeField] private Transform effectOrigin; // optional, put at boss feet

    [Header("Teleport Visuals")]
    [SerializeField] private Color teleportColor = new Color(0.325f, 0.84f, 1f);
    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private float vanishDelay = 0.1f;
    [SerializeField] private float reappearDelay = 0.1f;
    [SerializeField] private float effectScale = 1.5f;

    [Header("Teleport Logic")]
    [SerializeField] private bool disableRenderersDuringTeleport = true;
    [SerializeField] private bool useAgentWarp = true;

    private bool isTeleporting = false;

    private class MaterialState
    {
        public Material mat;
        public Color baseColor;
        public Color emissionColor;
        public bool hasBaseColor;
        public bool hasEmission;
    }

    private readonly List<MaterialState> cachedMaterials = new List<MaterialState>();

    private void Awake()
    {
        CacheMaterials();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (effectOrigin == null)
            effectOrigin = transform;
    }

    private void CacheMaterials()
    {
        cachedMaterials.Clear();

        foreach (Renderer rend in bossRenderers)
        {
            if (rend == null) continue;

            Material[] mats = rend.materials;

            foreach (Material mat in mats)
            {
                if (mat == null) continue;

                MaterialState state = new MaterialState();
                state.mat = mat;

                if (mat.HasProperty("_BaseColor"))
                {
                    state.hasBaseColor = true;
                    state.baseColor = mat.GetColor("_BaseColor");
                }
                else if (mat.HasProperty("_Color"))
                {
                    state.hasBaseColor = true;
                    state.baseColor = mat.GetColor("_Color");
                }

                if (mat.HasProperty("_EmissionColor"))
                {
                    state.hasEmission = true;
                    state.emissionColor = mat.GetColor("_EmissionColor");
                }

                cachedMaterials.Add(state);
            }
        }
    }

    public void TeleportTo(Vector3 destination)
    {
        if (!isTeleporting)
            StartCoroutine(TeleportRoutine(destination));
    }

    private IEnumerator TeleportRoutine(Vector3 destination)
    {
        isTeleporting = true;

        Vector3 startPos = transform.position;

        SpawnTeleportEffect(startPos);

        yield return StartCoroutine(FlashToColor(teleportColor, flashDuration, 3f));

        yield return new WaitForSeconds(vanishDelay);

        if (disableRenderersDuringTeleport)
            SetBossVisible(false);

        yield return null;

        if (agent != null && useAgentWarp)
        {
            agent.Warp(destination);
        }
        else
        {
            transform.position = destination;
        }

        SpawnTeleportEffect(destination);

        yield return new WaitForSeconds(reappearDelay);

        if (disableRenderersDuringTeleport)
            SetBossVisible(true);

        yield return StartCoroutine(FlashBack(flashDuration, 3f));

        isTeleporting = false;
    }

    private void SpawnTeleportEffect(Vector3 worldPos)
    {
        if (teleportVfxPrefab == null) return;

        Vector3 pos = worldPos;

        if (effectOrigin != null)
            pos = new Vector3(worldPos.x, effectOrigin.position.y, worldPos.z);

        GameObject fx = Instantiate(teleportVfxPrefab, pos, Quaternion.identity);
        fx.transform.localScale = Vector3.one * effectScale;
    }

    private void SetBossVisible(bool visible)
    {
        foreach (Renderer rend in bossRenderers)
        {
            if (rend != null)
                rend.enabled = visible;
        }
    }

    private IEnumerator FlashToColor(Color targetColor, float duration, float emissionBoost)
    {
        float t = 0f;

        while (t < duration)
        {
            float lerp = t / duration;

            foreach (MaterialState state in cachedMaterials)
            {
                if (state.mat == null) continue;

                if (state.hasBaseColor)
                {
                    Color start = state.baseColor;
                    Color c = Color.Lerp(start, targetColor, lerp);

                    if (state.mat.HasProperty("_BaseColor"))
                        state.mat.SetColor("_BaseColor", c);
                    else if (state.mat.HasProperty("_Color"))
                        state.mat.SetColor("_Color", c);
                }

                if (state.hasEmission)
                {
                    Color e = Color.Lerp(state.emissionColor, targetColor * emissionBoost, lerp);
                    state.mat.SetColor("_EmissionColor", e);
                    state.mat.EnableKeyword("_EMISSION");
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        foreach (MaterialState state in cachedMaterials)
        {
            if (state.mat == null) continue;

            if (state.hasBaseColor)
            {
                if (state.mat.HasProperty("_BaseColor"))
                    state.mat.SetColor("_BaseColor", targetColor);
                else if (state.mat.HasProperty("_Color"))
                    state.mat.SetColor("_Color", targetColor);
            }

            if (state.hasEmission)
            {
                state.mat.SetColor("_EmissionColor", targetColor * emissionBoost);
                state.mat.EnableKeyword("_EMISSION");
            }
        }
    }

    private IEnumerator FlashBack(float duration, float emissionBoost)
    {
        float t = 0f;

        while (t < duration)
        {
            float lerp = t / duration;

            foreach (MaterialState state in cachedMaterials)
            {
                if (state.mat == null) continue;

                if (state.hasBaseColor)
                {
                    Color current;

                    if (state.mat.HasProperty("_BaseColor"))
                        current = state.mat.GetColor("_BaseColor");
                    else
                        current = state.mat.GetColor("_Color");

                    Color c = Color.Lerp(current, state.baseColor, lerp);

                    if (state.mat.HasProperty("_BaseColor"))
                        state.mat.SetColor("_BaseColor", c);
                    else if (state.mat.HasProperty("_Color"))
                        state.mat.SetColor("_Color", c);
                }

                if (state.hasEmission)
                {
                    Color currentE = state.mat.GetColor("_EmissionColor");
                    Color e = Color.Lerp(currentE, state.emissionColor, lerp);
                    state.mat.SetColor("_EmissionColor", e);
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        foreach (MaterialState state in cachedMaterials)
        {
            if (state.mat == null) continue;

            if (state.hasBaseColor)
            {
                if (state.mat.HasProperty("_BaseColor"))
                    state.mat.SetColor("_BaseColor", state.baseColor);
                else if (state.mat.HasProperty("_Color"))
                    state.mat.SetColor("_Color", state.baseColor);
            }

            if (state.hasEmission)
                state.mat.SetColor("_EmissionColor", state.emissionColor);
        }
    }
}