using System.Collections;
using UnityEngine;

public class NoiseLure : ThrowableGadget
{
    [Header("----- Lure Settings -----")]
    [SerializeField][Min(0)] float EffectDuration = 2f;
    [SerializeField][Min(0)] float EffectRange = 10f;

    //AudioSource SoundSource;
    bool IsActive = false;

    private void OnValidate()
    {
        //if(!TryGetComponent<AudioSource>(out SoundSource))
        //{
        //    SoundSource = gameObject.AddComponent<AudioSource>();
        //}
    }

    public override void Impact(RaycastHit hit)
    {
        transform.position = hit.point;

        transform.localRotation = Quaternion.Euler(0, transform.localEulerAngles.y, 0);

        StartCoroutine(PlayLure());
    }

    private void Update()
    {
        if (IsActive)
        {
            Debug.Log("Made noise");
            NoiseManager.MakeNoise(transform.position, EffectRange);
        }
    }

    IEnumerator PlayLure()
    {
        IsActive = true;

        yield return new WaitForSeconds(EffectDuration);

        IsActive = false;

        Destroy(gameObject);
    }
}
