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
    [SerializeField] private Transform effectOrigin;


    [Header("Teleport Visuals")]
    [SerializeField]
    private Color teleportColor =
        new Color(
            0.325f,
            0.84f,
            1f
        );

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


    private readonly List<MaterialState>
        cachedMaterials =
            new List<MaterialState>();


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (agent == null)
        {
            agent =
                GetComponent<NavMeshAgent>();
        }


        if (effectOrigin == null)
        {
            effectOrigin =
                transform;
        }


        CacheMaterials();
    }


    // =========================================================
    // CACHE MATERIALS
    // =========================================================

    private void CacheMaterials()
    {
        cachedMaterials.Clear();


        if (bossRenderers == null)
        {
            return;
        }


        foreach (Renderer rend in bossRenderers)
        {
            if (rend == null)
            {
                continue;
            }


            Material[] mats =
                rend.materials;


            foreach (Material mat in mats)
            {
                if (mat == null)
                {
                    continue;
                }


                MaterialState state =
                    new MaterialState();


                state.mat =
                    mat;


                if (mat.HasProperty(
                    "_BaseColor"))
                {
                    state.hasBaseColor =
                        true;


                    state.baseColor =
                        mat.GetColor(
                            "_BaseColor"
                        );
                }
                else if (mat.HasProperty(
                    "_Color"))
                {
                    state.hasBaseColor =
                        true;


                    state.baseColor =
                        mat.GetColor(
                            "_Color"
                        );
                }


                if (mat.HasProperty(
                    "_EmissionColor"))
                {
                    state.hasEmission =
                        true;


                    state.emissionColor =
                        mat.GetColor(
                            "_EmissionColor"
                        );
                }


                cachedMaterials.Add(
                    state
                );
            }
        }
    }


    // =========================================================
    // PUBLIC TELEPORT
    // =========================================================

    public void TeleportTo(
        Vector3 destination)
    {
        if (isTeleporting)
        {
            return;
        }


        StartCoroutine(
            TeleportRoutine(
                destination
            )
        );
    }


    // =========================================================
    // TELEPORT ROUTINE
    // =========================================================

    private IEnumerator TeleportRoutine(
        Vector3 destination)
    {
        isTeleporting =
            true;


        Vector3 startPos =
            transform.position;


        // Spawn VFX around boss's current location.
        SpawnTeleportEffect(
            startPos
        );


        // Boss changes into the same cyan color as the VFX.
        yield return StartCoroutine(
            FlashToColor(
                teleportColor,
                flashDuration,
                3f
            )
        );


        // Tiny pause while fully blue.
        yield return new WaitForSeconds(
            vanishDelay
        );


        // Hide boss.
        if (disableRenderersDuringTeleport)
        {
            SetBossVisible(
                false
            );
        }


        yield return null;


        // Move boss.
        if (agent != null &&
            useAgentWarp &&
            agent.isOnNavMesh)
        {
            agent.Warp(
                destination
            );
        }
        else
        {
            transform.position =
                destination;
        }


        // Spawn another VFX where boss arrives.
        SpawnTeleportEffect(
            destination
        );


        yield return new WaitForSeconds(
            reappearDelay
        );


        // Boss becomes visible again.
        if (disableRenderersDuringTeleport)
        {
            SetBossVisible(
                true
            );
        }


        // Fade from teleport blue back to original material.
        yield return StartCoroutine(
            FlashBack(
                flashDuration
            )
        );


        isTeleporting =
            false;
    }


    // =========================================================
    // SPAWN EFFECT
    // =========================================================

    private void SpawnTeleportEffect(
        Vector3 worldPos)
    {
        if (teleportVfxPrefab == null)
        {
            return;
        }


        Vector3 pos =
            worldPos;


        // Keep effect on the same Y level as our
        // effect origin / boss feet.
        if (effectOrigin != null)
        {
            pos =
                new Vector3(
                    worldPos.x,
                    effectOrigin.position.y,
                    worldPos.z
                );
        }


        GameObject fx =
            Instantiate(
                teleportVfxPrefab,
                pos,
                Quaternion.identity
            );


        fx.transform.localScale =
            Vector3.one *
            effectScale;
    }


    // =========================================================
    // BOSS VISIBILITY
    // =========================================================

    private void SetBossVisible(
        bool visible)
    {
        if (bossRenderers == null)
        {
            return;
        }


        foreach (Renderer rend in bossRenderers)
        {
            if (rend != null)
            {
                rend.enabled =
                    visible;
            }
        }
    }


    // =========================================================
    // FLASH TO TELEPORT COLOR
    // =========================================================

    private IEnumerator FlashToColor(
        Color targetColor,
        float duration,
        float emissionBoost)
    {
        float t =
            0f;


        while (t < duration)
        {
            float lerp =
                t / duration;


            foreach (
                MaterialState state
                in cachedMaterials)
            {
                if (state.mat == null)
                {
                    continue;
                }


                if (state.hasBaseColor)
                {
                    Color c =
                        Color.Lerp(
                            state.baseColor,
                            targetColor,
                            lerp
                        );


                    if (state.mat.HasProperty(
                        "_BaseColor"))
                    {
                        state.mat.SetColor(
                            "_BaseColor",
                            c
                        );
                    }
                    else if (
                        state.mat.HasProperty(
                            "_Color"))
                    {
                        state.mat.SetColor(
                            "_Color",
                            c
                        );
                    }
                }


                if (state.hasEmission)
                {
                    Color emission =
                        Color.Lerp(
                            state.emissionColor,
                            targetColor *
                            emissionBoost,
                            lerp
                        );


                    state.mat.SetColor(
                        "_EmissionColor",
                        emission
                    );


                    state.mat.EnableKeyword(
                        "_EMISSION"
                    );
                }
            }


            t +=
                Time.deltaTime;


            yield return null;
        }


        // Guarantee final color.
        foreach (
            MaterialState state
            in cachedMaterials)
        {
            if (state.mat == null)
            {
                continue;
            }


            if (state.hasBaseColor)
            {
                if (state.mat.HasProperty(
                    "_BaseColor"))
                {
                    state.mat.SetColor(
                        "_BaseColor",
                        targetColor
                    );
                }
                else if (
                    state.mat.HasProperty(
                        "_Color"))
                {
                    state.mat.SetColor(
                        "_Color",
                        targetColor
                    );
                }
            }


            if (state.hasEmission)
            {
                state.mat.SetColor(
                    "_EmissionColor",
                    targetColor *
                    emissionBoost
                );


                state.mat.EnableKeyword(
                    "_EMISSION"
                );
            }
        }
    }


    // =========================================================
    // FLASH BACK TO ORIGINAL MATERIAL
    // =========================================================

    private IEnumerator FlashBack(
        float duration)
    {
        float t =
            0f;


        // Remember what color we're starting from.
        List<Color> startingBaseColors =
            new List<Color>();


        List<Color> startingEmissionColors =
            new List<Color>();


        foreach (
            MaterialState state
            in cachedMaterials)
        {
            if (state.mat == null)
            {
                startingBaseColors.Add(
                    Color.white
                );


                startingEmissionColors.Add(
                    Color.black
                );


                continue;
            }


            if (state.mat.HasProperty(
                "_BaseColor"))
            {
                startingBaseColors.Add(
                    state.mat.GetColor(
                        "_BaseColor"
                    )
                );
            }
            else if (state.mat.HasProperty(
                "_Color"))
            {
                startingBaseColors.Add(
                    state.mat.GetColor(
                        "_Color"
                    )
                );
            }
            else
            {
                startingBaseColors.Add(
                    Color.white
                );
            }


            if (state.hasEmission)
            {
                startingEmissionColors.Add(
                    state.mat.GetColor(
                        "_EmissionColor"
                    )
                );
            }
            else
            {
                startingEmissionColors.Add(
                    Color.black
                );
            }
        }


        while (t < duration)
        {
            float lerp =
                t / duration;


            for (
                int i = 0;
                i < cachedMaterials.Count;
                i++)
            {
                MaterialState state =
                    cachedMaterials[i];


                if (state.mat == null)
                {
                    continue;
                }


                if (state.hasBaseColor)
                {
                    Color c =
                        Color.Lerp(
                            startingBaseColors[i],
                            state.baseColor,
                            lerp
                        );


                    if (state.mat.HasProperty(
                        "_BaseColor"))
                    {
                        state.mat.SetColor(
                            "_BaseColor",
                            c
                        );
                    }
                    else if (
                        state.mat.HasProperty(
                            "_Color"))
                    {
                        state.mat.SetColor(
                            "_Color",
                            c
                        );
                    }
                }


                if (state.hasEmission)
                {
                    Color emission =
                        Color.Lerp(
                            startingEmissionColors[i],
                            state.emissionColor,
                            lerp
                        );


                    state.mat.SetColor(
                        "_EmissionColor",
                        emission
                    );
                }
            }


            t +=
                Time.deltaTime;


            yield return null;
        }


        // Guarantee original material values.
        foreach (
            MaterialState state
            in cachedMaterials)
        {
            if (state.mat == null)
            {
                continue;
            }


            if (state.hasBaseColor)
            {
                if (state.mat.HasProperty(
                    "_BaseColor"))
                {
                    state.mat.SetColor(
                        "_BaseColor",
                        state.baseColor
                    );
                }
                else if (
                    state.mat.HasProperty(
                        "_Color"))
                {
                    state.mat.SetColor(
                        "_Color",
                        state.baseColor
                    );
                }
            }


            if (state.hasEmission)
            {
                state.mat.SetColor(
                    "_EmissionColor",
                    state.emissionColor
                );
            }
        }
    }
}