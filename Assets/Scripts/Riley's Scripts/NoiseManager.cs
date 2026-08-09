using UnityEngine;
using System;

public static class NoiseManager
{
    public enum NoiseType { Footstep, Gunshot, Distraction }

    public static event Action<Vector3, float, NoiseType> OnNoiseMade;

    public static void ReportNoise(Vector3 location, float radius, NoiseType type)
    {
        OnNoiseMade?.Invoke(location, radius, type);
    }
}
