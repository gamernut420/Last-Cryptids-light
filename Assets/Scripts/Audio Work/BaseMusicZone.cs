using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(Collider))]
public class BaseMusicZone : MonoBehaviour
{
    [Header("Audio References")]
    [SerializeField] private AmbiencePlaylist ambiencePlaylist;
    [SerializeField] private AudioSource baseMusicSource;

    [Header("Transition")]
    [Min(0.01f)]
    [SerializeField] private float fadeDuration = 5f;
    [Range(0f, 1f)]
    [SerializeField] private float baseMusicVolume = 0.5f;

    private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();
    private Coroutine fadeCoroutine;
    private bool baseMusicPaused;

    private void Reset()
    {
        Collider zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider zoneCollider = GetComponent<Collider>();

        if (!zoneCollider.isTrigger)
        {
            Debug.LogWarning("BaseMusicZone: The Collider must have Is Trigger enabled.", this);
        }

        if (baseMusicSource != null)
        {
            baseMusicSource.playOnAwake = false;
            baseMusicSource.loop = true;
            baseMusicSource.spatialBlend = 0f;
            baseMusicSource.volume = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (baseMusicSource == null)
        {
            return;
        }

        bool gameIsPaused = Time.timeScale <= 0f;

        if (gameIsPaused && !baseMusicPaused && baseMusicSource.isPlaying)
        {
            baseMusicSource.Pause();
            baseMusicPaused = true;
        }
        else if (!gameIsPaused && baseMusicPaused)
        {
            baseMusicSource.UnPause();
            baseMusicPaused = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || !playerColliders.Add(other))
        {
            return;
        }

        if (playerColliders.Count == 1)
        {
            FadeToBaseMusic();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerColliders.Remove(other))
        {
            return;
        }

        if (playerColliders.Count == 0)
        {
            FadeToAmbience();
        }
    }

    public void FadeToBaseMusic()
    {
        if (ambiencePlaylist == null || baseMusicSource == null)
        {
            Debug.LogWarning("BaseMusicZone: Assign both audio references.", this);
            return;
        }

        if (baseMusicSource.clip == null)
        {
            Debug.LogWarning("BaseMusicZone: The Base Music AudioSource has no clip.", this);
            return;
        }

        if (!baseMusicSource.isPlaying)
        {
            baseMusicSource.Play();
        }

        StartFade(0f, baseMusicVolume, false);
    }

    public void FadeToAmbience()
    {
        if (ambiencePlaylist == null || baseMusicSource == null)
        {
            Debug.LogWarning("BaseMusicZone: Assign both audio references.", this);
            return;
        }

        StartFade(1f, 0f, true);
    }

    private void StartFade(float ambienceTarget, float baseMusicTarget, bool stopBaseMusicWhenDone)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(
            FadeRoutine(ambienceTarget, baseMusicTarget, stopBaseMusicWhenDone));
    }

    private IEnumerator FadeRoutine(
        float ambienceTarget,
        float baseMusicTarget,
        bool stopBaseMusicWhenDone)
    {
        float startingAmbienceVolume = ambiencePlaylist.GetTransitionVolume();
        float startingBaseMusicVolume = baseMusicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeDuration);

            ambiencePlaylist.SetTransitionVolume(
                Mathf.Lerp(startingAmbienceVolume, ambienceTarget, progress));

            baseMusicSource.volume =
                Mathf.Lerp(startingBaseMusicVolume, baseMusicTarget, progress);

            yield return null;
        }

        ambiencePlaylist.SetTransitionVolume(ambienceTarget);
        baseMusicSource.volume = baseMusicTarget;

        if (stopBaseMusicWhenDone && baseMusicSource.volume <= 0f)
        {
            baseMusicSource.Stop();
        }

        fadeCoroutine = null;
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") || other.transform.root.CompareTag("Player");
    }
}
