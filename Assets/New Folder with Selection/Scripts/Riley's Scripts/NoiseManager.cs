using UnityEngine;
using System;

public class NoiseManager : MonoBehaviour
{
    public static event Action<Vector3, float> OnNoiseMade;

    public static void MakeNoise(Vector3 sourcePosition, float loudnessRange)
    {
        OnNoiseMade?.Invoke(sourcePosition, loudnessRange);
    }
}
